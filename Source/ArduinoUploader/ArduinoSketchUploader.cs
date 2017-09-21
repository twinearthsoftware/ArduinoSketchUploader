using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArduinoUploader.BootloaderProgrammers;
using ArduinoUploader.BootloaderProgrammers.Protocols.AVR109;
using ArduinoUploader.BootloaderProgrammers.Protocols.STK500v1;
using ArduinoUploader.BootloaderProgrammers.Protocols.STK500v2;
using ArduinoUploader.BootloaderProgrammers.ResetBehavior;
using ArduinoUploader.Hardware;
using IntelHexFormatReader;
using IntelHexFormatReader.Model;
using RJCP.IO.Ports;

namespace ArduinoUploader
{
    public class ArduinoSketchUploader
    {
        internal static IArduinoUploaderLogger Logger { get; set; }

        private readonly ArduinoSketchUploaderOptions _options;
        private readonly IProgress<double> _progress;

        public ArduinoSketchUploader(ArduinoSketchUploaderOptions options, 
            IArduinoUploaderLogger logger = null, IProgress<double> progress = null)
        {
            Logger = logger;
            Logger?.Info("Starting ArduinoSketchUploader...");
            _options = options;
            _progress = progress;
        }

        public void UploadSketch()
        {
            var hexFileName = _options.FileName;
            string[] hexFileContents;
            Logger?.Info($"Starting upload process for file '{hexFileName}'.");
            try
            {
                hexFileContents = File.ReadAllLines(hexFileName);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message, ex);
                throw;
            }
            UploadSketch(hexFileContents);
        }

        public void UploadSketch(IEnumerable<string> hexFileContents)
        {
            try
            {
                var serialPortName = _options.PortName;
                var allPortNames = SerialPortStream.GetPortNames();
                var distinctPorts = allPortNames.Distinct().ToList();

                // If we don't specify a COM port, automagically select one if there is only a single match.
                if (string.IsNullOrWhiteSpace(serialPortName) && distinctPorts.SingleOrDefault() != null)
                {
                    Logger?.Info($"Port autoselected: {serialPortName}.");
                    serialPortName = distinctPorts.Single();
                }
                // Or else, check that we have an unambiguous match. Throw an exception otherwise.
                else if (!allPortNames.Any() || distinctPorts.SingleOrDefault(
                             x => x.Equals(serialPortName, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    throw new ArduinoUploaderException(
                        $"Specified COM port name '{serialPortName}' is not valid.");
                }

                Logger?.Trace($"Creating serial port '{serialPortName}'...");
                ArduinoBootloaderProgrammer programmer;
                IMcu mcu;
                SerialPortConfig serialPortConfig;

                switch (_options.ArduinoModel)
                {
                    case ArduinoModel.Mega1284:
                    {
                        mcu = new AtMega1284();
                        serialPortConfig = new SerialPortConfig(serialPortName, 115200,
                            new ResetThroughEnablingDtrBehavior(), new ResetThroughTogglingDtrRtsBehavior(250, 50), 250);
                        programmer = new Stk500V1BootloaderProgrammer(serialPortConfig, mcu);
                        break;
                    }
                    case ArduinoModel.Mega2560:
                    {
                        mcu = new AtMega2560();
                        serialPortConfig = new SerialPortConfig(serialPortName, 115200,
                            new ResetThroughEnablingDtrBehavior(), new ResetThroughTogglingDtrRtsBehavior(50, 50, true), 50);
                        programmer = new Stk500V2BootloaderProgrammer(serialPortConfig, mcu);
                        break;
                    }
                    case ArduinoModel.Leonardo:
                    case ArduinoModel.Micro:
                    {
                        mcu = new AtMega32U4();
                        serialPortConfig =
                            new SerialPortConfig(serialPortName, 57600, new ResetThrough1200BpsBehavior(), null);
                        programmer = new Avr109BootloaderProgrammer(serialPortConfig, mcu);
                        break;
                    }
                    case ArduinoModel.NanoR2:
                    {
                        mcu = new AtMega168();
                        serialPortConfig = new SerialPortConfig(serialPortName, 19200,
                            new ResetThroughEnablingDtrBehavior(), new ResetThroughTogglingDtrRtsBehavior(250, 50), 250);
                        programmer = new Stk500V1BootloaderProgrammer(serialPortConfig, mcu);
                        break;
                    }
                    case ArduinoModel.NanoR3:
                    {
                        mcu = new AtMega328P();
                        serialPortConfig = new SerialPortConfig(serialPortName, 57600,
                            new ResetThroughEnablingDtrBehavior(), new ResetThroughTogglingDtrRtsBehavior(250, 50), 250);
                        programmer = new Stk500V1BootloaderProgrammer(serialPortConfig, mcu);
                        break;
                    }
                    case ArduinoModel.UnoR3:
                    {
                        mcu = new AtMega328P();
                        serialPortConfig = new SerialPortConfig(serialPortName, 115200,
                            new ResetThroughEnablingDtrBehavior(), new ResetThroughTogglingDtrRtsBehavior(250, 50), 250);
                        programmer = new Stk500V1BootloaderProgrammer(serialPortConfig, mcu);
                        break;
                    }
                    default:
                    {
                        throw new ArduinoUploaderException($"Unsupported model: {_options.ArduinoModel}!");
                    }
                }

                try
                {
                    programmer.Open();

                    Logger?.Info("Establishing sync...");
                    programmer.EstablishSync();
                    Logger?.Info("Sync established.");

                    Logger?.Info("Checking device signature...");
                    programmer.CheckDeviceSignature();
                    Logger?.Info("Device signature checked.");

                    Logger?.Info("Initializing device...");
                    programmer.InitializeDevice();
                    Logger?.Info("Device initialized.");

                    Logger?.Info("Enabling programming mode on the device...");
                    programmer.EnableProgrammingMode();
                    Logger?.Info("Programming mode enabled.");

                    Logger?.Info("Programming device...");
                    programmer.ProgramDevice(ReadHexFile(hexFileContents, mcu.Flash.Size), _progress);
                    Logger?.Info("Device programmed.");

                    Logger?.Info("Leaving programming mode...");
                    programmer.LeaveProgrammingMode();
                    Logger?.Info("Left programming mode!");
                }
                finally
                {
                    programmer.Close();
                }
                Logger?.Info("All done, shutting down!");
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message, ex);
                throw;
            }
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
                Logger?.Error(ex.Message, ex);
                throw;
            }
        }

        #endregion
    }
}