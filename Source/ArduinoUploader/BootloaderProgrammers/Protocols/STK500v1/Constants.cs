namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1
{
    internal static class Constants
    {
        internal const byte CmdStkGetSync = 0x30;
        internal const byte CmdStkGetParameter = 0x41;
        internal const byte CmdStkSetDevice = 0x42;
        internal const byte CmdStkEnterProgmode = 0x50;
        internal const byte CmdStkLeaveProgmode = 0x51;
        internal const byte CmdStkLoadAddress = 0x55;
        internal const byte CmdStkProgPage = 0x64;
        internal const byte CmdStkReadPage = 0x74;
        internal const byte CmdStkReadSignature = 0x75;

        internal const byte SyncCrcEop = 0x20;

        internal const byte RespStkOk = 0x10;
        internal const byte RespStkFailed = 0x11;
        internal const byte RespStkNodevice = 0x13;
        internal const byte RespStkInsync = 0x14;
        internal const byte RespStkNosync = 0x15;

        internal const byte ParmStkSwMajor = 0x81;
        internal const byte ParmStkSwMinor = 0x82;
    }
}