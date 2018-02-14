using System;
using System.Linq;

namespace ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2.Messages
{
    internal class ExecuteSpiCommandRequest : Request
    {
        internal ExecuteSpiCommandRequest(byte numTx, byte numRx, byte rxStartAddr, byte[] txData)
        {
            var data = new byte[numTx];
            Buffer.BlockCopy(txData, 0, data, 0, numTx);
            var header = new[]
            {
                Constants.CmdSpiMulti,
                numTx,
                numRx,
                rxStartAddr
            };
            Bytes = header.Concat(data).ToArray();
        }
    }
}
