using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages;
using ArduinoUploader.Hardware;
using ArduinoUploader.Hardware.Memory;
using RJCP.IO.Ports;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109
{
    internal class Avr109BootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        public Avr109BootloaderProgrammer(SerialPortConfig serialPortConfig, IMcu mcu)
            : base(serialPortConfig, mcu)
        {
        }

        public override void Close()
        {
            try
            {
                var currentPort = SerialPort.PortName;
                Logger?.Info($"Closing {currentPort}...");
                SerialPort.Close();
                Logger?.Info($"Waiting for virtual port {currentPort} to disappear...");

                const int timeoutVirtualPointDisappearance = 10000;
                const int virtualPortDisappearanceInterval = 100;
                var result = WaitHelper.WaitFor(timeoutVirtualPointDisappearance, virtualPortDisappearanceInterval,
                    () => SerialPortStream.GetPortNames().Contains(currentPort) ? null : currentPort,
                    (i, item, interval) =>
                        item == null
                            ? $"T+{i * interval} - Port still present..."
                            : $"T+{i * interval} - Port disappeared: {item}!");

                if (result == null)
                    Logger?.Warn(
                        $"Virtual COM port {currentPort} was still present "
                        + "after {timeoutVirtualPointDisappearance} ms!");
            }
            catch (Exception ex)
            {
                throw new ArduinoUploaderException(
                    $"Exception during close of the programmer: '{ex.Message}'.");
            }
        }

        public override void CheckDeviceSignature()
        {
            Logger?.Debug($"Expecting to find '{Mcu.DeviceSignature}'...");
            Send(new ReadSignatureBytesRequest());
            var response = Receive<ReadSignatureBytesResponse>(3);
            if (response == null)
                throw new ArduinoUploaderException(
                    "Unable to check device signature!");

            var signature = response.Signature;
            if (BitConverter.ToString(signature) != Mcu.DeviceSignature)
                throw new ArduinoUploaderException(
                    $"Unexpected device signature - found '{BitConverter.ToString(signature)}'"
                    + $"- expected '{Mcu.DeviceSignature}'.");
        }

        public override void InitializeDevice()
        {
            Send(new ReturnSoftwareIdentifierRequest());
            var softIdResponse = Receive<ReturnSoftwareIdentifierResponse>(7);
            if (softIdResponse == null)
                throw new ArduinoUploaderException(
                    "Unable to retrieve software identifier!");

            Logger?.Info("Software identifier: "
                + $"'{Encoding.ASCII.GetString(softIdResponse.Bytes)}'");

            Send(new ReturnSoftwareVersionRequest());
            var softVersionResponse = Receive<ReturnSoftwareVersionResponse>(2);
            if (softVersionResponse == null)
                throw new ArduinoUploaderException(
                    "Unable to retrieve software version!");

            Logger?.Info("Software Version: "
                + $"{softVersionResponse.MajorVersion}.{softVersionResponse.MinorVersion}");

            Send(new ReturnProgrammerTypeRequest());
            var progTypeResponse = Receive<ReturnProgrammerTypeResponse>();
            if (progTypeResponse == null)
                throw new ArduinoUploaderException(
                    "Unable to retrieve programmer type!");

            Logger?.Info($"Programmer type: {progTypeResponse.ProgrammerType}.");

            Send(new CheckBlockSupportRequest());
            var checkBlockResponse = Receive<CheckBlockSupportResponse>(3);
            if (checkBlockResponse == null)
                throw new ArduinoUploaderException("Unable to retrieve block support!");
            if (!checkBlockResponse.HasBlockSupport)
                throw new ArduinoUploaderException("Block support is not supported!");

            Logger?.Info($"Block support - buffer size {checkBlockResponse.BufferSize} bytes.");

            Send(new ReturnSupportedDeviceCodesRequest());
            var devices = new List<byte>();
            do
            {
                var nextByte = (byte) ReceiveNext();
                if (nextByte != Constants.Null) devices.Add(nextByte);
                else break;
            } while (true);

            var supportedDevices = string.Join("-", devices);
            Logger?.Info($"Supported devices: {supportedDevices}.");

            var devCode = Mcu.DeviceCode;
            if (!devices.Contains(devCode))
                throw new ArduinoUploaderException(
                    $"Device {devCode} not in supported list of devices: {supportedDevices}!");

            Logger?.Info($"Selecting device type '{devCode}'...");
            Send(new SelectDeviceTypeRequest(devCode));
            var response = ReceiveNext();
            if (response != Constants.CarriageReturn)
                throw new ArduinoUploaderException("Unable to execute select device type command!");
        }

        public override void EnableProgrammingMode()
        {
            Send(new EnterProgrammingModeRequest());
            var response = ReceiveNext();
            if (response != Constants.CarriageReturn)
                throw new ArduinoUploaderException("Unable to enter programming mode!");
        }

        public override void LoadAddress(IMemory memory, int offset)
        {
            Logger?.Trace($"Sending load address request: {offset}.");
            Send(new SetAddressRequest(offset / 2));
            var response = ReceiveNext();
            if (response != Constants.CarriageReturn)
                throw new ArduinoUploaderException("Unable to execute set address request!");
        }

        public override byte[] ExecuteReadPage(IMemory memory)
        {
            var type = memory.Type;
            var blockSize = memory.PageSize;
            Send(new StartBlockReadRequest(type, blockSize));
            var response = Receive<StartBlockReadResponse>(blockSize);
            return response.Bytes;
        }

        public override void ExecuteWritePage(IMemory memory, int offset, byte[] bytes)
        {
            var type = memory.Type;
            var blockSize = memory.PageSize;
            Send(new StartBlockLoadRequest(type, blockSize, bytes));
            var response = ReceiveNext();
            if (response != Constants.CarriageReturn)
                throw new ArduinoUploaderException("Unable to execute write page!");
        }

        public override void LeaveProgrammingMode()
        {
            Send(new LeaveProgrammingModeRequest());
            var leaveProgModeResp = ReceiveNext();
            if (leaveProgModeResp != Constants.CarriageReturn)
                throw new ArduinoUploaderException("Unable to leave programming mode!");

            Send(new ExitBootLoaderRequest());
            var exitBootloaderResp = ReceiveNext();
            if (exitBootloaderResp != Constants.CarriageReturn)
                throw new ArduinoUploaderException("Unable to exit boot loader!");
        }
    }
}