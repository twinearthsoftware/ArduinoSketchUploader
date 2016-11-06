using System;
using System.Collections.Generic;
using System.IO;
using ArduinoUploader.Hardware;
using ArduinoUploader.Hardware.Memory;
using ArduinoUploader.Protocols;
using ArduinoUploader.Protocols.STK500v2;
using ArduinoUploader.Protocols.STK500v2.Messages;
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

        private readonly IDictionary<MemoryType, byte> readCommands = new Dictionary<MemoryType, byte>()
        {
            { MemoryType.FLASH, Constants.CMD_READ_FLASH_ISP },
            { MemoryType.EEPROM, Constants.CMD_READ_EEPROM_ISP }
        };

        private readonly IDictionary<MemoryType, byte> writeCommands = new Dictionary<MemoryType, byte>()
        {
            { MemoryType.FLASH, Constants.CMD_PROGRAM_FLASH_ISP },
            { MemoryType.EEPROM, Constants.CMD_PROGRAM_EEPROM_ISP }
        };

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

        public WiringBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu)
            : base(serialPort, mcu)
        {
        }

        protected override void Reset()
        {
            ToggleDtrRts(50, 50);
        }

        protected override void Send(IRequest request)
        {
            var requestBodyLength = request.Bytes.Length;
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

            var wrappedResponseBytes = new byte[300];
            var messageStart = ReceiveNext();
            if (messageStart != Constants.MESSAGE_START)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "No Start Message detected!");
                return null;                
            }
            wrappedResponseBytes[0] = (byte) messageStart;
            logger.Trace("Received MESSAGE_START.");

            var seqNumber = ReceiveNext();
            if (seqNumber != LastCommandSequenceNumber)
            {
                logger.Warn(
                    STK500v2_CORRUPT_WRAPPER,
                    "Wrong sequence number!");
                return null;                      
            }
            wrappedResponseBytes[1] = sequenceNumber;
            logger.Trace("Received sequence number.");

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
            logger.Trace("Received message size: {0}.", messageSize);

            var token = ReceiveNext();
            if (token != Constants.TOKEN)
            {
                logger.Warn(
                   STK500v2_CORRUPT_WRAPPER,
                   "Token not received!");
                return null;               
            }
            wrappedResponseBytes[4] = (byte) token;

            logger.Trace("Received TOKEN.");

            var payload = new byte[messageSize];
            var retrieved = 0;
            try
            {
                retrieved = SerialPort.Read(payload, 0, messageSize);
                logger.Trace(
                    "Retrieved {0} bytes: {1}",
                    retrieved,
                    BitConverter.ToString(payload));
            }
            catch (TimeoutException)
            {
                payload = null;
            }
            if (payload == null || retrieved < messageSize)
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

        public override void ExecuteWritePage(IMemory memory, int offset, byte[] bytes)
        {
            LoadAddress(memory, offset / 2);
            logger.Info(
                "Sending execute write page request for offset {0} ({1} bytes)...", 
                offset, bytes.Length);

            var writeCmd = writeCommands[memory.Type];

            Send(new ExecuteProgramPageRequest(writeCmd, memory, bytes));
            var response = Receive<ExecuteProgramPageResponse>();
            if (response == null || response.AnswerID != writeCmd
                || response.Status != Constants.STATUS_CMD_OK)
            {
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        "Executing write page request at offset {0} failed!", offset));
            }
        }

        public override byte[] ExecuteReadPage(IMemory memory, int offset)
        {
            LoadAddress(memory, offset / 2);
            logger.Trace("Sending execute read page request (offset {0})...", offset);
            var readCmd = readCommands[memory.Type];

            Send(new ExecuteReadPageRequest(readCmd, memory));
            var response = Receive<ExecuteReadPageResponse>();
            if (response == null || response.AnswerID != readCmd
                || response.Status != Constants.STATUS_CMD_OK)
            {
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format(
                        "Executing read page request at offset {0} failed!", offset));
            }
            var responseBytes = new byte[memory.PageSize];
            Buffer.BlockCopy(response.Bytes, 2, responseBytes, 0, responseBytes.Length);
            return responseBytes;
        }

        private void LoadAddress(IMemory memory, int addr)
        {
            logger.Trace("Sending load address request: {0}.", addr);
            Send(new LoadAddressRequest(memory, addr));
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
