using ArduinoUploader.Hardware;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class ArduinoBootloaderProgrammer : SerialPortBootloaderProgrammer
    {
        protected int MaxSyncRetries { get { return 20; } }

        protected abstract void Reset();

        protected ArduinoBootloaderProgrammer(UploaderSerialPort serialPort, IMCU mcu)
            : base(serialPort, mcu)
        {
        }

        public override void Open()
        {
            Reset();
        }

        public override void Close()
        {
            Reset();
        }
    }
}
