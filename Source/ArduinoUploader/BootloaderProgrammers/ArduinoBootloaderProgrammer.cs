using System;
using ArduinoUploader.Hardware;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class ArduinoBootloaderProgrammer : SerialPortBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected int MaxSyncRetries { get { return 20; } }

        protected abstract void Reset();

        protected ArduinoBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu, Func<int, MemoryBlock> memoryBlockGenerator)
            : base(serialPort, mcu, memoryBlockGenerator)
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
