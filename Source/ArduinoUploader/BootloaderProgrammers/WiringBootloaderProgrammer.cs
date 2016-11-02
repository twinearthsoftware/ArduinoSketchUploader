using ArduinoUploader.Hardware;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class WiringBootloaderProgrammer : ArduinoBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WiringBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu, MemoryBlock memoryBlock)
            : base(serialPort, mcu, memoryBlock)
        {
        }

        protected override void Reset()
        {
            logger.Info(BootloaderProgrammerMessages.RESETTING_ARDUINO);
            ToggleDtrRts(50, 50);
        }

        public override void EstablishSync()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckDeviceSignature()
        {
            throw new System.NotImplementedException();
        }

        public override void InitializeDevice()
        {
            throw new System.NotImplementedException();
        }

        public override void EnableProgrammingMode()
        {
            throw new System.NotImplementedException();
        }

        public override void ProgramDevice()
        {
            throw new System.NotImplementedException();
        }
    }
}
