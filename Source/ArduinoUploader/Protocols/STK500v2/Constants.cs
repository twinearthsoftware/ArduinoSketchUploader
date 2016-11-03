namespace ArduinoUploader.Protocols.STK500v2
{
    internal static class Constants
    {
        internal const byte Cmnd_SIGN_ON                        = 0x01;

        internal const byte Status_CMD_OK                       = 0x00;

        internal const byte MESSAGE_START                       = 0x1b;
        internal const byte TOKEN                               = 0x0e;
    }
}
