using ArduinoUploader.Hardware;

namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class SetDeviceProgrammingParametersRequest : Request
    {
        public SetDeviceProgrammingParametersRequest(ATMegaMCU atMegaMcu)
        {
            var flashPageSize = atMegaMcu.FlashPageSize;
            var epromSize = atMegaMcu.EpromSize;
            var flashSize = atMegaMcu.FlashSize;

            Bytes = new byte[22];
            Bytes[0] = Constants.Cmnd_STK_SET_DEVICE;
            Bytes[1] = atMegaMcu.DevCode;
            Bytes[2] = 0; 
            Bytes[3] = 0; 
            Bytes[4] = 1; 
            Bytes[5] = 1;
            Bytes[6] = 1; 
            Bytes[7] = 1;
            Bytes[8] = (byte) (atMegaMcu.Fuse + atMegaMcu.LFuse + atMegaMcu.HFuse + atMegaMcu.EFuse);
            Bytes[9] = 0xff; 
            Bytes[10] = 0xff; 
            Bytes[11] = 0xff;
            Bytes[12] = 0xff;
            Bytes[13] = (byte) ((flashPageSize >> 8) & 0x00ff);
            Bytes[14] = (byte) (flashPageSize & 0x00ff);
            Bytes[15] = (byte) ((epromSize >> 8) & 0x00ff);
            Bytes[16] = (byte) (epromSize & 0x00ff);
            Bytes[17] = (byte) ((flashSize >> 24) & 0xff);
            Bytes[18] = (byte) ((flashSize >> 16) & 0xff);
            Bytes[19] = (byte) ((flashSize >> 8) & 0xff);
            Bytes[20] = (byte) (flashSize & 0xff);
            Bytes[21] = Constants.Sync_CRC_EOP;
        }
    }
}
