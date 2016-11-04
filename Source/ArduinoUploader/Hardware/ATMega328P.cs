using System.Collections.Generic;

namespace ArduinoUploader.Hardware
{
    internal class ATMega328P : ATMegaMCU
    {
        public override int FlashSize { get { return 32*1024; } }
        public override int FlashPageSize { get { return 0x80; } }
        public override int EEPROMSize { get { return 1024; } }

        public override byte DeviceCode { get { return 0x86; } }
        public override byte DeviceRevision { get { return 0; } }
        public override byte ProgType { get { return 0; } }
        public override byte ParallelMode { get { return 1; } }
        public override byte Polling { get { return 1; } }
        public override byte SelfTimed { get { return 1; } }
        public override byte LockBytes { get { return 1; } }
        public override byte FuseBytes { get { return 3; } }
        public override byte FlashPollVal1 { get { return 0xff; } }
        public override byte FlashPollVal2 { get { return 0xff; } }
        public override byte EEPROMPollVal1 { get { return 0xff; } }
        public override byte EEPROMPollVal2 { get { return 0xff; } }

        public override byte Timeout { get { return 200; } }
        public override byte StabDelay { get { return 100; } }
        public override byte CmdExeDelay { get { return 25; } }
        public override byte SynchLoops { get { return 32;  } }
        public override byte ByteDelay { get { return 0; } }
        public override byte PollIndex { get { return 3; } }
        public override byte PollValue { get { return 0x53; } }

        public override IDictionary<Command, byte[]> CommandBytes
        {
            get { return new Dictionary<Command, byte[]>(); }
        }
    }
}
