namespace ArduinoUploader.BootloaderProgrammers.Protocols.AVR109.Messages
{
    internal class ReturnSupportedDeviceCodesRequest : Request
    {
        internal ReturnSupportedDeviceCodesRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReturnSupportedDeviceCodes
            };
        }
    }
}