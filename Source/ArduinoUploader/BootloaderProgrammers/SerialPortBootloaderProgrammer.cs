using System;
using System.IO;
using System.Linq;
using System.Threading;
using ArduinoUploader.Protocols;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class SerialPortBootloaderProgrammer : BootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected UploaderSerialPort SerialPort { get; private set; }

        protected SerialPortBootloaderProgrammer(UploaderSerialPort serialPort, MemoryBlock memoryBlock)
            : base(memoryBlock)
        {
            SerialPort = serialPort;
        }

        protected void ToggleDtrRts(int wait1, int wait2)
        {
            logger.Debug(BootloaderProgrammerMessages.TOGGLE_DTR_RTS);

            SerialPort.DtrEnable = false;
            SerialPort.RtsEnable = false;

            Thread.Sleep(wait1);

            SerialPort.DtrEnable = true;
            SerialPort.RtsEnable = true;

            Thread.Sleep(wait2);    
        }

        protected virtual void Send(IRequest request)
        {
            var bytes = request.Bytes;
            var length = bytes.Length;
            logger.Trace(
                "Sending {0} bytes: {1}{2}{3}{4}", 
                length, 
                Environment.NewLine, BitConverter.ToString(bytes),
                Environment.NewLine, string.Join("-", bytes.Select(x => " " + Convert.ToChar(x))));
            SerialPort.Write(bytes, 0, length);
        }

        protected virtual void SendWithSyncRetry(IRequest request, Func<byte,bool> noSync, Func<byte,bool> inSync)
        {
            byte nextByte;
            while (true)
            {
                Send(request);
                nextByte = (byte) ReceiveNext();
                if (noSync(nextByte))
                {
                    EstablishSync();
                    continue;
                }
                break;
            }
            if (!inSync(nextByte))
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Unable to aqcuire sync in SendWithSyncRetry for request of type {0}!", request.GetType()));
        }

        protected virtual TResponse Receive<TResponse>(int length = 1) where TResponse : Response
        {
            var bytes = new byte[length];
            try
            {
                SerialPort.Read(bytes, 0, length);
                logger.Trace(
                    "Received {0} bytes: {1}{2}{3}{4}",
                    length,
                    Environment.NewLine, BitConverter.ToString(bytes),
                    Environment.NewLine, string.Join("-", bytes.Select(x => " " + Convert.ToChar(x))));
                var result = (TResponse) Activator.CreateInstance(typeof(TResponse));
                result.Bytes = bytes;
                return result;
            }
            catch (TimeoutException)
            {
                logger.Trace(BootloaderProgrammerMessages.TIMEOUT);
                return null;
            }
        }

        protected int ReceiveNext()
        {
            var bytes = new byte[1];
            try
            {
                SerialPort.Read(bytes, 0, 1);
                return bytes[0];
            }
            catch (TimeoutException)
            {
                return -1;
            }
        }
    }
}
