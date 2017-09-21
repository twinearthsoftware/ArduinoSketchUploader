namespace ArduinoUploader.BootloaderProgrammers.Protocols
{
    internal interface IMessage
    {
        byte[] Bytes { get; set; }
    }
}