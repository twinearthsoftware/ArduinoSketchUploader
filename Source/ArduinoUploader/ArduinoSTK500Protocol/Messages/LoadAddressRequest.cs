namespace ArduinoUploader.ArduinoSTK500Protocol.Messages
{
    internal class LoadAddressRequest : Request
    {
        public LoadAddressRequest(uint address)
        {
            Bytes = new[]
            {
                CommandConstants.CommandConstants.Cmnd_STK_LOAD_ADDRESS,
                (byte)(address & 0xff),
                (byte)((address >> 8) & 0xff),
                CommandConstants.CommandConstants.Sync_CRC_EOP
            };
        }
    }
}
