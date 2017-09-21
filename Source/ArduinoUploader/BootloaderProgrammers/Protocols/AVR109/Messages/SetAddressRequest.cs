namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class SetAddressRequest : Request
    {
        internal SetAddressRequest(int offset)
        {
            Bytes = new[]
            {
                Constants.CmdSetAddress,
                (byte) ((offset >> 8) & 0xff),
                (byte) (offset & 0xff)
            };
        }
    }
}