namespace ArduinoUploader.Protocols.STK500v2.Messages
{
    internal class LoadAddressRequest : Request
    {
        public LoadAddressRequest(int addr)
        {
            Bytes = new[]
            {
                Constants.CMD_LOAD_ADDRESS,
                (byte)((addr >> 24) & 0xff),
                (byte)((addr >> 16) & 0xff),
                (byte)((addr >> 8) & 0xff),
                (byte)(addr & 0xff)
            };
        }
    }
}
