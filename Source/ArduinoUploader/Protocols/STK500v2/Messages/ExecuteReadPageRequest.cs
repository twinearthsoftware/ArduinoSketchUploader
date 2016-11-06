using ArduinoUploader.Hardware;

namespace ArduinoUploader.Protocols.STK500v2.Messages
{
    internal class ExecuteReadPageRequest : Request
    {
        public ExecuteReadPageRequest(MemoryType memType, int pageSize)
        {
            Bytes = new[]
            {
                memType == MemoryType.FLASH ? Constants.CMD_READ_FLASH_PP : Constants.CMD_READ_EEPROM_PP,
                (byte)((pageSize >> 8) & 0xff),
                (byte)(pageSize & 0xff)
            };
        }
    }
}
