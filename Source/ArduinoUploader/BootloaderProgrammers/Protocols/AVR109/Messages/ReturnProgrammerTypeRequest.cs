namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ReturnProgrammerTypeRequest : Request
    {
        internal ReturnProgrammerTypeRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReturnProgrammerType
            };
        }
    }
}