using System;
using ArduinoUploader;
using ArduinoUploader.Hardware;
using McMaster.Extensions.CommandLineUtils;
using NLog;

namespace ArduinoSketchUploader
{
    /// <summary>
    /// The ArduinoSketchUploader can upload a compiled (Intel) HEX file directly to an attached Arduino.
    /// </summary>
    internal class Program
    {
        private class ArduinoSketchUploaderLogger : IArduinoUploaderLogger
        {
            private static readonly Logger Logger = LogManager.GetLogger("ArduinoSketchUploader");

            public void Error(string message, Exception exception)
            {
                Logger.Error(exception, message);
            }

            public void Warn(string message)
            {
                Logger.Warn(message);
            }

            public void Info(string message)
            {
                Logger.Info(message);
            }

            public void Debug(string message)
            {
                Logger.Debug(message);
            }

            public void Trace(string message)
            {
                Logger.Trace(message);
            }
        }

        private static int Main(string[] args)
        {
            using (var app = new CommandLineApplication())
            {
                app.HelpOption("-h|--help");
                var arduinoModel = app.Option<ArduinoModel>("-m|--model <MODEL>",
                    "Device model",
                    CommandOptionType.SingleValue).IsRequired();
                var arduinoPort = app.Option<string>("-p|--port <PORT>",
                    "Name of the COM port where the Arduino is attached (e.g. 'COM1', 'COM2', 'COM3'...).",
                    CommandOptionType.SingleValue);
                var firmwareFile = app.Option<string>("-f|--file <FILE>",
                     "Arduino model. Valid parameters are any of the following: [Leonardo, Mega1284, Mega2560, Micro, NanoR2, NanoR3, UnoR3].",
                    CommandOptionType.SingleValue).IsRequired();

                app.OnExecute(() =>
                {
                    var options = new ArduinoSketchUploaderOptions
                    {
                        PortName = arduinoPort.ParsedValue,
                        FileName = firmwareFile.ParsedValue,
                        ArduinoModel = arduinoModel.ParsedValue
                    };

                    var logger = new ArduinoSketchUploaderLogger();
                    var progress = new Progress<double>(
                        p => logger.Info($"Upload progress: {p * 100:F1}% ..."));

                    var uploader = new ArduinoUploader.ArduinoSketchUploader(options, logger, progress);
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

                return app.Execute(args);
            }
        }

        private enum StatusCodes
        {
            Success,
            ArduinoUploaderException,
            GeneralRuntimeException
        }
    }
}