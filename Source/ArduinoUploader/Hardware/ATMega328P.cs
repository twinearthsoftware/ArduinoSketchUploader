namespace ArduinoUploader.Hardware
{
    internal class ATMega328P : ATMegaMCU
    {
        public ATMega328P()
            : base(0x86, 0x0, 0x1, 0x1, 0x1, 0x80, 32 * 1024, 1024)
        {
        }
    }
}
