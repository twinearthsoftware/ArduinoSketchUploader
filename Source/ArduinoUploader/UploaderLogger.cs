using System;
using NLog;

namespace ArduinoUploader
{
    internal class UploaderLogger
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal static void LogAndThrowError<TException>(string errorMessage) where TException : Exception, new()
        {
            logger.Error(errorMessage);
            var exception = (TException) Activator.CreateInstance(typeof(TException), errorMessage);
            throw exception;
        }
    }
}
