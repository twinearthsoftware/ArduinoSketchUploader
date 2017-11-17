using System;
using ArduinoUploader;

namespace ArduinoSketchUploaderNetCore
{
    internal class ArduinoSketchUploaderLogger : IArduinoUploaderLogger
    {
        public void Error(string message, Exception exception)
        {
            Console.WriteLine(Prefix(message, "ERROR"));
            Console.WriteLine(Prefix(exception.StackTrace, "ERROR"));
        }

        public void Warn(string message)
        {
            Console.WriteLine(Prefix(message, "WARN"));
        }

        public void Info(string message)
        {
            Console.WriteLine(Prefix(message, "INFO"));
        }

        public void Debug(string message)
        {
            Console.WriteLine(Prefix(message, "DEBUG"));
        }

        public void Trace(string message)
        {
            Console.WriteLine(Prefix(message, "TRACE"));
        }

        private string Prefix(string message, string logLevel)
        {
            var now = DateTime.Now;
            return $"[{now.ToString("yyyy-MM-dd HH:mm:ss")}][ArduinoSketchUploader][{logLevel.PadRight(5)}] - {message}";
        }
    }
}