using System;
using System.IO.Ports;
using System.Threading;

namespace ArduinoUploader
{
    internal class UploaderSerialPort : SerialPort
    {
        private readonly object serialDataIncoming;
        private int numberOfBytesToRead;

        public UploaderSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {
            serialDataIncoming = new object();
            DataReceived += OnDataReceived;
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var availableBytes = BytesToRead;
            if (availableBytes < numberOfBytesToRead) return;
            lock (serialDataIncoming)
            {
                Monitor.Pulse(serialDataIncoming);
            }
        }

        public void WaitForBytes(int numberOfBytes)
        {
            numberOfBytesToRead = numberOfBytes;
            lock (serialDataIncoming)
            {
                if (!Monitor.Wait(serialDataIncoming, ReadTimeout))
                    throw new TimeoutException();
            }
        }
    }
}
