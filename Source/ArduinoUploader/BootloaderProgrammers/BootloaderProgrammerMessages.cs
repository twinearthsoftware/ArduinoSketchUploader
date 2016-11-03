namespace ArduinoUploader.BootloaderProgrammers
{
    internal static class BootloaderProgrammerMessages
    {
        #region Arduino

        internal const string RESETTING_ARDUINO = "Resetting Arduino...";

        #endregion

        #region General device programming

        internal const string NO_SYNC = "Unable to establish sync.";
        internal const string NO_SYNC_WITH_RETRIES = "Unable to establish sync after {0} retries!";

        #endregion

        #region Serial

        internal const string TOGGLE_DTR_RTS = "Toggling DTR/RTS...";
        internal const string TIMEOUT = "Timeout - no response received.";

        #endregion

        #region STK500v2

        internal const string STK500v2_CORRUPT_WRAPPER = "STK500V2 wrapper corrupted - discarding received bytes!";

        #endregion
    }
}
