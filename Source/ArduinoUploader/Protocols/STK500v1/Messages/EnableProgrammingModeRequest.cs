namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class EnableProgrammingModeRequest : Request
    {
        public EnableProgrammingModeRequest()
        {
            Bytes = new[]
            {
                Constants.CMD_STK_ENTER_PROGMODE,
                Constants.SYNC_CRC_EOP
            };
        }
    }
}
