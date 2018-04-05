using System;
using System.Threading;
using ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages;
using ArduinoUploader.Hardware;
using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1
{
    internal class Stk500V1BootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        internal Stk500V1BootloaderProgrammer(SerialPortConfig serialPortConfig, IMcu mcu)
            : base(serialPortConfig, mcu)
        {
        }

        public override void EstablishSync()
        {
            const int maxRetries = 256;
            var retryCounter = 0;
            while (retryCounter++ < maxRetries)
            {
                SerialPort.DiscardInBuffer();
                Send(new GetSyncRequest());
                var result = Receive<GetSyncResponse>();
                if (result == null) continue;
                if (result.IsInSync) break;
                Thread.Sleep(20);
            }
            if (retryCounter == maxRetries)
                throw new ArduinoUploaderException(
                    $"Unable to establish sync after {maxRetries} retries.");

            retryCounter = 0;
            while (retryCounter++ < maxRetries)
            {
                var nextByte = ReceiveNext();
                if (nextByte == Constants.RespStkOk) break;
            }
            if (retryCounter == maxRetries)
                throw new ArduinoUploaderException("Unable to establish sync.");
        }

        protected void SendWithSyncRetry(IRequest request)
        {
            byte nextByte;
            while (true)
            {
                Send(request);
                nextByte = (byte) ReceiveNext();
                if (nextByte == Constants.RespStkNosync)
                {
                    EstablishSync();
                    continue;
                }
                break;
            }
            if (nextByte != Constants.RespStkInsync)
                throw new ArduinoUploaderException(
                    $"Unable to aqcuire sync in SendWithSyncRetry for request of type {request.GetType()}!");
        }

        public override void CheckDeviceSignature()
        {
            Logger?.Debug($"Expecting to find '{Mcu.DeviceSignature}'...");
            SendWithSyncRetry(new ReadSignatureRequest());
            var response = Receive<ReadSignatureResponse>(4);
            if (response == null || !response.IsCorrectResponse)
                throw new ArduinoUploaderException("Unable to check device signature!");

            var signature = response.Signature;
            if (BitConverter.ToString(signature) != Mcu.DeviceSignature)
                throw new ArduinoUploaderException(
                    $"Unexpected device signature - found '{BitConverter.ToString(signature)}'- expected '{Mcu.DeviceSignature}'.");
        }

        public override void InitializeDevice()
        {
            var majorVersion = GetParameterValue(Constants.ParmStkSwMajor);
            var minorVersion = GetParameterValue(Constants.ParmStkSwMinor);
            Logger?.Info($"Retrieved software version: {majorVersion}.{minorVersion}.");

            Logger?.Info("Setting device programming parameters...");
            SendWithSyncRetry(new SetDeviceProgrammingParametersRequest((Mcu) Mcu));
            var nextByte = ReceiveNext();

            if (nextByte != Constants.RespStkOk)
                throw new ArduinoUploaderException("Unable to set device programming parameters!");
        }

        public override void EnableProgrammingMode()
        {
            SendWithSyncRetry(new EnableProgrammingModeRequest());
            var nextByte = ReceiveNext();
            if (nextByte == Constants.RespStkOk) return;
            if (nextByte == Constants.RespStkNodevice || nextByte == Constants.RespStkFailed)
                throw new ArduinoUploaderException("Unable to enable programming mode on the device!");
        }

        public override void LeaveProgrammingMode()
        {
            SendWithSyncRetry(new LeaveProgrammingModeRequest());
            var nextByte = ReceiveNext();
            if (nextByte == Constants.RespStkOk) return;
            if (nextByte == Constants.RespStkNodevice || nextByte == Constants.RespStkFailed)
                throw new ArduinoUploaderException("Unable to leave programming mode on the device!");
        }

        private uint GetParameterValue(byte param)
        {
            Logger?.Trace($"Retrieving parameter '{param}'...");
            SendWithSyncRetry(new GetParameterRequest(param));
            var nextByte = ReceiveNext();
            var paramValue = (uint) nextByte;
            nextByte = ReceiveNext();

            if (nextByte == Constants.RespStkFailed)
                throw new ArduinoUploaderException($"Retrieving parameter '{param}' failed!");

            if (nextByte != Constants.RespStkOk)
                throw new ArduinoUploaderException(
                    $"General protocol error while retrieving parameter '{param}'.");

            return paramValue;
        }

        public override void ExecuteWritePage(IMemory memory, int offset, byte[] bytes)
        {
            SendWithSyncRetry(new ExecuteProgramPageRequest(memory, bytes));
            var nextByte = ReceiveNext();
            if (nextByte == Constants.RespStkOk) return;
            throw new ArduinoUploaderException($"Write at offset {offset} failed!");
        }

        public override byte[] ExecuteReadPage(IMemory memory)
        {
            var pageSize = memory.PageSize;
            SendWithSyncRetry(new ExecuteReadPageRequest(memory.Type, pageSize));
            var bytes = ReceiveNext(pageSize);
            if (bytes == null)
                throw new ArduinoUploaderException("Execute read page failed!");

            var nextByte = ReceiveNext();
            if (nextByte == Constants.RespStkOk) return bytes;
            throw new ArduinoUploaderException("Execute read page failed!");
        }

        public override void LoadAddress(IMemory memory, int addr)
        {
            Logger?.Trace($"Sending load address request: {addr}.");
            addr = addr >> 1;
            SendWithSyncRetry(new LoadAddressRequest(addr));
            var result = ReceiveNext();
            if (result == Constants.RespStkOk) return;
            throw new ArduinoUploaderException($"LoadAddress failed with result {result}!");
        }
    }
}