using System.Linq;
using RJCP.IO.Ports;

namespace ArduinoUploader.BootloaderProgrammers.ResetBehavior
{
    internal class ResetThrough1200BpsBehavior : IResetBehavior
    {
        private static IArduinoUploaderLogger Logger => ArduinoSketchUploader.Logger;

        public SerialPortStream Reset(SerialPortStream serialPort, SerialPortConfig config)
        {
            const int timeoutVirtualPortDiscovery = 10000;
            const int virtualPortDiscoveryInterval = 100;
            Logger?.Info("Issuing forced 1200bps reset...");
            var currentPortName = serialPort.PortName;
            var originalPorts = SerialPortStream.GetPortNames();

            // Close port ...
            serialPort.Close();

            // And now open port at 1200 bps
            serialPort = new SerialPortStream(currentPortName, 1200)
            {
                Handshake = Handshake.DtrRts
            };
            serialPort.Open();

            // Close and wait for a new virtual COM port to appear ...
            serialPort.Close();

            var newPort = WaitHelper.WaitFor(timeoutVirtualPortDiscovery, virtualPortDiscoveryInterval,
                () => SerialPortStream.GetPortNames().Except(originalPorts).SingleOrDefault(),
                (i, item, interval) =>
                    item == null
                        ? $"T+{i * interval} - Port not found"
                        : $"T+{i * interval} - Port found: {item}");

            if (newPort == null)
                throw new ArduinoUploaderException(
                    $"No (unambiguous) virtual COM port detected (after {timeoutVirtualPortDiscovery}ms).");

            return new SerialPortStream
            {
                BaudRate = config.BaudRate,
                PortName = newPort,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.DtrRts
            };
        }
    }
}