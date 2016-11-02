namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class EnableProgrammingModeRequest : Request
    {
        public EnableProgrammingModeRequest()
        {
            Bytes = new[]
            {
                Constants.Cmnd_STK_ENTER_PROGMODE,
                Constants.Sync_CRC_EOP
            };
        }
    }
}
