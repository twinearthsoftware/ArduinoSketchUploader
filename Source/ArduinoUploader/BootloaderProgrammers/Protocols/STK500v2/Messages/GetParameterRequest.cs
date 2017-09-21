namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class GetParameterRequest : Request
    {
        internal GetParameterRequest(byte param)
        {
            Bytes = new[]
            {
                Constants.CmdGetParameter,
                param
            };
        }
    }
}