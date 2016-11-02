namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class GetParameterRequest : Request
    {
        public GetParameterRequest(byte param)
        {
            Bytes = new[]
            {
                Constants.Cmnd_STK_GET_PARAMETER,
                param,
                Constants.Sync_CRC_EOP
            };
        }
    }
}
