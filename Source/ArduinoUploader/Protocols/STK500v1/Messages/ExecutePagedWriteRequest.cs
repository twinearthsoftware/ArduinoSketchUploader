using System;

namespace ArduinoUploader.Protocols.STK500v1.Messages
{
    internal class ExecutePagedWriteRequest : Request
    {
        public ExecutePagedWriteRequest(int pageSize, int blockSize, byte[] bytesToCopy)
        {
            Bytes = new byte[blockSize + 5];
            var i = 0;
            Bytes[i++] = Constants.CMD_STK_PROG_PAGE;
            Bytes[i++] = (byte)((blockSize >> 8) & 0xff);
            Bytes[i++] = (byte)(blockSize & 0xff);
            Bytes[i++] = (byte)'F';
            Buffer.BlockCopy(bytesToCopy, 0, Bytes, i, pageSize);
            i += blockSize;
            Bytes[i] = Constants.SYNC_CRC_EOP;
        }
    }
}
