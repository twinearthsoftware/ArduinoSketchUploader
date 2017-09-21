namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class GetSyncResponse : Response
    {
        internal bool IsInSync => Bytes.Length > 0 && Bytes[0] == Constants.RespStkInsync;
    }
}