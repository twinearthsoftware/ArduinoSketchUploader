namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class ExecuteProgramPageResponse : Response
    {
        internal byte AnswerId => Bytes[0];
        internal byte Status => Bytes[1];
    }
}