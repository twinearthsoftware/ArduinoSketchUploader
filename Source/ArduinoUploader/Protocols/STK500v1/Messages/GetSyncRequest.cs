namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class GetSyncRequest : Request
    {
        public GetSyncRequest()
        {
            Bytes = new[]
            {
                Constants.Cmnd_STK_GET_SYNC,
                Constants.Sync_CRC_EOP
            };
        }
    }
}
