using RJCP.IO.Ports;

namespace ArduinoUploader.BootloaderProgrammers.ResetBehavior
{
    internal class ResetThroughTogglingDtrBehavior : IResetBehavior
    {
        private bool Toggle { get; }

        public ResetThroughTogglingDtrBehavior(bool toggle)
        {
            Toggle = toggle;
        }

        public SerialPortStream Reset(SerialPortStream serialPort, SerialPortConfig config)
        {
            serialPort.DtrEnable = Toggle;
            return serialPort;
        }
    }
}