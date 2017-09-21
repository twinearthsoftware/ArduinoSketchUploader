namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ReturnSoftwareIdentifierRequest : Request
    {
        internal ReturnSoftwareIdentifierRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReturnSoftwareIdentifier
            };
        }
    }
}