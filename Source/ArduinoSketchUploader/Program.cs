using ArduinoUploader;

namespace ArduinoSketchUploader
{
    /// <summary>
    /// The ArduinoLibCSharp SketchUploader can upload a compiled (Intel) HEX file directly to an attached Arduino.
    /// 
    /// This code was heavily inspired by avrdude's STK500 implementation.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var commandLineOptions = new CommandLineOptions();
            if (!CommandLine.Parser.Default.ParseArguments(args, commandLineOptions)) return;

            var options = new ArduinoSketchUploaderOptions
            {
                PortName = commandLineOptions.PortName,
                FileName = commandLineOptions.FileName
            };
            var uploader = new ArduinoUploader.ArduinoSketchUploader(options);
            uploader.UploadSketch();
        }
    }

}
