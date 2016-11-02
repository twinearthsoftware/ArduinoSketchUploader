using ArduinoUploader.Hardware;
using IntelHexFormatReader.Model;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class ArduinoBootloaderProgrammer : SerialPortBootloaderProgrammer
    {
        protected MCU MCU { get; private set; }

        protected abstract void Reset();

        protected ArduinoBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu, MemoryBlock memoryBlock)
            : base(serialPort, memoryBlock)
        {
            MCU = mcu;
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
