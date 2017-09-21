using RJCP.IO.Ports;

namespace ArduinoUploader.BootloaderProgrammers.ResetBehavior
{
    internal interface IResetBehavior
    {
        SerialPortStream Reset(SerialPortStream serialPort, SerialPortConfig config);
    }
}