namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class LeaveProgrammingModeRequest : Request
    {
        internal LeaveProgrammingModeRequest()
        {
            Bytes = new[]
            {
                Constants.CmdLeaveProgmodeIsp,
                (byte) 0x01,
                (byte) 0x01
            };
        }
    }
}