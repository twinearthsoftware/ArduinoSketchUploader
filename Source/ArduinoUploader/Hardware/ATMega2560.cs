namespace ArduinoUploader.Hardware
{
    internal class ATMega2560 : ATMegaMCU
    {
        public ATMega2560(byte devcode, byte fuse, byte lfuse, byte hfuse, byte efuse, byte flashPageSize) 
            : base(devcode, fuse, lfuse, hfuse, efuse, flashPageSize, 256 * 1024, 4 * 1024)
        {
        }
    }
}
