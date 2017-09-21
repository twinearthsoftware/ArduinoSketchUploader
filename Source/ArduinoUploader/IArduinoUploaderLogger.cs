using System;

namespace ArduinoUploader
{
    public interface IArduinoUploaderLogger
    {
        void Error(string message, Exception exception);

        void Warn(string message);

        void Info(string message);

        void Debug(string message);

        void Trace(string message);
    }
}
