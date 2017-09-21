using System.Collections.Generic;
using System.Linq;
using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.Hardware
{
    internal abstract class Mcu : IMcu
    {
        public abstract byte DeviceCode { get; }
        public abstract string DeviceSignature { get; }

        public abstract byte DeviceRevision { get; }
        public abstract byte LockBytes { get; }
        public abstract byte FuseBytes { get; }

        public abstract byte Timeout { get; }
        public abstract byte StabDelay { get; }
        public abstract byte CmdExeDelay { get; }
        public abstract byte SynchLoops { get; }
        public abstract byte ByteDelay { get; }
        public abstract byte PollValue { get; }
        public abstract byte PollIndex { get; }

        public virtual byte ProgType => 0;

        public virtual byte ParallelMode => 0;

        public virtual byte Polling => 1;

        public virtual byte SelfTimed => 1;

        public abstract IDictionary<Command, byte[]> CommandBytes { get; }

        public abstract IList<IMemory> Memory { get; }

        public IMemory Flash => Memory.SingleOrDefault(x => x.Type == MemoryType.Flash);

        public IMemory Eeprom => Memory.SingleOrDefault(x => x.Type == MemoryType.Eeprom);
    }
}