namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class GetSyncRequest : Request
    {
        public GetSyncRequest()
        {
            Bytes = new[]
            {
                CommandConstants.CommandConstants.Cmnd_STK_GET_SYNC,
                CommandConstants.CommandConstants.Sync_CRC_EOP
            };
        }
    }
}
