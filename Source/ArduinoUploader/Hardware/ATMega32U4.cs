using System.Collections.Generic;
using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.Hardware
{
    internal class AtMega32U4 : Mcu
    {
        public override byte DeviceCode => 0x44;

        public override byte DeviceRevision => 0;

        public override byte ProgType => 0;

        public override byte ParallelMode => 0;

        public override byte Polling => 0;

        public override byte SelfTimed => 0;

        public override byte LockBytes => 0;

        public override byte FuseBytes => 0;

        public override byte Timeout => 200;

        public override byte StabDelay => 100;

        public override byte CmdExeDelay => 25;

        public override byte SynchLoops => 32;

        public override byte ByteDelay => 0;

        public override byte PollIndex => 3;

        public override byte PollValue => 0x53;

        public override string DeviceSignature => "1E-95-87";

        public override IDictionary<Command, byte[]> CommandBytes => 
            new Dictionary<Command, byte[]>();

        public override IList<IMemory> Memory => new List<IMemory>
        {
            new FlashMemory
            {
                Size = 32 * 1024,
                PageSize = 128,
                PollVal1 = 0xff,
                PollVal2 = 0xff
            },
            new EepromMemory
            {
                Size = 1024,
                PollVal1 = 0xff,
                PollVal2 = 0xff
            }
        };
    }
}