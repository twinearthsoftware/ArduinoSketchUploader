namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class ReadSignatureResponse : Response
    {
        internal bool IsCorrectResponse => Bytes.Length == 4 && Bytes[3] == Constants.RespStkOk;

        internal byte[] Signature => new[] {Bytes[0], Bytes[1], Bytes[2]};
    }
}