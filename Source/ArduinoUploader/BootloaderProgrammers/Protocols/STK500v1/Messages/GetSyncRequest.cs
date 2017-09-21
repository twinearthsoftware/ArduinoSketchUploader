namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class GetSyncRequest : Request
    {
        internal GetSyncRequest()
        {
            Bytes = new[]
            {
                Constants.CmdStkGetSync,
                Constants.SyncCrcEop
            };
        }
    }
}