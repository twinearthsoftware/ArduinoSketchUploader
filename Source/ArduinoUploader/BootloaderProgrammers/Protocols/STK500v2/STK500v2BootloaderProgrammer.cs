using System;
using System.Collections.Generic;
using ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages;
using ArduinoUploader.Hardware;
using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2
{
    internal class Stk500V2BootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static byte _sequenceNumber;
        protected static byte LastCommandSequenceNumber;

        private readonly IDictionary<MemoryType, byte> _readCommands = new Dictionary<MemoryType, byte>
        {
            {MemoryType.Flash, Constants.CmdReadFlashIsp},
            {MemoryType.Eeprom, Constants.CmdReadEepromIsp}
        };

        private readonly IDictionary<MemoryType, byte> _writeCommands = new Dictionary<MemoryType, byte>
        {
            {MemoryType.Flash, Constants.CmdProgramFlashIsp},
            {MemoryType.Eeprom, Constants.CmdProgramEepromIsp}
        };

        private string _deviceSignature;

        public Stk500V2BootloaderProgrammer(SerialPortConfig serialPortConfig, IMcu mcu)
            : base(serialPortConfig, mcu)
        {
        }

        protected static byte SequenceNumber
        {
            get
            {
                if (_sequenceNumber == 255) _sequenceNumber = 0;
                return ++_sequenceNumber;
            }
        }

        protected override void Send(IRequest request)
        {
            var requestBodyLength = request.Bytes.Length;
            var totalMessageLength = requestBodyLength + 6;
            var wrappedBytes = new byte[totalMessageLength];
            wrappedBytes[0] = Constants.MessageStart;
            wrappedBytes[1] = LastCommandSequenceNumber = SequenceNumber;
            wrappedBytes[2] = (byte) (requestBodyLength >> 8);
            wrappedBytes[3] = (byte) (requestBodyLength & 0xFF);
            wrappedBytes[4] = Constants.Token;
            Buffer.BlockCopy(request.Bytes, 0, wrappedBytes, 5, requestBodyLength);

            byte checksum = 0;
            for (var i = 0; i < totalMessageLength - 1; i++) checksum ^= wrappedBytes[i];
            wrappedBytes[totalMessageLength - 1] = checksum;

            request.Bytes = wrappedBytes;
            base.Send(request);
        }

        protected TResponse Receive<TResponse>() where TResponse : Response
        {
            var response = (TResponse) Activator.CreateInstance(typeof(TResponse));

            var wrappedResponseBytes = new byte[300];

            // Discard bytes until we get a MessageStart
            const int maxRetries = 256;
            var retryCounter = 0;
            while (retryCounter++ < maxRetries)
            {
                var messageStart = ReceiveNext();
                if (messageStart == Constants.MessageStart) break;
            }
            if (retryCounter == maxRetries)
            {
                Logger?.Trace($"No MESSAGE_START found after {maxRetries} bytes!");
                return null;
            }

            wrappedResponseBytes[0] = Constants.MessageStart;

            var seqNumber = ReceiveNext();
            if (seqNumber != LastCommandSequenceNumber)
            {
                Logger?.Warn(CorruptWrapper($"Wrong sequence number: {seqNumber} - expected {LastCommandSequenceNumber}!"));
                return null;
            }
            wrappedResponseBytes[1] = _sequenceNumber;

            var messageSizeHighByte = ReceiveNext();
            if (messageSizeHighByte == -1)
            {
                Logger?.Warn(CorruptWrapper("Timeout ocurred!"));
                return null;
            }
            wrappedResponseBytes[2] = (byte) messageSizeHighByte;

            var messageSizeLowByte = ReceiveNext();
            if (messageSizeLowByte == -1)
            {
                Logger?.Warn(CorruptWrapper("Timeout ocurred!"));
                return null;
            }
            wrappedResponseBytes[3] = (byte) messageSizeLowByte;

            var messageSize = (messageSizeHighByte << 8) + messageSizeLowByte;

            var token = ReceiveNext();
            if (token != Constants.Token)
            {
                Logger?.Warn(CorruptWrapper("Token not received!"));
                return null;
            }
            wrappedResponseBytes[4] = (byte) token;

            var payload = ReceiveNext(messageSize);
            if (payload == null)
            {
                Logger?.Warn(CorruptWrapper("Inner message not received!"));
                return null;
            }

            Buffer.BlockCopy(payload, 0, wrappedResponseBytes, 5, messageSize);

            var responseCheckSum = ReceiveNext();
            if (responseCheckSum == -1)
            {
                Logger?.Warn(CorruptWrapper("Checksum not received!"));
                return null;
            }
            wrappedResponseBytes[5 + messageSize] = (byte) responseCheckSum;

            byte checksum = 0;
            for (var i = 0; i < 5 + messageSize; i++) checksum ^= wrappedResponseBytes[i];

            if (responseCheckSum != checksum)
            {
                Logger?.Warn(CorruptWrapper("Checksum incorrect!"));
                return null;
            }

            var message = new byte[messageSize];
            Buffer.BlockCopy(wrappedResponseBytes, 5, message, 0, messageSize);
            response.Bytes = message;
            return response;
        }

        public override void EstablishSync()
        {
            Send(new GetSyncRequest());
            var result = Receive<GetSyncResponse>();
            if (result == null || !result.IsInSync)
                throw new ArduinoUploaderException(
                    "Unable to establish sync!");
            _deviceSignature = result.Signature;
        }

        public override void CheckDeviceSignature()
        {
            Logger?.Debug($"Expecting to find '{Mcu.DeviceSignature}'...");
            if (!_deviceSignature.Equals(Mcu.DeviceSignature))
                throw new ArduinoUploaderException(
                    $"Unexpected device signature - found '{_deviceSignature}'"
                    + $"- expected '{Mcu.DeviceSignature}'.");
        }

        public override void InitializeDevice()
        {
            var hardwareVersion = GetParameterValue(Constants.ParamHwVer);
            var softwareMajor = GetParameterValue(Constants.ParamSwMajor);
            var softwareMinor = GetParameterValue(Constants.ParamSwMinor);
            var vTarget = GetParameterValue(Constants.ParamVTarget);
            Logger?.Info(
                $"Retrieved software version: {hardwareVersion} (hardware) "
                + $"- {softwareMajor}.{softwareMinor} (software).");
            Logger?.Info($"Parameter VTarget: {vTarget}.");
        }

        public override void EnableProgrammingMode()
        {
            Send(new EnableProgrammingModeRequest(Mcu));
            var response = Receive<EnableProgrammingModeResponse>();
            if (response == null || response.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to enable programming mode on the device!");

            // TODO: properly decode SPI instructions...

            // --- Device signature
            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x30, 0x00, 0x00, 0x00 }));
            var spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x30, 0x00, 0x01, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (response == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x30, 0x00, 0x02, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (response == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            // --- Fuses

            // LFuse
            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x50, 0x00, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x50, 0x00, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x50, 0x00, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            // HFuse
            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x58, 0x08, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x58, 0x08, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x58, 0x08, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            // EFuse
            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x50, 0x08, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x50, 0x08, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");

            Send(new ExecuteSpiCommandRequest(4, 4, 0, new byte[] { 0x50, 0x08, 0x00, 0x00 }));
            spiResponse = Receive<ExecuteSpiCommandResponse>();
            if (spiResponse == null || spiResponse.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Unable to execute SPI command!");
        }

        public override void LeaveProgrammingMode()
        {
            Send(new LeaveProgrammingModeRequest());
            var response = Receive<LeaveProgrammingModeResponse>();
            if (response == null)
                throw new ArduinoUploaderException("Unable to leave programming mode on the device!");
        }

        public override void ExecuteWritePage(IMemory memory, int offset, byte[] bytes)
        {
            Logger?.Trace("Sending execute write page request for offset "
                + $"{offset} ({bytes.Length} bytes)...");

            var writeCmd = _writeCommands[memory.Type];

            Send(new ExecuteProgramPageRequest(writeCmd, memory, bytes));
            var response = Receive<ExecuteProgramPageResponse>();
            if (response == null || response.AnswerId != writeCmd
                || response.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException(
                    $"Executing write page request at offset {offset} failed!");
        }

        public override byte[] ExecuteReadPage(IMemory memory)
        {
            var readCmd = _readCommands[memory.Type];

            Send(new ExecuteReadPageRequest(readCmd, memory));
            var response = Receive<ExecuteReadPageResponse>();
            if (response == null || response.AnswerId != readCmd || response.Status != Constants.StatusCmdOk)
                throw new ArduinoUploaderException("Executing read page request failed!");

            var responseBytes = new byte[memory.PageSize];
            Buffer.BlockCopy(response.Bytes, 2, responseBytes, 0, responseBytes.Length);
            return responseBytes;
        }

        public override void LoadAddress(IMemory memory, int offset)
        {
            Logger?.Trace($"Sending load address request: {offset:X2}.");
            offset = offset >> 1;
            Send(new LoadAddressRequest(memory, offset));
            var response = Receive<LoadAddressResponse>();
            if (response == null || !response.Succeeded)
                throw new ArduinoUploaderException("Unable to execute load address!");
        }

        private uint GetParameterValue(byte param)
        {
            Logger?.Trace($"Retrieving parameter '{param:X2}'...");
            Send(new GetParameterRequest(param));
            var response = Receive<GetParameterResponse>();
            if (response == null || !response.IsSuccess)
                throw new ArduinoUploaderException($"Retrieving parameter '{param}' failed!");
            return response.ParameterValue;
        }

        private static string CorruptWrapper(string message)
        {
            return $"STK500V2 wrapper corrupted ({message})!";
        }
    }
}