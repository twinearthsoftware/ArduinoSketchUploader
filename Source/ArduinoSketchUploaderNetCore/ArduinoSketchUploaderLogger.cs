using System;
using ArduinoUploader;
using Serilog;

namespace ArduinoSketchUploaderNetCore
{
    internal class ArduinoSketchUploaderLogger : IArduinoUploaderLogger
    {
        public void Error(string message, Exception exception)
        {
            Log.Error(exception, message);
        }

        public void Warn(string message)
        {
            Log.Warning(message);
        }

        public void Info(string message)
        {
            Log.Information(message);
        }

        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Trace(string message)
        {
            Log.Verbose(message);
        }
    }
}