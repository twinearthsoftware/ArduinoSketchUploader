namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class GetSyncResponse : Response
    {
        public bool IsInSync
        {
            get { return Bytes.Length > 0 && Bytes[0] == CommandConstants.CommandConstants.Resp_STK_INSYNC; }
        }
    }
}
