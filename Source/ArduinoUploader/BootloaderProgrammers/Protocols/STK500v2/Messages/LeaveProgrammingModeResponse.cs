namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class LeaveProgrammingModeResponse : Response
    {
        internal bool Success => Bytes.Length == 2
            && Bytes[0] == Constants.CmdLeaveProgmodeIsp
            && Bytes[1] == Constants.StatusCmdOk;
    }
}