using IntelHexFormatReader.Model;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class BootloaderProgrammer : IBootloaderProgrammer
    {
        protected MemoryBlock MemoryBlock { get; private set; }

        protected BootloaderProgrammer(MemoryBlock memoryBlock)
        {
            MemoryBlock = memoryBlock;
        }

        public abstract void Open();
        public abstract void Close();
        public abstract void EstablishSync();
        public abstract void CheckDeviceSignature();
        public abstract void InitializeDevice();
        public abstract void EnableProgrammingMode();
        public abstract void ProgramDevice();
    }
}
