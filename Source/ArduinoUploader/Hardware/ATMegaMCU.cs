namespace ArduinoUploader.Hardware
{
    internal abstract class ATMegaMCU : MCU
    {
        protected ATMegaMCU(byte devcode, byte fuse, byte lfuse, byte hfuse, byte efuse, 
            byte flashPageSize, int flashSize, int epromSize)
        {
            DevCode = devcode;
            Fuse = fuse;
            LFuse = lfuse;
            HFuse = hfuse;
            EFuse = efuse;
            FlashPageSize = flashPageSize;
            FlashSize = flashSize;
            EpromSize = epromSize;
        }

        public byte DevCode { get; private set; }
        public byte Fuse { get; private set; }
        public byte LFuse { get; private set; }
        public byte HFuse { get; private set; }
        public byte EFuse { get; private set; }
        public byte FlashPageSize { get; private set; }

        public int FlashSize { get; set; }
        public int EpromSize { get; set; }
    }
}
