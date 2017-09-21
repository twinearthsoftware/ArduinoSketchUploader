namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class LoadAddressResponse : Response
    {
        internal bool Succeeded => Bytes.Length == 2
            && Bytes[0] == Constants.CmdLoadAddress
            && Bytes[1] == Constants.StatusCmdOk;
    }
}