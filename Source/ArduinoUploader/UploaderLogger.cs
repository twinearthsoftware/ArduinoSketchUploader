using System;
using ArduinoUploader.Logging;

namespace ArduinoUploader
{
    internal class UploaderLogger
    {
        private static readonly ILog logger = LogProvider.For<UploaderLogger>();

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
