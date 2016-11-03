namespace ArduinoUploader.Protocols.STK500v2.Messages
{
    internal class GetSyncRequest : Request
    {
        public GetSyncRequest()
        {
            Bytes = new[]
            {
                Constants.Cmnd_SIGN_ON
            };
        }
    }
}
