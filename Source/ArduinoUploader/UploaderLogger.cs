using System;
using NLog;

namespace ArduinoUploader
{
    internal class UploaderLogger
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal static void LogError(string errorMessage, Exception ex)
        {
            logger.Error(ex, errorMessage);
        }

        internal static void LogErrorAndThrow(string errorMessage) 
        {
            logger.Error(errorMessage);
            throw new ArduinoUploaderException(errorMessage);
        }
    }
}
