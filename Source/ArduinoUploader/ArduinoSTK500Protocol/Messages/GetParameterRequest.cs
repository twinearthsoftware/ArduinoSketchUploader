namespace ArduinoUploader.ArduinoSTK500Protocol.Messages
{
    internal class GetParameterRequest : Request
    {
        public GetParameterRequest(byte param)
        {
            Bytes = new[]
            {
                CommandConstants.CommandConstants.Cmnd_STK_GET_PARAMETER,
                param,
                CommandConstants.CommandConstants.Sync_CRC_EOP
            };
        }
    }
}
