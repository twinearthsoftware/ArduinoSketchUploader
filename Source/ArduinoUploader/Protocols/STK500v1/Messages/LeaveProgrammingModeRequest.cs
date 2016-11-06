namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class LeaveProgrammingModeRequest : Request
    {
        public LeaveProgrammingModeRequest()
        {
            Bytes = new[]
            {
                Constants.CMD_STK_LEAVE_PROGMODE,
                Constants.SYNC_CRC_EOP
            };
        }
    }
}
