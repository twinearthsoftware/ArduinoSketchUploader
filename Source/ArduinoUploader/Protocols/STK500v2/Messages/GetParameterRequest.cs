namespace ArduinoUploader.Protocols.STK500v2.Messages
{
    internal class GetParameterRequest : Request
    {
        public GetParameterRequest(byte param)
        {
            Bytes = new[]
            {
                Constants.CMD_GET_PARAMETER,
                param
            };
        }
    }
}
