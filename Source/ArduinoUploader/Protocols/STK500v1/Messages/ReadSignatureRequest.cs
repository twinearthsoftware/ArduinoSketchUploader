namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class ReadSignatureRequest : Request
    {
        public ReadSignatureRequest()
        {
            Bytes = new[]
            {
                Constants.Cmnd_STK_READ_SIGNATURE,
                Constants.Sync_CRC_EOP
            };
        }
    }
}
