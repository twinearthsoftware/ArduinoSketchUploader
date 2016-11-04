using System.Collections.Generic;

namespace ArduinoUploader.Hardware
{
    internal interface MCU
    {
        int FlashSize { get; }
        int FlashPageSize { get; }
        int EEPROMSize { get; }

        #region Programming

        byte DeviceCode { get; }
        byte DeviceRevision { get; }
        byte ProgType { get; }
        byte ParallelMode { get; }
        byte Polling { get; }
        byte SelfTimed { get; }
        byte LockBytes { get; }
        byte FuseBytes { get; }
        byte FlashPollVal1 { get; }
        byte FlashPollVal2 { get; }
        byte EEPROMPollVal1 { get; }
        byte EEPROMPollVal2 { get; }

        byte Timeout { get; }
        byte StabDelay { get; }
        byte CmdExeDelay { get; }
        byte SynchLoops { get; }
        byte ByteDelay { get; }
        byte PollValue { get; }
        byte PollIndex { get; }

        IDictionary<Command,byte[]> CommandBytes { get; }

        #endregion
    }
}
