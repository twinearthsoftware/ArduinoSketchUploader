using System;
using System.Threading;
using ArduinoUploader.Hardware;
using ArduinoUploader.Protocols;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class SerialPortBootloaderProgrammer : BootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected UploaderSerialPort SerialPort { get; private set; }

        protected SerialPortBootloaderProgrammer(UploaderSerialPort serialPort, MCU mcu)
            : base(mcu)
        {
            SerialPort = serialPort;
        }

        protected void ToggleDtrRts(int wait1, int wait2)
        {
            logger.Trace("Toggling DTR/RTS...");

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
                "Sending {0} bytes: {1}{2}", 
                length, Environment.NewLine, BitConverter.ToString(bytes));
            SerialPort.Write(bytes, 0, length);
        }

        protected int ReceiveNext()
        {
            var bytes = new byte[1];
            try
            {
                SerialPort.Read(bytes, 0, 1);
                logger.Trace(
                    "Receiving byte: {0}",
                    BitConverter.ToString(bytes));
                return bytes[0];
            }
            catch (TimeoutException)
            {
                return -1;
            }
        }

        protected byte[] ReceiveNext(int length)
        {
            var bytes = new byte[length];
            try
            {
                SerialPort.WaitForBytes(length);
                SerialPort.Read(bytes, 0, length);
                logger.Trace("Receiving byte: {0}", BitConverter.ToString(bytes));
                return bytes;
            }
            catch (TimeoutException)
            {
                return null;
            }            
        }
    }
}
