using System;
using System.IO;
using System.Linq;
using ArduinoUploader.Protocols;
using ArduinoUploader.Protocols.STK500v1.CommandConstants;
using ArduinoUploader.Protocols.STK500v1.HardwareConstants;
using ArduinoUploader.Protocols.STK500v1.Messages;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class OptibootBootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal OptibootBootloaderProgrammer(UploaderSerialPort serialPort, MemoryBlock memoryBlock)
            : base(serialPort, memoryBlock)
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
            for (i = 0; i < UploaderSerialPort.MaxSyncRetries; i++)
            {
                Send(new GetSyncRequest());
                var result = Receive<GetSyncResponse>();
                if (result == null) continue;
                if (result.IsInSync) break;
            }

            if (i == UploaderSerialPort.MaxSyncRetries)
                UploaderLogger.LogAndThrowError<IOException>(string.Format("Unable to establish sync after {0} retries!", UploaderSerialPort.MaxSyncRetries));

            var nextByte = ReceiveNext();

            if (nextByte != CommandConstants.Resp_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>("Unable to establish sync.");
        }

        protected void SendWithSyncRetry(IRequest request)
        {
            SendWithSyncRetry(
                request,
                (b) => b == CommandConstants.Resp_STK_NOSYNC,
                (b) => b == CommandConstants.Resp_STK_INSYNC);
        }

        public override void CheckDeviceSignature()
        {
            logger.Info("Checking device signature...");
            logger.Debug("Expecting to find 0x1e 0x95 0x0f...");
            SendWithSyncRetry(new ReadSignatureRequest());
            var response = Receive<ReadSignatureResponse>(4);
            if (response == null || !response.IsCorrectResponse)
                UploaderLogger.LogAndThrowError<IOException>("Unable to check device signature!");
            // ReSharper disable once PossibleNullReferenceException
            var signature = response.Signature;
            if (signature[0] != 0x1e || signature[1] != 0x95 || signature[2] != 0x0f)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Signature {0} {1} {2} was different than what was expected!",
                        signature[0], signature[1], signature[2]));
        }

        public override void InitializeDevice()
        {
            logger.Info("Initializing device!");
            var majorVersion = GetParameterValue(CommandConstants.Parm_STK_SW_MAJOR);
            var minorVersion = GetParameterValue(CommandConstants.Parm_STK_SW_MINOR);
            logger.Info("Retrieved software version: {0}.{1}.", majorVersion, minorVersion);

            logger.Info("Setting device programming parameters...");
            SendWithSyncRetry(new SetDeviceProgrammingParametersRequest());
            var nextByte = ReceiveNext();

            if (nextByte != CommandConstants.Resp_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>("Unable to set device programming parameters!");
            logger.Info("Device initialized!");
        }

        public override void EnableProgrammingMode()
        {
            logger.Info("Enabling programming mode on the device...");
            SendWithSyncRetry(new EnableProgrammingModeRequest());
            var nextByte = ReceiveNext();
            if (nextByte == CommandConstants.Resp_STK_OK) return;
            if (nextByte == CommandConstants.Resp_STK_NODEVICE || nextByte == CommandConstants.Resp_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>("Unable to enable programming mode on the device!");
        }

        public override void ProgramDevice()
        {
            var sizeToWrite = MemoryBlock.HighestModifiedOffset + 1;
            const byte pageSize = ATMega328Constants.ATMEGA328_FLASH_PAGESIZE;
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
            logger.Trace("Retrieving parameter '{0}'...", param);
            SendWithSyncRetry(new GetParameterRequest(param));
            var nextByte = ReceiveNext();
            var paramValue = (uint)nextByte;
            nextByte = ReceiveNext();

            if (nextByte == CommandConstants.Resp_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>(string.Format("Fetching parameter '{0}' failed!", param));
            if (nextByte != CommandConstants.Resp_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(string.Format("Protocol error while retrieving parameter '{0}'", param));
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
                if (nextByte == CommandConstants.Resp_STK_OK) return;
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Write for address page from address {0} failed!", addr));
            }
        }

        private void LoadAddress(uint addr)
        {
            SendWithSyncRetry(new LoadAddressRequest(addr));
            var result = ReceiveNext();
            if (result == CommandConstants.Resp_STK_OK) return;
            UploaderLogger.LogAndThrowError<IOException>(string.Format("LoadAddress failed with result {0}!", result));
        }
    }
}
