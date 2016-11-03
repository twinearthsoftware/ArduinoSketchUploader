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
        private const string EXPECTED_PROGRAMMER_IDENTIFIER = "AVRISP_2";

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
            ToggleDtrRts(50, 60);
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

        protected override TResponse Receive<TResponse>(int length = 1)
        {
            var response = base.Receive<TResponse>(length);
            if (response == null) return null;

            var wrappedResponseBytes = response.Bytes;

            var messageStart = wrappedResponseBytes[0];
            var seqNumber = wrappedResponseBytes[1];
            var messageSizeHighByte = wrappedResponseBytes[2];
            var messageSizeLowByte = wrappedResponseBytes[3];
            var token = wrappedResponseBytes[4];
            var messageSize = (messageSizeHighByte << 8) + messageSizeLowByte;

            if (messageStart != Constants.MESSAGE_START
                || token != Constants.TOKEN
                || seqNumber != LastCommandSequenceNumber
                || (6 + messageSize) >= wrappedResponseBytes.Length)
            {
                logger.Warn(BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER);
                return null;
            }

            byte checksum = 0;
            for (var i = 0; i < 5 + messageSize; i++) checksum ^= wrappedResponseBytes[i];

            if (wrappedResponseBytes[5 + messageSize] != checksum)
            {
                logger.Warn(BootloaderProgrammerMessages.STK500v2_CORRUPT_WRAPPER);
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
                var result = Receive<GetSyncResponse>(32);
                if (result == null) continue;
                if (result.IsInSync 
                    && result.Signature.Equals(EXPECTED_PROGRAMMER_IDENTIFIER, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            if (i == MaxSyncRetries)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(BootloaderProgrammerMessages.NO_SYNC_WITH_RETRIES, MaxSyncRetries));
        }

        public override void CheckDeviceSignature()
        {
            throw new System.NotImplementedException();
        }

        public override void InitializeDevice()
        {
            throw new System.NotImplementedException();
        }

        public override void EnableProgrammingMode()
        {
            throw new System.NotImplementedException();
        }

        public override void ProgramDevice()
        {
            throw new System.NotImplementedException();
        }
    }
}
