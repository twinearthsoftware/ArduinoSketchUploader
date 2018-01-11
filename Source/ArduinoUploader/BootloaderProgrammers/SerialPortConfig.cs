using ArduinoUploader.BootloaderProgrammers.ResetBehavior;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal class SerialPortConfig
    {
        private const int DefaultTimeout = 1000;

        public SerialPortConfig(
            string portName,
            int baudRate,
            IResetBehavior preOpenResetBehavior,
            IResetBehavior postOpenResetBehavior,
            IResetBehavior closeResetAction,
            int sleepAfterOpen = 0,
            int readTimeout = DefaultTimeout,
            int writeTimeout = DefaultTimeout)
        {
            PortName = portName;
            BaudRate = baudRate;
            PreOpenResetBehavior = preOpenResetBehavior;
            PostOpenResetBehavior = postOpenResetBehavior;
            CloseResetAction = closeResetAction;
            SleepAfterOpen = sleepAfterOpen;
            ReadTimeOut = readTimeout;
            WriteTimeOut = writeTimeout;
        }

        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public IResetBehavior PreOpenResetBehavior { get; set; }
        public IResetBehavior PostOpenResetBehavior { get; set; }
        public IResetBehavior CloseResetAction { get; set; }
        public int SleepAfterOpen { get; set; }
        public int ReadTimeOut { get; set; }
        public int WriteTimeOut { get; set; }
    }
}