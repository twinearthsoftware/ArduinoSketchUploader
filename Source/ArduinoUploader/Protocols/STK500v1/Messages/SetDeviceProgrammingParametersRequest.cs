using ArduinoUploader.Hardware;

namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class SetDeviceProgrammingParametersRequest : Request
    {
        public SetDeviceProgrammingParametersRequest(MCU mcu)
        {
            var flashPageSize = mcu.FlashPageSize;
            var epromSize = mcu.EEPROMSize;
            var flashSize = mcu.FlashSize;

            Bytes = new byte[22];
            Bytes[0] = Constants.CMD_STK_SET_DEVICE;
            Bytes[1] = mcu.DeviceCode;
            Bytes[2] = mcu.DeviceRevision; 
            Bytes[3] = mcu.ProgType; 
            Bytes[4] = mcu.ParallelMode; 
            Bytes[5] = mcu.Polling;
            Bytes[6] = mcu.SelfTimed; 
            Bytes[7] = mcu.LockBytes;
            Bytes[8] = mcu.FuseBytes;
            Bytes[9] = mcu.FlashPollVal1; 
            Bytes[10] = mcu.FlashPollVal2; 
            Bytes[11] = mcu.EEPROMPollVal1;
            Bytes[12] = mcu.EEPROMPollVal2;
            Bytes[13] = (byte) ((flashPageSize >> 8) & 0x00ff);
            Bytes[14] = (byte) (flashPageSize & 0x00ff);
            Bytes[15] = (byte) ((epromSize >> 8) & 0x00ff);
            Bytes[16] = (byte) (epromSize & 0x00ff);
            Bytes[17] = (byte) ((flashSize >> 24) & 0xff);
            Bytes[18] = (byte) ((flashSize >> 16) & 0xff);
            Bytes[19] = (byte) ((flashSize >> 8) & 0xff);
            Bytes[20] = (byte) (flashSize & 0xff);
            Bytes[21] = Constants.SYNC_CRC_EOP;
        }
    }
}
