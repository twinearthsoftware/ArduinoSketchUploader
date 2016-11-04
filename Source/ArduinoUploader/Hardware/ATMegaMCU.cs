using System.Collections.Generic;

namespace ArduinoUploader.Hardware
{
    internal abstract class ATMegaMCU : MCU
    {
        public abstract int FlashSize { get; }
        public abstract int FlashPageSize { get; }
        public abstract int EEPROMSize { get; }

        public abstract byte DeviceCode { get; }
        public abstract byte DeviceRevision { get; }
        public abstract byte LockBytes { get; }
        public abstract byte FuseBytes { get; }
        public abstract byte FlashPollVal1 { get; }
        public abstract byte FlashPollVal2 { get; }
        public abstract byte EEPROMPollVal1 { get; }
        public abstract byte EEPROMPollVal2 { get; }

        public abstract byte Timeout { get; }
        public abstract byte StabDelay { get; }
        public abstract byte CmdExeDelay { get; }
        public abstract byte SynchLoops { get; }
        public abstract byte ByteDelay { get; }
        public abstract byte PollValue { get; }
        public abstract byte PollIndex { get; }

        public abstract IDictionary<Command, byte[]> CommandBytes { get; }

        public virtual byte ProgType { get { return 0; } }
        public virtual byte ParallelMode { get { return 0; } }
        public virtual byte Polling { get { return 1; } }
        public virtual byte SelfTimed { get { return 1; } }
    }
}
