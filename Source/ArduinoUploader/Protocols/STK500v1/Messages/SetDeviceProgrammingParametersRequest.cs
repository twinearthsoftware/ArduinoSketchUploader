using ArduinoUploader.Protocols.STK500v1.HardwareConstants;

namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class SetDeviceProgrammingParametersRequest : Request
    {
        public SetDeviceProgrammingParametersRequest()
        {
            Bytes = new byte[22];
            Bytes[0] = CommandConstants.CommandConstants.Cmnd_STK_SET_DEVICE;
            Bytes[1] = ATMega328Constants.ATMEGA328__DEVCODE;
            Bytes[2] = 0; 
            Bytes[3] = 0; 
            Bytes[4] = 1; 
            Bytes[5] = 1;
            Bytes[6] = 1; 
            Bytes[7] = 1;
            Bytes[8] = ATMega328Constants.ATMEGA328_FUSE + ATMega328Constants.ATMEGA328_LFUSE + ATMega328Constants.ATMEGA328_HFUSE + ATMega328Constants.ATMEGA328_EFUSE;
            Bytes[9] = 0xff; 
            Bytes[10] = 0xff; 
            Bytes[11] = 0xff;
            Bytes[12] = 0xff;
            Bytes[13] = (ATMega328Constants.ATMEGA328_FLASH_PAGESIZE >> 8) & 0x00ff;
            Bytes[14] = ATMega328Constants.ATMEGA328_FLASH_PAGESIZE & 0x00ff;
            Bytes[15] = (ATMega328Constants.ATMEGA328_EPROM_SIZE >> 8) & 0x00ff;
            Bytes[16] = ATMega328Constants.ATMEGA328_EPROM_SIZE & 0x00ff;
            Bytes[17] = (ATMega328Constants.ATMEGA328_FLASH_SIZE >> 24) & 0xff;
            Bytes[18] = (ATMega328Constants.ATMEGA328_FLASH_SIZE >> 16) & 0xff;
            Bytes[19] = (ATMega328Constants.ATMEGA328_FLASH_SIZE >> 8) & 0xff;
            Bytes[20] = ATMega328Constants.ATMEGA328_FLASH_SIZE & 0xff;
            Bytes[21] = CommandConstants.CommandConstants.Sync_CRC_EOP;
        }
    }
}
