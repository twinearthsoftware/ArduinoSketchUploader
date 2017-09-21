using RJCP.IO.Ports;

namespace ArduinoUploader.BootloaderProgrammers.ResetBehavior
{
    internal class ResetThroughEnablingDtrBehavior : IResetBehavior
    {
        public SerialPortStream Reset(SerialPortStream serialPort, SerialPortConfig config)
        {
            serialPort.DtrEnable = true;
            return serialPort;
        }
    }
}