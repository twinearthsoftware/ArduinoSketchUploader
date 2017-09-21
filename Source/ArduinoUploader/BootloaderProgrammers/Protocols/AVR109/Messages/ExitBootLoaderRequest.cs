namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ExitBootLoaderRequest : Request
    {
        internal ExitBootLoaderRequest()
        {
            Bytes = new[]
            {
                Constants.CmdExitBootloader
            };
        }
    }
}