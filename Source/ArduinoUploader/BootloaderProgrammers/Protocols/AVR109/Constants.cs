namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109
{
    internal static class Constants
    {
        internal const byte Null = 0x00;

        internal const byte CarriageReturn = 0x0d;

        internal const byte CmdSetAddress = 0x41;
        internal const byte CmdStartBlockLoad = 0x42;
        internal const byte CmdExitBootloader = 0x45;
        internal const byte CmdLeaveProgrammingMode = 0x4c;
        internal const byte CmdEnterProgrammingMode = 0x50;
        internal const byte CmdReturnSoftwareIdentifier = 0x53;
        internal const byte CmdSelectDeviceType = 0x54;
        internal const byte CmdReturnSoftwareVersion = 0x56;
        internal const byte CmdCheckBlockSupport = 0x62;
        internal const byte CmdStartBlockRead = 0x67;
        internal const byte CmdReturnProgrammerType = 0x70;
        internal const byte CmdReadSignatureBytes = 0x73;
        internal const byte CmdReturnSupportedDeviceCodes = 0x74;
    }
}