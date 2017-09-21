namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class ReadSignatureRequest : Request
    {
        internal ReadSignatureRequest()
        {
            Bytes = new[]
            {
                Constants.CmdStkReadSignature,
                Constants.SyncCrcEop
            };
        }
    }
}