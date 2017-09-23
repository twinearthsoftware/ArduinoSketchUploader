using System;
using System.Collections.Generic;
using System.Linq;
using ArduinoUploader;
using ArduinoUploader.Hardware;
using Microsoft.Extensions.CommandLineUtils;
using Serilog;
using Serilog.Core;

namespace ArduinoSketchUploaderNetCore
{
    class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var logger = new ArduinoSketchUploaderLogger();

            var app = new CommandLineApplication {Name = "ArduinoSketchUploaderNetCore"};
            app.HelpOption("-?|-h|--help");

            var file = app.Option("-f|--file",
                "Path to the input file (in intel HEX format) which is to be uploaded to the Arduino.",
                CommandOptionType.SingleValue);

            var port = app.Option("-p|--port",
                "Name of the COM port where the Arduino is attached (e.g. 'COM1', 'COM2', 'COM3'...).",
                CommandOptionType.SingleValue);

            var modelOption = app.Option("-m|--model",
                "Arduino model. Valid parameters are any of the following: [Leonardo, Mega1284, Mega2560, Micro, NanoR2, NanoR3, UnoR3].",
                CommandOptionType.SingleValue);

            var errors = new List<string>();

            if (!Enum.TryParse(typeof(ArduinoModel), modelOption.Value(), out var model))
            {
                errors.Add("Missing valid arduino model");
            }

            if (!file.HasValue())
            {
                errors.Add("Missing file name");
            }

            if (errors.Any())
            {
                Console.WriteLine("Missing or wrong options provided");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                Console.ReadLine();
                Environment.Exit((int)StatusCodes.GeneralRuntimeException);
            }
            
            app.OnExecute(() =>
            {
                var options = new ArduinoSketchUploaderOptions
                {
                    PortName = port.Value(),
                    FileName = file.Value(),
                    ArduinoModel = (ArduinoModel)model
                };

                var progress = new Progress<double>(
                    p => logger.Info($"Programming progress: {p * 100:F1}% ..."));

                var uploader = new ArduinoSketchUploader(options, logger, progress);
                try
                {
                    uploader.UploadSketch();
                    return (int)StatusCodes.Success;
                }
                catch (ArduinoUploaderException)
                {
                    return (int)StatusCodes.ArduinoUploaderException;
                }
                catch (Exception ex)
                {
                    logger.Error($"Unexpected exception: {ex.Message}!", ex);
                    return (int)StatusCodes.GeneralRuntimeException;
                }
            });

        }
    }
}