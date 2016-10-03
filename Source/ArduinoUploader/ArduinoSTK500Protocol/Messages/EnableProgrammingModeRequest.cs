namespace ArduinoUploader.ArduinoSTK500Protocol.Messages
{
    internal class EnableProgrammingModeRequest : Request
    {
        public EnableProgrammingModeRequest()
        {
            Bytes = new[]
            {
                CommandConstants.CommandConstants.Cmnd_STK_ENTER_PROGMODE,
                CommandConstants.CommandConstants.Sync_CRC_EOP
            };
        }
    }
}
