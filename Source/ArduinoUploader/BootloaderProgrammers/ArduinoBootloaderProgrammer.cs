using IntelHexFormatReader.Model;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class ArduinoBootloaderProgrammer : SerialPortBootloaderProgrammer
    {
        protected abstract void Reset();

        protected ArduinoBootloaderProgrammer(UploaderSerialPort serialPort, MemoryBlock memoryBlock)
            : base(serialPort, memoryBlock)
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
