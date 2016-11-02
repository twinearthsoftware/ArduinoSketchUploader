using System.IO.Ports;

namespace ArduinoUploader
{
    internal class UploaderSerialPort : SerialPort
    {
        internal static readonly int MaxSyncRetries = 20;

        public UploaderSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {
        }
    }
}
