namespace ArduinoUploader.Hardware
{
    internal interface MCU
    {
        int FlashSize { get; set; }
        int EpromSize { get; set; }
    }
}
