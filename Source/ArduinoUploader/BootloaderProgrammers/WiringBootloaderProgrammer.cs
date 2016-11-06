using System;
using System.IO;
using ArduinoUploader.Hardware;
using ArduinoUploader.Protocols;
using ArduinoUploader.Protocols.STK500v2;
using ArduinoUploader.Protocols.STK500v2.Messages;
using IntelHexFormatReader.Model;
using NLog;
using EnableProgrammingModeRequest = ArduinoUploader.Protocols.STK500v2.Messages.EnableProgrammingModeRequest;
using GetParameterRequest = ArduinoUploader.Protocols.STK500v2.Messages.GetParameterRequest;
using GetSyncRequest = ArduinoUploader.Protocols.STK500v2.Messages.GetSyncRequest;
using GetSyncResponse = ArduinoUploader.Protocols.STK500v2.Messages.GetSyncResponse;
using LoadAddressRequest = ArduinoUploader.Protocols.STK500v2.Messages.LoadAddressRequest;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class WiringBootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string EXPECTED_DEVICE_SIGNATURE = "AVRISP_2";
        private const string STK500v2_CORRUPT_WRAPPER = "STK500V2 wrapper corrupted ({0})!";

        private string deviceSignature;
        private static byte sequenceNumber;
        protected static byte LastCommandSequenceNumber;
        protected static byte SequenceNumber
        {
            get
            {
                if (sequenceNumber == 255) sequenceNumber = 0;
                return ++sequenceNumber;
            }
        }

        public WiringBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu, Func<int, MemoryBlock> memoryBlockGenerator)
            : base(serialPort, mcu, memoryBlockGenerator)
        {
        }

        protected override void Reset()
        {
            ToggleDtrRts(50, 50);
        }

        protected override void Send(IRequest request)
        {
            var requestBodyLength = (byte) request.Bytes.Length;
            var totalMessageLength = requestBodyLength + 6;
            var wrappedBytes = new byte[totalMessageLength];
            wrappedBytes[0] = Constants.MESSAGE_START;
            wrappedBytes[1] = LastCommandSequenceNumber = SequenceNumber;
            wrappedBytes[2] = (byte)(requestBodyLength >> 8);
            wrappedBytes[3] = (byte) (requestBodyLength & 0xFF);
            wrappedBytes[4] = Constants.TOKEN;
            Buffer.BlockCopy(request.Bytes, 0, wrappedBytes, 5, requestBodyLength);

            byte checksum = 0;
            for (var i = 0; i < totalMessageLength - 1; i++) checksum ^= wrappedBytes[i];
            wrappedBytes[totalMessageLength -1] = checksum;

            request.Bytes = wrappedBytes;
            base.Send(request);
        }

        protected TResponse Receive<TResponse>() where TResponse: Response
        {
            var response = (TResponse) Activator.CreateInstance(typeof(TResponse));

            var wrappedResponseBytes = new byte[32];
            var messageStart = ReceiveNext();
            if (messageStart != Constants.MESSAGE_START)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "No Start Message detected!");
                return null;                
            }
            wrappedResponseBytes[0] = (byte) messageStart;

            var seqNumber = ReceiveNext();
            if (seqNumber != LastCommandSequenceNumber)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "Wrong sequence number!");
                return null;                      
            }
            wrappedResponseBytes[1] = sequenceNumber;

            var messageSizeHighByte = ReceiveNext();
            if (messageSizeHighByte == -1)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "Timeout ocurred!");
                return null;                       
            }
            wrappedResponseBytes[2] = (byte) messageSizeHighByte;

            var messageSizeLowByte = ReceiveNext();
            if (messageSizeLowByte == -1)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "Timeout ocurred!");
                return null;
            }
            wrappedResponseBytes[3] = (byte) messageSizeLowByte;

            var messageSize = (messageSizeHighByte << 8) + messageSizeLowByte;

            var token = ReceiveNext();
            if (token != Constants.TOKEN)
            {
                logger.Warn(
                   STK500v2_CORRUPT_WRAPPER,
                   "Token not received!");
                return null;               
            }
            wrappedResponseBytes[4] = (byte) token;

            var payload = new byte[messageSize];
            try
            {
                SerialPort.Read(payload, 0, messageSize);
            }
            catch (TimeoutException)
            {
                payload = null;
            }
            if (payload == null)
            {
                logger.Warn(
                   STK500v2_CORRUPT_WRAPPER,
                   "Inner message not received!");
                return null;                               
            }

            Buffer.BlockCopy(payload, 0, wrappedResponseBytes, 5, messageSize);

            var responseCheckSum = ReceiveNext();
            if (responseCheckSum == -1)
            {
                logger.Warn(
                   STK500v2_CORRUPT_WRAPPER,
                   "Checksum not received!");
                return null;
            }
            wrappedResponseBytes[5 + messageSize] = (byte) responseCheckSum;

            byte checksum = 0;
            for (var i = 0; i < 5 + messageSize; i++) checksum ^= wrappedResponseBytes[i];

            if (responseCheckSum != checksum)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "Checksum incorrect!"
                    );
                return null;
            }

            var message = new byte[messageSize];
            Buffer.BlockCopy(wrappedResponseBytes, 5, message, 0, messageSize);
            response.Bytes = message;
            return response;
        }

        public override void EstablishSync()
        {
            int i;
            for (i = 0; i < MaxSyncRetries; i++)
            {
                Send(new GetSyncRequest());
                var result = Receive<GetSyncResponse>();
                if (result == null) continue;
                if (!result.IsInSync) continue;
                deviceSignature = result.Signature;
                break;
            }

            if (i == MaxSyncRetries)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        "Unable to establish sync after {0} retries.", MaxSyncRetries));
        }

        public override void CheckDeviceSignature()
        {
            logger.Debug("Expecting to find '{0}'...", EXPECTED_DEVICE_SIGNATURE);

            if (!deviceSignature.Equals(EXPECTED_DEVICE_SIGNATURE))
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Unexpected device signature - found '{0}'- expected '{1}'.",
                        deviceSignature, EXPECTED_DEVICE_SIGNATURE));
        }

        public override void InitializeDevice()
        {
            var hardwareVersion = GetParameterValue(Constants.PARAM_HW_VER);
            var softwareMajor = GetParameterValue(Constants.PARAM_SW_MAJOR);
            var softwareMinor = GetParameterValue(Constants.PARAM_SW_MINOR);
            logger.Info("Retrieved software version: {0}.",
                string.Format("{0} (hardware) - {1}.{2} (software)", 
                    hardwareVersion, softwareMajor, softwareMinor));
        }

        public override void EnableProgrammingMode()
        {
            Send(new EnableProgrammingModeRequest(MCU));
            var response = Receive<EnableProgrammingModeResponse>();
            if (response == null)
                UploaderLogger.LogAndThrowError<IOException>(
                    "Unable to enable programming mode on the device!");
        }

        public override void ExecuteWritePage(MemoryType memType, int offset, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] ExecuteReadPage(MemoryType memType, int offset, int pageSize)
        {
            LoadAddress(offset);
            Send(new ExecuteReadPageRequest(memType, offset));
            var response = Receive<ExecuteReadPageResponse>();
            //SendWithSyncRetry(new ExecuteReadPageRequest(memType, pageSize));
            //var bytes = ReceiveNext(pageSize);
            //if (bytes == null)
            //{
            //    UploaderLogger.LogAndThrowError<IOException>(
            //        string.Format("Read at offset {0} failed!", offset));
            //}

            //var nextByte = ReceiveNext();
            //if (nextByte == Constants.RESP_STK_OK) return bytes;
            //UploaderLogger.LogAndThrowError<IOException>(
            //    string.Format("Read at offset {0} failed!", offset));
            return null;
        }

        private void LoadAddress(int addr)
        {
            logger.Trace("Sending load address request: {0}.", addr);
            Send(new LoadAddressRequest(addr | 1 << 31));
            var response = Receive<LoadAddressResponse>();
            if (response == null || !response.Succeeded)
                UploaderLogger.LogAndThrowError<IOException>(
                    "Unable to execute load address!");
        }

        private uint GetParameterValue(byte param)
        {
            logger.Trace("Retrieving parameter '{0}'...", param);
            Send(new GetParameterRequest(param));
            var response = Receive<GetParameterResponse>();
            if (response == null || !response.IsSuccess)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Retrieving parameter '{0}' failed!", param));
            return response.ParameterValue;
        }
    }
}
