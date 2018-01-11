using System.Diagnostics.CodeAnalysis;

namespace ArduinoUploader.Config
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Protocol
    {
        Stk500v1,
        Stk500v2,
        Avr109
    }
}
