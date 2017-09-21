namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class LeaveProgrammingModeRequest : Request
    {
        internal LeaveProgrammingModeRequest()
        {
            Bytes = new[]
            {
                Constants.CmdLeaveProgrammingMode
            };
        }
    }
}