namespace ArduinoUploader.Protocols.STK500v2
{
    internal static class Constants
    {
        internal const byte CMD_SIGN_ON                         = 0x01;
        internal const byte CMD_GET_PARAMETER                   = 0x03;
        internal const byte CMD_ENTER_PROGRMODE_ISP             = 0x10;

        internal const byte STATUS_CMD_OK                       = 0x00;

        internal const byte MESSAGE_START                       = 0x1b;
        internal const byte TOKEN                               = 0x0e;

        internal const byte PARAM_HW_VER                        = 0x90;
        internal const byte PARAM_SW_MAJOR                      = 0x91;
        internal const byte PARAM_SW_MINOR                      = 0x92;
    }
}
