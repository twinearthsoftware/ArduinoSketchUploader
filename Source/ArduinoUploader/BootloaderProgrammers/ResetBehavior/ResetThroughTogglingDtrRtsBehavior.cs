using System.Threading;
using RJCP.IO.Ports;

namespace ArduinoUploader.BootloaderProgrammers.ResetBehavior
{
    internal class ResetThroughTogglingDtrRtsBehavior : IResetBehavior
    {
        private static IArduinoUploaderLogger Logger => ArduinoSketchUploader.Logger;

        private int Wait1 { get; }
        private int Wait2 { get; }
        private bool Invert { get; }

        public ResetThroughTogglingDtrRtsBehavior(int wait1, int wait2, bool invert = false)
        {
            Wait1 = wait1;
            Wait2 = wait2;
            Invert = invert;
        }

        public SerialPortStream Reset(SerialPortStream serialPort, SerialPortConfig config)
        {
            Logger?.Trace("Toggling DTR/RTS...");

            serialPort.DtrEnable = Invert;
            serialPort.RtsEnable = Invert;

            Thread.Sleep(Wait1);

            serialPort.DtrEnable = !Invert;
            serialPort.RtsEnable = !Invert;

            Thread.Sleep(Wait2);

            return serialPort;
        }
    }
}