namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class SelectDeviceTypeRequest : Request
    {
        internal SelectDeviceTypeRequest(byte deviceCode)
        {
            Bytes = new[]
            {
                Constants.CmdSelectDeviceType,
                deviceCode
            };
        }
    }
}