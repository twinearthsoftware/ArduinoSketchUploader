namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class LoadAddressRequest : Request
    {
        public LoadAddressRequest(uint address)
        {
            Bytes = new[]
            {
                Constants.Cmnd_STK_LOAD_ADDRESS,
                (byte)(address & 0xff),
                (byte)((address >> 8) & 0xff),
                Constants.Sync_CRC_EOP
            };
        }
    }
}
