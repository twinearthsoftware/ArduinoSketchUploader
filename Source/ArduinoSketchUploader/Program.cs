using System;
using ArduinoUploader;
using CommandLine;

namespace ArduinoSketchUploaderNetCore
{
    class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ArduinoSketchUploaderLogger();

            Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(cmdlineOptions =>
            {
                var progress = new Progress<double>(
                    p => logger.Info($"Programming progress: {p * 100:F1}% ..."));


                var options = new ArduinoSketchUploaderOptions
                {
                    ArduinoModel = cmdlineOptions.ArduinoModel,
                    FileName = cmdlineOptions.FileName,
                    PortName = cmdlineOptions.PortName
                };

                var uploader = new ArduinoSketchUploader(options, logger, progress);
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
                    logger.Error($"Unexpected exception: {ex.Message}!", ex);
                    Environment.Exit((int)StatusCodes.GeneralRuntimeException);
                }
            }).WithNotParsed(options =>
                Environment.Exit((int)StatusCodes.FailedToParseCommandLineArgs)); ;
        }
    }
}