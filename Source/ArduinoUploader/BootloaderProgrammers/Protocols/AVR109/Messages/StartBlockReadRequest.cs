using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class StartBlockReadRequest : Request
    {
        internal StartBlockReadRequest(MemoryType memType, int blockSize)
        {
            Bytes = new[]
            {
                Constants.CmdStartBlockRead,
                (byte) (blockSize >> 8),
                (byte) (blockSize & 0xff),
                (byte) (memType == MemoryType.Flash ? 'F' : 'E')
            };
        }
    }
}