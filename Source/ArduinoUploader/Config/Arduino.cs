using System.Xml.Serialization;

namespace ArduinoUploader.Config
{
    public class Arduino
    {
        [XmlAttribute("model")]
        public string Model { get; set; }

        public McuIdentifier Mcu { get; set; } 

        public int BaudRate { get; set; }

        public string PreOpenResetBehavior { get; set; }

        public string PostOpenResetBehavior { get; set; }

        public string CloseResetBehavior { get; set; }

        public int SleepAfterOpen { get; set; } = 0;

        public int ReadTimeout { get; set; } = 1000;

        public int WriteTimeout { get; set; } = 1000;

        public Protocol Protocol { get; set; }
    }
}
