namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class GetParameterResponse : Response
    {
        internal bool IsSuccess => 
            Bytes.Length > 2 && Bytes[0] == Constants.CmdGetParameter
            && Bytes[1] == Constants.StatusCmdOk;

        internal byte ParameterValue => Bytes[2];
    }
}