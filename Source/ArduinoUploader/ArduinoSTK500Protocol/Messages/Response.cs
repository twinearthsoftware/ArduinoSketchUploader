namespace ArduinoUploader.ArduinoSTK500Protocol.Messages
{
    internal abstract class Response : IRequest
    {
        public byte[] Bytes { get; set; }
    }
}
