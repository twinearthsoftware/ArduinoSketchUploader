namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ReadSignatureBytesRequest : Request
    {
        internal ReadSignatureBytesRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReadSignatureBytes
            };
        }
    }
}