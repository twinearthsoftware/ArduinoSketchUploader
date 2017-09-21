namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ReturnSoftwareVersionResponse : Response
    {
        internal char MajorVersion => (char) Bytes[0];

        internal char MinorVersion => (char) Bytes[1];
    }
}