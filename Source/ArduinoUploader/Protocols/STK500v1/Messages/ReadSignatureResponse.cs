namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class ReadSignatureResponse : Response
    {
        public bool IsCorrectResponse
        {
            get { return Bytes.Length == 4 && Bytes[3] == Constants.RESP_STK_OK; }
        }

        public byte[] Signature
        {
            get {  return new[] { Bytes[0], Bytes[1], Bytes[2] }; }
        }
    }
}
