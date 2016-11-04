using System;
using System.IO;
using ArduinoUploader.Hardware;
using ArduinoUploader.Protocols;
using ArduinoUploader.Protocols.STK500v2;
using ArduinoUploader.Protocols.STK500v2.Messages;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class WiringBootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string EXPECTED_DEVICE_SIGNATURE = "AVRISP_2";

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

        public WiringBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu, MemoryBlock memoryBlock)
            : base(serialPort, mcu, memoryBlock)
        {
        }

        protected override void Reset()
        {
            logger.Info(BootloaderProgrammerMessages.RESETTING_ARDUINO);
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
                    BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                    BootloaderProgrammerMessages.STK500v2_NO_START_MESSAGE);
                return null;                
            }
            wrappedResponseBytes[0] = (byte) messageStart;

            var seqNumber = ReceiveNext();
            if (seqNumber != LastCommandSequenceNumber)
            {
                logger.Warn(
                    BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                    BootloaderProgrammerMessages.STK500v2_WRONG_SEQ_NUMBER);
                return null;                      
            }
            wrappedResponseBytes[1] = sequenceNumber;

            var messageSizeHighByte = ReceiveNext();
            if (messageSizeHighByte == -1)
            {
                logger.Warn(
                    BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                    BootloaderProgrammerMessages.STK500v2_TIMEOUT);
                return null;                       
            }
            wrappedResponseBytes[2] = (byte) messageSizeHighByte;

            var messageSizeLowByte = ReceiveNext();
            if (messageSizeLowByte == -1)
            {
                logger.Warn(
                    BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                    BootloaderProgrammerMessages.STK500v2_TIMEOUT);
                return null;
            }
            wrappedResponseBytes[3] = (byte) messageSizeLowByte;

            var messageSize = (messageSizeHighByte << 8) + messageSizeLowByte;

            var token = ReceiveNext();
            if (token != Constants.TOKEN)
            {
                logger.Warn(
                   BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                   BootloaderProgrammerMessages.STK500v2_TOKEN_NOT_RECEIVED);
                return null;               
            }
            wrappedResponseBytes[4] = (byte) token;

            var payload = ReceiveNext(messageSize);
            if (payload == null)
            {
                logger.Warn(
                   BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                   BootloaderProgrammerMessages.STK500v2_MESSAGE_NOT_RECEIVED);
                return null;                               
            }

            Buffer.BlockCopy(payload, 0, wrappedResponseBytes, 5, messageSize);

            var responseCheckSum = ReceiveNext();
            if (responseCheckSum == -1)
            {
                logger.Warn(
                   BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                   BootloaderProgrammerMessages.STK500v2_CHECKSUM_NOT_RECEIVED);
                return null;
            }
            wrappedResponseBytes[5 + messageSize] = (byte) responseCheckSum;

            byte checksum = 0;
            for (var i = 0; i < 5 + messageSize; i++) checksum ^= wrappedResponseBytes[i];

            if (responseCheckSum != checksum)
            {
                logger.Warn(
                    BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER,
                    BootloaderProgrammerMessages.STK500v2_CHECKSUM_INCORRECT
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
                    string.Format(BootloaderProgrammerMessages.NO_SYNC_WITH_RETRIES, MaxSyncRetries));
        }

        public override void CheckDeviceSignature()
        {
            logger.Debug(BootloaderProgrammerMessages.DEVICE_SIG_EXPECTED, 
                EXPECTED_DEVICE_SIGNATURE);

            if (!deviceSignature.Equals(EXPECTED_DEVICE_SIGNATURE))
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.UNEXPECTED_DEVICE_SIG,
                        deviceSignature, EXPECTED_DEVICE_SIGNATURE));
        }

        public override void InitializeDevice()
        {
            var hardwareVersion = GetParameterValue(Constants.PARAM_HW_VER);
            var softwareMajor = GetParameterValue(Constants.PARAM_SW_MAJOR);
            var softwareMinor = GetParameterValue(Constants.PARAM_SW_MINOR);
            logger.Info(BootloaderProgrammerMessages.SOFTWARE_VERSION,
                string.Format("{0} (hardware) - {1}.{2} (software)", 
                    hardwareVersion, softwareMajor, softwareMinor));
        }

        public override void EnableProgrammingMode()
        {
            Send(new EnableProgrammingModeRequest(MCU));
            var response = Receive<EnableProgrammingModeResponse>();
            if (response == null)
                UploaderLogger.LogAndThrowError<IOException>(
                    BootloaderProgrammerMessages.ENABLE_PROGMODE_FAILURE);
        }

        public override void ProgramDevice()
        {
            throw new NotImplementedException();
        }

        private uint GetParameterValue(byte param)
        {
            logger.Trace(BootloaderProgrammerMessages.GET_PARAM, param);
            Send(new GetParameterRequest(param));
            var response = Receive<GetParameterResponse>();
            if (response == null || !response.IsSuccess)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.GET_PARAM_FAILED, param));
            // ReSharper disable once PossibleNullReferenceException
            return response.ParameterValue;
        }
    }
}
