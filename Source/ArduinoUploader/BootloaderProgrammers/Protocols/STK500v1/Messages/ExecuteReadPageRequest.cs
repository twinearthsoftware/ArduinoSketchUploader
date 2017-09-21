using ArduinoUploader.Hardware.Memory;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1.Messages
{
    internal class ExecuteReadPageRequest : Request
    {
        internal ExecuteReadPageRequest(MemoryType memType, int pageSize)
        {
            Bytes = new byte[5];
            Bytes[0] = Constants.CmdStkReadPage;
            Bytes[1] = (byte) ((pageSize >> 8) & 0xff);
            Bytes[2] = (byte) (pageSize & 0xff);
            Bytes[3] = (byte) (memType == MemoryType.Eeprom ? 'E' : 'F');
            Bytes[4] = Constants.SyncCrcEop;
        }
    }
}