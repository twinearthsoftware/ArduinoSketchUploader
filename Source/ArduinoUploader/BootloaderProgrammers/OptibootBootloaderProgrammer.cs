using System;
using System.IO;
using System.Linq;
using ArduinoUploader.Hardware;
using ArduinoUploader.Protocols;
using ArduinoUploader.Protocols.STK500v1;
using ArduinoUploader.Protocols.STK500v1.Messages;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class OptibootBootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string EXPECTED_DEVICE_SIGNATURE = "1e-95-0f";

        internal OptibootBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu, MemoryBlock memoryBlock)
            : base(serialPort, mcu, memoryBlock)
        {
        }

        protected override void Reset()
        {
            logger.Info(BootloaderProgrammerMessages.RESETTING_ARDUINO);
            ToggleDtrRts(250, 50);
        }

        public override void EstablishSync()
        {
            int i;
            for (i = 0; i < MaxSyncRetries; i++)
            {
                Send(new GetSyncRequest());
                var result = Receive<GetSyncResponse>();
                if (result == null) continue;
                if (result.IsInSync) break;
            }

            if (i == MaxSyncRetries)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.NO_SYNC_WITH_RETRIES, MaxSyncRetries));

            var nextByte = ReceiveNext();

            if (nextByte != Constants.RESP_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(
                    BootloaderProgrammerMessages.NO_SYNC);
        }

        protected TResponse Receive<TResponse>(int length = 1) where TResponse : Response
        {
            var bytes = new byte[length];
            try
            {
                SerialPort.Read(bytes, 0, length);
                logger.Trace(
                    "Received {0} bytes: {1}{2}{3}{4}",
                    length,
                    Environment.NewLine, BitConverter.ToString(bytes),
                    Environment.NewLine, string.Join("-", bytes.Select(x => " " + Convert.ToChar(x))));
                var result = (TResponse)Activator.CreateInstance(typeof(TResponse));
                result.Bytes = bytes;
                return result;
            }
            catch (TimeoutException)
            {
                logger.Trace(BootloaderProgrammerMessages.TIMEOUT, SerialPort.ReadTimeout);
                return null;
            }
        }

        protected void SendWithSyncRetry(IRequest request)
        {
            byte nextByte;
            while (true)
            {
                Send(request);
                nextByte = (byte)ReceiveNext();
                if (nextByte == Constants.RESP_STK_NOSYNC)
                {
                    EstablishSync();
                    continue;
                }
                break;
            }
            if (nextByte != Constants.RESP_STK_INSYNC)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.SEND_WITH_SYNC_RETRY_FAILURE, 
                        request.GetType()));
        }

        public override void CheckDeviceSignature()
        {
            logger.Debug(BootloaderProgrammerMessages.DEVICE_SIG_EXPECTED, EXPECTED_DEVICE_SIGNATURE);
            SendWithSyncRetry(new ReadSignatureRequest());
            var response = Receive<ReadSignatureResponse>(4);
            if (response == null || !response.IsCorrectResponse)
                UploaderLogger.LogAndThrowError<IOException>(
                    BootloaderProgrammerMessages.CHECK_DEVICE_SIG_FAILURE);

            // ReSharper disable once PossibleNullReferenceException
            var signature = response.Signature;
            if (signature[0] != 0x1e || signature[1] != 0x95 || signature[2] != 0x0f)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        BootloaderProgrammerMessages.UNEXPECTED_DEVICE_SIG,
                        BitConverter.ToString(signature), 
                        EXPECTED_DEVICE_SIGNATURE));
        }

        public override void InitializeDevice()
        {
            var majorVersion = GetParameterValue(Constants.PARM_STK_SW_MAJOR);
            var minorVersion = GetParameterValue(Constants.PARM_STK_SW_MINOR);
            logger.Info(BootloaderProgrammerMessages.SOFTWARE_VERSION, 
                string.Format("{0}.{1}", majorVersion, minorVersion));

            logger.Info(BootloaderProgrammerMessages.SET_DEVICE_PARAMS);
            SendWithSyncRetry(new SetDeviceProgrammingParametersRequest((ATMegaMCU)MCU));
            var nextByte = ReceiveNext();

            if (nextByte != Constants.RESP_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(
                    BootloaderProgrammerMessages.SET_DEVICE_PARAMS_FAILURE);
        }

        public override void EnableProgrammingMode()
        {
            SendWithSyncRetry(new EnableProgrammingModeRequest());
            var nextByte = ReceiveNext();
            if (nextByte == Constants.RESP_STK_OK) return;
            if (nextByte == Constants.RESP_STK_NODEVICE || nextByte == Constants.RESP_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>(BootloaderProgrammerMessages.ENABLE_PROGMODE_FAILURE);
        }

        public override void ProgramDevice()
        {
            var sizeToWrite = MemoryBlock.HighestModifiedOffset + 1;
            var pageSize = ((ATMegaMCU)MCU).FlashPageSize;
            logger.Info("Preparing to write {0} bytes...", sizeToWrite);
            logger.Info("Flash memory page size: {0}.", pageSize);

            int pageaddr;
            for (pageaddr = 0; pageaddr < sizeToWrite; pageaddr += pageSize)
            {
                var needsWrite = false;
                for (var i = pageaddr; i < pageaddr + pageSize; i++)
                {
                    if (!MemoryBlock.Cells[i].Modified) continue;
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

        private uint GetParameterValue(byte param)
        {
            logger.Trace(BootloaderProgrammerMessages.GET_PARAM, param);
            SendWithSyncRetry(new GetParameterRequest(param));
            var nextByte = ReceiveNext();
            var paramValue = (uint)nextByte;
            nextByte = ReceiveNext();

            if (nextByte == Constants.RESP_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.GET_PARAM_FAILED, param));

            if (nextByte != Constants.RESP_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.GET_PARAM_FAILED_PROTOCOL, param));

            return paramValue;
        }

        private void ExecutePagedWrite(int addr, int pageSize)
        {
            int blockSize;
            var n = addr + pageSize;

            for (; addr < n; addr += blockSize)
            {
                blockSize = n - addr < pageSize ? n - addr : pageSize;
                LoadAddress((uint)Math.Truncate(addr / (double)2));
                var bytesToCopy = MemoryBlock.Cells.Skip(addr).Take(pageSize).Select(x => x.Value).ToArray();
                SendWithSyncRetry(new ExecutePagedWriteRequest(pageSize, blockSize, bytesToCopy));
                var nextByte = ReceiveNext();
                if (nextByte == Constants.RESP_STK_OK) return;
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Write for address page from address {0} failed!", addr));
            }
        }

        private void LoadAddress(uint addr)
        {
            SendWithSyncRetry(new LoadAddressRequest(addr));
            var result = ReceiveNext();
            if (result == Constants.RESP_STK_OK) return;
            UploaderLogger.LogAndThrowError<IOException>(string.Format("LoadAddress failed with result {0}!", result));
        }
    }
}
