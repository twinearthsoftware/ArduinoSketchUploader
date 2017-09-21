using System;

namespace ArduinoUploader
{
    public class ArduinoUploaderException : Exception
    {
        public ArduinoUploaderException(string message) : base(message)
        {
        }
    }
}