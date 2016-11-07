using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using ArduinoUploader.BootloaderProgrammers;
using ArduinoUploader.Hardware;
using IntelHexFormatReader;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader
{
    public class ArduinoSketchUploader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ArduinoSketchUploaderOptions options;
        private UploaderSerialPort serialPort;

        private const int SerialPortTimeOut = 1000;

        public ArduinoSketchUploader(ArduinoSketchUploaderOptions options)
        {
            logger.Info("Starting ArduinoSketchUploader...");
            this.options = options;
        }

        public void UploadSketch()
        {
            var hexFileName = options.FileName;
            logger.Info("Starting upload process for file '{0}'.", hexFileName);
            var hexFileContents = File.ReadAllLines(hexFileName);
            UploadSketch(hexFileContents);
        }

        public void UploadSketch(IEnumerable<string> hexFileContents)
        {
            var serialPortName = options.PortName;

            var ports = SerialPort.GetPortNames();

            if (!ports.Any() || ports.Distinct().SingleOrDefault(
                x => x.Equals(serialPortName, StringComparison.OrdinalIgnoreCase)) == null)
            {
                UploaderLogger.LogAndThrowError<ArgumentException>(
                    string.Format("Specified COM port name '{0}' is not valid.", serialPortName));
            }

            logger.Trace("Creating serial port '{0}'...", serialPortName);
            SerialPortBootloaderProgrammer programmer = null;

            IMCU mcu = null;

            switch (options.ArduinoModel)
            {
                case ArduinoModel.Mega2560:
                {
                    mcu = new ATMega2560();
                    serialPort = new UploaderSerialPort(serialPortName, 115200);
                    programmer = new WiringBootloaderProgrammer(serialPort, mcu);
                    break;
                }
                case ArduinoModel.NanoR3:
                {
                    mcu = new ATMega328P();
                    serialPort = new UploaderSerialPort(serialPortName, 57600);
                    programmer = new OptibootBootloaderProgrammer(serialPort, mcu);
                    break;
                }
                case ArduinoModel.UnoR3:
                {
                    mcu = new ATMega328P();
                    serialPort = new UploaderSerialPort(serialPortName, 115200);
                    programmer = new OptibootBootloaderProgrammer(serialPort, mcu);
                    break;
                }
                default:
                {
                    UploaderLogger.LogAndThrowError<IOException>(
                        string.Format("Unsupported model: {0}!", options.ArduinoModel));
                    break;
                }
            }
            try
            {
                TryToOpenSerialPort();
                ConfigureSerialPort();

                programmer.Open();

                logger.Info("Establishing sync...");
                programmer.EstablishSync();
                logger.Info("Sync established.");

                logger.Info("Checking device signature...");
                programmer.CheckDeviceSignature();
                logger.Info("Device signature checked.");

                logger.Info("Initializing device...");
                programmer.InitializeDevice();
                logger.Info("Device initialized.");

                logger.Info("Enabling programming mode on the device...");
                programmer.EnableProgrammingMode();
                logger.Info("Programming mode enabled.");

                logger.Info("Programming device...");
                programmer.ProgramDevice(ReadHexFile(hexFileContents, mcu.Flash.Size));
                logger.Info("Device programmed.");

                programmer.Close();
            }
            finally
            {
                CloseSerialPort();
            }
            logger.Info("All done, shutting down!");
        }

        #region Private Methods

        private static MemoryBlock ReadHexFile(IEnumerable<string> hexFileContents, int memorySize)
        {
            try
            {
                var reader = new HexFileReader(hexFileContents, memorySize);
                return reader.Parse();
            }
            catch (Exception ex)
            {
                UploaderLogger.LogAndThrowError<IOException>(ex.Message);
            }
            return null;
        }

        private void TryToOpenSerialPort()
        {
            logger.Trace("Opening serial port...");
            try
            {
                serialPort.Open();
            }
            catch (UnauthorizedAccessException)
            {
                UploaderLogger.LogAndThrowError<UnauthorizedAccessException>(
                    "Access to the port is denied. This or another process is currently using this port.");
            }
            catch (ArgumentOutOfRangeException)
            {
                UploaderLogger.LogAndThrowError<ArgumentOutOfRangeException>(
                    "The configuration parameters for the port are invalid (e.g. baud rate, parity, databits).");
            }
            catch (IOException)
            {
                UploaderLogger.LogAndThrowError<IOException>("The port is in an invalid state.");
            }
            logger.Trace("Opened serial port {0} with baud rate {1}!", serialPort.PortName, serialPort.BaudRate);
        }

        private void CloseSerialPort()
        {
            logger.Info("Closing serial port...");
            serialPort.DtrEnable = false;
            serialPort.RtsEnable = false;
            try
            {
                serialPort.Close();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void ConfigureSerialPort()
        {
            logger.Trace("Setting Read/Write Timeout on serial port to '{0}'.", SerialPortTimeOut);
            serialPort.ReadTimeout = SerialPortTimeOut;
            serialPort.WriteTimeout = SerialPortTimeOut;
        }

        #endregion
    }
}
