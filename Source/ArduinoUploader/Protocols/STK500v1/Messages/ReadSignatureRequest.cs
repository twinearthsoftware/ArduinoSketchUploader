namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class ReadSignatureRequest : Request
    {
        public ReadSignatureRequest()
        {
            Bytes = new[]
            {
                CommandConstants.CommandConstants.Cmnd_STK_READ_SIGNATURE,
                CommandConstants.CommandConstants.Sync_CRC_EOP
            };
        }
    }
}
