namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class EnableProgrammingModeRequest : Request
    {
        internal EnableProgrammingModeRequest()
        {
            Bytes = new[]
            {
                Constants.CmdStkEnterProgmode,
                Constants.SyncCrcEop
            };
        }
    }
}