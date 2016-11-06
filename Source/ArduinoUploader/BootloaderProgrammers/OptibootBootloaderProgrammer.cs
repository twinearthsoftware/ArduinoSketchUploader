using System;
using System.IO;
using System.Linq;
using ArduinoUploader.Hardware;
using ArduinoUploader.Hardware.Memory;
using ArduinoUploader.Protocols;
using ArduinoUploader.Protocols.STK500v1;
using ArduinoUploader.Protocols.STK500v1.Messages;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class OptibootBootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string EXPECTED_DEVICE_SIGNATURE = "1e-95-0f";

        internal OptibootBootloaderProgrammer(UploaderSerialPort serialPort, IMCU mcu)
            : base(serialPort, mcu)
        {
        }

        protected override void Reset()
        {
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
                    string.Format(
                        "Unable to establish sync after {0} retries.", MaxSyncRetries));

            var nextByte = ReceiveNext();

            if (nextByte != Constants.RESP_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(
                    "Unable to establish sync.");
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
                var result = (TResponse) Activator.CreateInstance(typeof(TResponse));
                result.Bytes = bytes;
                return result;
            }
            catch (TimeoutException)
            {
                logger.Trace(
                    "Timeout - no response received after {0}ms.", 
                    SerialPort.ReadTimeout);
                return null;
            }
        }

        protected void SendWithSyncRetry(IRequest request)
        {
            byte nextByte;
            while (true)
            {
                Send(request);
                nextByte = (byte) ReceiveNext();
                if (nextByte == Constants.RESP_STK_NOSYNC)
                {
                    EstablishSync();
                    continue;
                }
                break;
            }
            if (nextByte != Constants.RESP_STK_INSYNC)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        "Unable to aqcuire sync in SendWithSyncRetry for request of type {0}!", 
                        request.GetType()));
        }

        public override void CheckDeviceSignature()
        {
            logger.Debug("Expecting to find '{0}'...", EXPECTED_DEVICE_SIGNATURE);
            SendWithSyncRetry(new ReadSignatureRequest());
            var response = Receive<ReadSignatureResponse>(4);
            if (response == null || !response.IsCorrectResponse)
                UploaderLogger.LogAndThrowError<IOException>(
                    "Unable to check device signature!");

            var signature = response.Signature;
            if (signature[0] != 0x1e || signature[1] != 0x95 || signature[2] != 0x0f)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        "Unexpected device signature - found '{0}'- expected '{1}'.",
                        BitConverter.ToString(signature), 
                        EXPECTED_DEVICE_SIGNATURE));
        }

        public override void InitializeDevice()
        {
            var majorVersion = GetParameterValue(Constants.PARM_STK_SW_MAJOR);
            var minorVersion = GetParameterValue(Constants.PARM_STK_SW_MINOR);
            logger.Info("Retrieved software version: {0}.", 
                string.Format("{0}.{1}", majorVersion, minorVersion));

            logger.Info("Setting device programming parameters...");
            SendWithSyncRetry(new SetDeviceProgrammingParametersRequest((MCU)MCU));
            var nextByte = ReceiveNext();

            if (nextByte != Constants.RESP_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(
                    "Unable to set device programming parameters!");
        }

        public override void EnableProgrammingMode()
        {
            SendWithSyncRetry(new EnableProgrammingModeRequest());
            var nextByte = ReceiveNext();
            if (nextByte == Constants.RESP_STK_OK) return;
            if (nextByte == Constants.RESP_STK_NODEVICE || nextByte == Constants.RESP_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>(
                    "Unable to enable programming mode on the device!");
        }

        private uint GetParameterValue(byte param)
        {
            logger.Trace("Retrieving parameter '{0}'...", param);
            SendWithSyncRetry(new GetParameterRequest(param));
            var nextByte = ReceiveNext();
            var paramValue = (uint)nextByte;
            nextByte = ReceiveNext();

            if (nextByte == Constants.RESP_STK_Failed)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Retrieving parameter '{0}' failed!", param));

            if (nextByte != Constants.RESP_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        "General protocol error while retrieving parameter '{0}'.", 
                        param));

            return paramValue;
        }

        public override void ExecuteWritePage(IMemory memory, int offset, byte[] bytes)
        {
            LoadAddress(offset);
            SendWithSyncRetry(new ExecuteProgramPageRequest(memory, bytes));
            var nextByte = ReceiveNext();
            if (nextByte == Constants.RESP_STK_OK) return;
            UploaderLogger.LogAndThrowError<IOException>(
                string.Format("Write at offset {0} failed!", offset));
        }

        public override byte[] ExecuteReadPage(IMemory memory, int offset)
        {
            var pageSize = memory.PageSize;
            LoadAddress(offset);
            SendWithSyncRetry(new ExecuteReadPageRequest(memory.Type, pageSize));
            var bytes = ReceiveNext(pageSize);
            if (bytes == null)
            {
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Read at offset {0} failed!", offset));                
            }

            var nextByte = ReceiveNext();
            if (nextByte == Constants.RESP_STK_OK) return bytes;
            UploaderLogger.LogAndThrowError<IOException>(
                string.Format("Read at offset {0} failed!", offset));
            return null;
        }

        private void LoadAddress(int addr)
        {
            logger.Trace("Sending load address request: {0}.", addr);
            addr = addr >> 1;
            SendWithSyncRetry(new LoadAddressRequest(addr));
            var result = ReceiveNext();
            if (result == Constants.RESP_STK_OK) return;
            UploaderLogger.LogAndThrowError<IOException>(string.Format("LoadAddress failed with result {0}!", result));
        }
    }
}
