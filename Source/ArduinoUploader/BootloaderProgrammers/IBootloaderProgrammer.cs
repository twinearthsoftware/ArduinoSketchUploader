using ArduinoUploader.Hardware;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal interface IBootloaderProgrammer
    {
        void Open();
        void Close();
        void EstablishSync();
        void CheckDeviceSignature();
        void InitializeDevice();
        void EnableProgrammingMode();
        void ProgramDevice();
        void ExecuteWritePage(MemoryType memType, int offset, byte[] bytes);
        byte[] ExecuteReadPage(MemoryType memType, int offset, int pageSize);
    }
}
