namespace ArduinoUploader.ArduinoSTK500Protocol.Messages
{
    internal abstract class Request : IRequest
    {
        public byte[] Bytes { get; set; }
    }
}
