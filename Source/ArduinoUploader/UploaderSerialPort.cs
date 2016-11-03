using System.IO.Ports;

namespace ArduinoUploader
{
    internal class UploaderSerialPort : SerialPort
    {
        public UploaderSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {
        }
    }
}
