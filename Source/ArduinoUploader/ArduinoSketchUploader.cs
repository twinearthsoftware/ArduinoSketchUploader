using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using ArduinoUploader.ArduinoSTK500Protocol;
using ArduinoUploader.ArduinoSTK500Protocol.CommandConstants;
using ArduinoUploader.ArduinoSTK500Protocol.HardwareConstants;
using ArduinoUploader.ArduinoSTK500Protocol.Messages;
using IntelHexFormatReader;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader
{
    /// <summary>
    /// The ArduinoLibCSharp SketchUploader can upload a compiled (Intel) HEX file directly to an attached Arduino (UNO).
    /// 
    /// This code was heavily inspired by avrdude's STK500 implementation.
    /// </summary>
    public class ArduinoSketchUploader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ArduinoSketchUploaderOptions options;
        private UploaderSerialPort serialPort;
        private MemoryBlock hexFileMemoryBlock;

        private const int UploadBaudRate = 115200;
        private const int ReadTimeOut = 1000;

        public ArduinoSketchUploader(ArduinoSketchUploaderOptions options)
        {
            logger.Info("Starting ArduinoSketchUploader...");
            this.options = options;
        }

        public void UploadSketch()
        {
            var hexFileName = options.FileName;
            logger.Info("Starting upload process for file '{0}'.", hexFileName);
            var hexFileContents = File.ReadAllLines(hexFileName);
            UploadSketch(hexFileContents);
        }

        public void UploadSketch(IEnumerable<string> hexFileContents)
        {
            var serialPortName = options.PortName;
            hexFileMemoryBlock = ReadHexFile(hexFileContents);

            if (SerialPort.GetPortNames().SingleOrDefault(x => x.Equals(serialPortName, StringComparison.OrdinalIgnoreCase)) == null)
                UploaderLogger.LogAndThrowError<ArgumentException>(string.Format("Specified COM port name '{0}' is not valid.", serialPortName));

            logger.Trace("Creating serial port '{0}'...", serialPortName);
            serialPort = new UploaderSerialPort(serialPortName, UploadBaudRate);

            try
            {
                TryToOpenSerialPort();
                ConfigureSerialPort();
                ResetArduino();
                EstablishSync();
                CheckDeviceSignature();
                InitializeDevice();
                EnableProgrammingMode();
                ProgramDevice();
                ResetArduino();
            }
            finally
            {
                CloseSerialPort();
            }
            logger.Info("All done, shutting down!");
        }

        #region Private Methods

        private static MemoryBlock ReadHexFile(IEnumerable<string> hexFileContents)
        {
            try
            {
                var reader = new HexFileReader(hexFileContents, ATMega328Constants.ATMEGA328_FLASH_SIZE);
                return reader.Parse();
            }
            catch (Exception ex)
            {
                UploaderLogger.LogAndThrowError<IOException>(ex.Message);
            }
            return null;
        }

        private void TryToOpenSerialPort()
        {
            logger.Trace("Opening serial port...");
            try
            {
                serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                UploaderLogger.LogAndThrowError<UnauthorizedAccessException>(
                    "Access to the port is denied. This or another process is currently using this port.");
            }
            catch (ArgumentOutOfRangeException)
            {
                UploaderLogger.LogAndThrowError<ArgumentOutOfRangeException>(
                    "The configuration parameters for the port are invalid (e.g. baud rate, parity, databits).");
            }
            catch (IOException)
            {
                UploaderLogger.LogAndThrowError<IOException>("The port is in an invalid state.");
            }
            logger.Trace("Opened serial port {0} with baud rate {1}!", serialPort.PortName, serialPort.BaudRate);
        }

        private void CloseSerialPort()
        {
            logger.Info("Closing serial port...");
            serialPort.DtrEnable = false;
            serialPort.RtsEnable = false;
            try
            {
                serialPort.Close();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void ConfigureSerialPort()
        {
            logger.Trace("Setting Read Timeout on serial port to '{0}'.", ReadTimeOut);
            serialPort.ReadTimeout = ReadTimeOut;
        }

        private void EstablishSync()
        {
            logger.Info("Trying to establish sync...");
            serialPort.EstablishSync();
            logger.Info("Sync established!");
        }

        private void ResetArduino()
        {
            logger.Info("Resetting Arduino...");
            serialPort.DtrEnable = false;
            serialPort.RtsEnable = false;

            Thread.Sleep(250);

            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;

            Thread.Sleep(50);
        }

        private void InitializeDevice()
        {
            logger.Info("Initializing device!");
            var majorVersion = GetParameterValue(CommandConstants.Parm_STK_SW_MAJOR);
            var minorVersion = GetParameterValue(CommandConstants.Parm_STK_SW_MINOR);
            logger.Info("Retrieved software version: {0}.{1}.", majorVersion, minorVersion);

            logger.Info("Setting device programming parameters...");
            serialPort.SendWithSyncRetry(new SetDeviceProgrammingParametersRequest());
            var nextByte = serialPort.ReceiveNext();

            if (nextByte != CommandConstants.Resp_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>("Unable to set device programming parameters!");
            logger.Info("Device initialized!");
        }

        private void EnableProgrammingMode()
        {
            logger.Info("Enabling programming mode on the device...");
            serialPort.SendWithSyncRetry(new EnableProgrammingModeRequest());
            var nextByte = serialPort.ReceiveNext();
            if (nextByte == CommandConstants.Resp_STK_OK) return;
            if (nextByte == CommandConstants.Resp_STK_NODEVICE || nextByte == CommandConstants.Resp_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>("Unable to enable programming mode on the device!");
        }

        private uint GetParameterValue(byte param)
        {
            logger.Trace("Retrieving parameter '{0}'...", param);
            serialPort.SendWithSyncRetry(new GetParameterRequest(param));
            var nextByte = serialPort.ReceiveNext();
            var paramValue = (uint)nextByte;
            nextByte = serialPort.ReceiveNext();

            if (nextByte == CommandConstants.Resp_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>(string.Format("Fetching parameter '{0}' failed!", param));
            if (nextByte != CommandConstants.Resp_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(string.Format("Protocol error while retrieving parameter '{0}'", param));
            return paramValue;
        }

        private void CheckDeviceSignature()
        {
            logger.Info("Checking device signature, excpecting to find 0x1e 0x95 0x0f...");
            serialPort.SendWithSyncRetry(new ReadSignatureRequest());
            var response = serialPort.Receive<ReadSignatureResponse>(4);
            if (response == null || !response.IsCorrectResponse)
                UploaderLogger.LogAndThrowError<IOException>("Unable to check device signature!");
            // ReSharper disable once PossibleNullReferenceException
            var signature = response.Signature;
            if (signature[0] != 0x1e || signature[1] != 0x95 || signature[2] != 0x0f)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Signature {0} {1} {2} was different than what was expected!", signature[0], signature[1], signature[2]));
        }

        private void ProgramDevice()
        {
            var sizeToWrite = hexFileMemoryBlock.HighestModifiedOffset + 1;
            const byte pageSize = ATMega328Constants.ATMEGA328_FLASH_PAGESIZE;
            logger.Info("Preparing to write {0} bytes...", sizeToWrite);
            logger.Info("Flash memory page size: {0}.", pageSize);

            int pageaddr;
            for (pageaddr = 0; pageaddr < sizeToWrite; pageaddr += pageSize)
            {
                var needsWrite = false;
                for (var i = pageaddr; i < pageaddr + pageSize; i++)
                {
                    if (!hexFileMemoryBlock.Cells[i].Modified) continue;
                    needsWrite = true;
                    break;
                }
                if (needsWrite)
                {
                    logger.Trace("Executing paged write from address {0} (page size {1})...", pageaddr, pageSize);
                    ExecutePagedWrite(pageaddr, pageSize);
                }
                else
                {
                    logger.Trace("Skip writing page...");
                }
            }
            logger.Info("{0} bytes written to flash memory!", sizeToWrite);
        }

        private void ExecutePagedWrite(int addr, int pageSize)
        {
            int blockSize;
            var n = addr + pageSize;

            for (; addr < n; addr += blockSize)
            {
                blockSize = n - addr < pageSize ? n - addr : pageSize;
                LoadAddress((uint)Math.Truncate(addr / (double)2));
                var bytesToCopy = hexFileMemoryBlock.Cells.Skip(addr).Take(pageSize).Select(x => x.Value).ToArray();
                serialPort.SendWithSyncRetry(new ExecutePagedWriteRequest(pageSize, blockSize, bytesToCopy));
                var nextByte = serialPort.ReceiveNext();
                if (nextByte == CommandConstants.Resp_STK_OK) return;
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Write for address page from address {0} failed!", addr));
            }
        }

        private void LoadAddress(uint addr)
        {
            serialPort.SendWithSyncRetry(new LoadAddressRequest(addr));
            var result = serialPort.ReceiveNext();
            if (result == CommandConstants.Resp_STK_OK) return;
            UploaderLogger.LogAndThrowError<IOException>(string.Format("LoadAddress failed with result {0}!", result));
        }

        #endregion
    }
}
