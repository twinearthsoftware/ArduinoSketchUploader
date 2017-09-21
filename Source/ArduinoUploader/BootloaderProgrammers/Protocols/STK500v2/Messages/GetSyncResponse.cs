using System;
using System.Text;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class GetSyncResponse : Response
    {
        internal bool IsInSync => Bytes.Length > 1 
            && Bytes[0] == Constants.CmdSignOn && Bytes[1] == Constants.StatusCmdOk;

        internal string Signature
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