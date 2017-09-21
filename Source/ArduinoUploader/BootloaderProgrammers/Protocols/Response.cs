namespace ArduinoUploader.BootloaderProgrammers.Protocols
{
    internal abstract class Response : IRequest
    {
        public byte[] Bytes { get; set; }
    }
}