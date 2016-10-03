namespace ArduinoUploader.ArduinoSTK500Protocol.Messages
{
    internal interface IMessage
    {
        byte[] Bytes { get; set; }
    }
}
