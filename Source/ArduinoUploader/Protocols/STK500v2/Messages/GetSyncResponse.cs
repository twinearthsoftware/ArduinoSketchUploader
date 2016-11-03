using System;
using System.Text;

namespace ArduinoUploader.Protocols.STK500v2.Messages
{
    internal class GetSyncResponse : Response
    {
        public bool IsInSync
        {
            get
            {
                return Bytes.Length > 1 
                    && Bytes[0] == Constants.Cmnd_SIGN_ON 
                    && Bytes[1] == Constants.Status_CMD_OK;
            }
        }

        public string Signature
        {
            get
            {
                var signatureLength = Bytes[2];
                var signature = new byte[signatureLength];
                Buffer.BlockCopy(Bytes, 3, signature, 0, signatureLength);
                return Encoding.ASCII.GetString(signature);
            }
        }
    }
}
