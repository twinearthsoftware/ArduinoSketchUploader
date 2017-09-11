using System;
using ArduinoUploader;
using NLog;

namespace ArduinoSketchUploader
{
    /// <summary>
    /// The ArduinoSketchUploader can upload a compiled (Intel) HEX file directly to an attached Arduino.
    /// </summary>
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetLogger("ArduinoSketchUploader");

        private enum StatusCodes
        {
            Success,
            ArduinoUploaderException,
            GeneralRuntimeException
        }

        private static void Main(string[] args)
        {
            var commandLineOptions = new CommandLineOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, commandLineOptions)) return;

            var options = new ArduinoSketchUploaderOptions
            {
                PortName = commandLineOptions.PortName,
                FileName = commandLineOptions.FileName,
                ArduinoModel = commandLineOptions.ArduinoModel
            };
            var progress = new Progress<double>(p => logger.Info("{0:F1}%", p * 100));
            var uploader = new ArduinoUploader.ArduinoSketchUploader(options, progress);
            try
            {
                uploader.UploadSketch();
                Environment.Exit((int)StatusCodes.Success);
            }
            catch (ArduinoUploaderException)
            {
                Environment.Exit((int)StatusCodes.ArduinoUploaderException);
            }
            catch (Exception ex)
            {
                UploaderLogger.LogError(string.Format("Unexpected exception: {0}!", ex.Message), ex);
                Environment.Exit((int)StatusCodes.GeneralRuntimeException);
            }
        }
    }

}
