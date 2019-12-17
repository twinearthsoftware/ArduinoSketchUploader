using ArduinoUploader.Hardware;

namespace ArduinoUploader
{
    public class ArduinoSketchUploaderOptions
    {
        public string FileName { get; set; }

        public string PortName { get; set; }

        public ArduinoModel ArduinoModel { get; set; }

        public bool TriggerBootloader { get; set; }
    }
}