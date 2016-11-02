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
    }
}
