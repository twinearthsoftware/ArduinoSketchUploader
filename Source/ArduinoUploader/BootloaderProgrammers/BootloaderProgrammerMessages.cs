namespace ArduinoUploader.BootloaderProgrammers
{
    internal static class BootloaderProgrammerMessages
    {
        #region Arduino

        internal const string RESETTING_ARDUINO = "Resetting Arduino...";

        #endregion

        #region General device programming

        internal const string ESTABLISH_SYNC = "Establishing sync...";
        internal const string SYNC_ESTABLISHED = "Sync established.";
        internal const string NO_SYNC = "Unable to establish sync.";
        internal const string NO_SYNC_WITH_RETRIES = "Unable to establish sync after {0} retries.";
        internal const string SEND_WITH_SYNC_RETRY_FAILURE = "Unable to aqcuire sync in SendWithSyncRetry for request of type {0}!";

        internal const string CHECK_DEVICE_SIG = "Checking device signature...";
        internal const string CHECK_DEVICE_SIG_FAILURE = "Unable to check device signature!";
        internal const string DEVICE_SIG_CHECKED = "Device signature checked.";
        internal const string DEVICE_SIG_EXPECTED = "Expecting to find '{0}'...";
        internal const string UNEXPECTED_DEVICE_SIG = "Unexpected device signature - found '{0}'- expected '{1}'.";

        internal const string ENABLE_PROGMODE = "Enabling programming mode on the device...";
        internal const string PROGMODE_ENABLED = "Programming mode enabled.";
        internal const string ENABLE_PROGMODE_FAILURE = "Unable to enable programming mode on the device!";

        internal const string INITIALIZE_DEVICE = "Initializing device...";
        internal const string DEVICE_INITIALIZED = "Device initialized.";

        internal const string SET_DEVICE_PARAMS = "Setting device programming parameters...";
        internal const string SET_DEVICE_PARAMS_FAILURE = "Unable to set device programming parameters!";

        internal const string SOFTWARE_VERSION = "Retrieved software version: {0}.";

        internal const string GET_PARAM = "Retrieving parameter '{0}'...";
        internal const string GET_PARAM_FAILED = "Retrieving parameter '{0}' failed!";
        internal const string GET_PARAM_FAILED_PROTOCOL = "General protocol error while retrieving parameter '{0}'";

        #endregion

        #region Serial

        internal const string TOGGLE_DTR_RTS = "Toggling DTR/RTS...";
        internal const string TIMEOUT = "Timeout - no response received after {0}ms.";

        #endregion

        #region STK500v2

        internal const string STK500v2_CORRUPT_WRAPPER = "STK500V2 wrapper corrupted ({0})!";
        internal const string STK500v2_NO_START_MESSAGE = "No Start Message detected!";
        internal const string STK500v2_WRONG_SEQ_NUMBER = "Wrong sequence number!";
        internal const string STK500v2_TIMEOUT = "Timeout ocurred!";
        internal const string STK500v2_TOKEN_NOT_RECEIVED = "Token not received!";
        internal const string STK500v2_MESSAGE_NOT_RECEIVED = "Inner message not received!";
        internal const string STK500v2_CHECKSUM_NOT_RECEIVED = "Checksum not received!";
        internal const string STK500v2_CHECKSUM_INCORRECT = "Checksum incorrect!";

        #endregion
    }
}
