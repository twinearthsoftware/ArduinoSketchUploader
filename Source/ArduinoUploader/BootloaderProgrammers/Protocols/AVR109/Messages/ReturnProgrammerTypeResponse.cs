namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ReturnProgrammerTypeResponse : Response
    {
        internal char ProgrammerType => (char) Bytes[0];
    }
}