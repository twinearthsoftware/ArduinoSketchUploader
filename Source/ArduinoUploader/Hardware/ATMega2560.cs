using System.Collections.Generic;
using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.Hardware
{
    internal class AtMega2560 : Mcu
    {
        public override byte DeviceCode => 0xb2;

        public override byte DeviceRevision => 0;

        public override byte LockBytes => 1;

        public override byte FuseBytes => 3;

        public override byte Timeout => 200;

        public override byte StabDelay => 100;

        public override byte CmdExeDelay => 25;

        public override byte SynchLoops => 32;

        public override byte ByteDelay => 0;
        
        public override byte PollIndex => 3;

        public override byte PollValue => 0x53;

        public override string DeviceSignature => "AVRISP_2";

        public override IDictionary<Command, byte[]> CommandBytes => 
            new Dictionary<Command, byte[]>
        {
            {Command.PgmEnable, new byte[] {0xac, 0x53, 0x00, 0x00}}
        };

        public override IList<IMemory> Memory => new List<IMemory>
        {
            new FlashMemory
            {
                Size = 256 * 1024,
                PageSize = 256,
                PollVal1 = 0x00,
                PollVal2 = 0x00,
                Delay = 10,
                CmdBytesRead = new byte[] {0x20, 0x00, 0x00},
                CmdBytesWrite = new byte[] {0x40, 0x4c, 0x00}
            },
            new EepromMemory
            {
                Size = 4 * 1024,
                PollVal1 = 0x00,
                PollVal2 = 0x00,
                Delay = 10,
                CmdBytesRead = new byte[] {0xa0, 0x00, 0x00},
                CmdBytesWrite = new byte[] {0xc1, 0xc2, 0x00}
            }
        };
    }
}