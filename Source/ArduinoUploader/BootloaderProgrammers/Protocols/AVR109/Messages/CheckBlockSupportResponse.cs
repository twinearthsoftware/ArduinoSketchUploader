namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class CheckBlockSupportResponse : Response
    {
        internal bool HasBlockSupport => Bytes[0] == (byte) 'Y';

        internal int BufferSize => (Bytes[1] << 8) + Bytes[2];
    }
}