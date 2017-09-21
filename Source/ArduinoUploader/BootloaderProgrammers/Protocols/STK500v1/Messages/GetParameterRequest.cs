namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class GetParameterRequest : Request
    {
        internal GetParameterRequest(byte param)
        {
            Bytes = new[]
            {
                Constants.CmdStkGetParameter,
                param,
                Constants.SyncCrcEop
            };
        }
    }
}