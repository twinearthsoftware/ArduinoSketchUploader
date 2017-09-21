namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class LoadAddressRequest : Request
    {
        internal LoadAddressRequest(int address)
        {
            Bytes = new[]
            {
                Constants.CmdStkLoadAddress,
                (byte) (address & 0xff),
                (byte) ((address >> 8) & 0xff),
                Constants.SyncCrcEop
            };
        }
    }
}