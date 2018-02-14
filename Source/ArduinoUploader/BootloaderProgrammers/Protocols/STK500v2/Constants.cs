namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2
{
    internal static class Constants
    {
        internal const byte CmdSignOn = 0x01;
        internal const byte CmdGetParameter = 0x03;
        internal const byte CmdLoadAddress = 0x06;
        internal const byte CmdEnterProgrmodeIsp = 0x10;
        internal const byte CmdLeaveProgmodeIsp = 0x11;
        internal const byte CmdProgramFlashIsp = 0x13;
        internal const byte CmdReadFlashIsp = 0x14;
        internal const byte CmdProgramEepromIsp = 0x15;
        internal const byte CmdReadEepromIsp = 0x16;
        internal const byte CmdSpiMulti = 0x1d;

        internal const byte StatusCmdOk = 0x00;

        internal const byte MessageStart = 0x1b;
        internal const byte Token = 0x0e;

        internal const byte ParamHwVer = 0x90;
        internal const byte ParamSwMajor = 0x91;
        internal const byte ParamSwMinor = 0x92;
        internal const byte ParamVTarget = 0x94;
    }
}