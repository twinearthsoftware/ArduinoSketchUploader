# ArduinoSketchUploader

This repository contains both a .NET library and a Windows command line utility to upload a compiled sketch (.HEX file) directly to an Arduino board (without having to use the Arduino IDE or avrdude).

> *Compatibility note: This library has only been tested with UNO (ATMega328P) based Arduino boards. It is expected that tweaking of hardware constants in the STK-500 bootloader communication is required in order to support other architectures.*

## How to use the command line application ##

[Download the latest Windows binaries here (.zip file, version 1.0.3).](https://github.com/christophediericx/ArduinoSketchUploader/files/515179/ArduinoSketchUploader-1.0.3.zip)

When running *ArduinoSketchUploader.exe* without arguments, the application will document it's usage:

```
ArduinoSketchUploader 1.0.0.0
Copyright c  2016

ERROR(S):
  -f/--file required option is missing.
  -p/--port required option is missing.


  -f, --file    Required. Path to the input file (in intel HEX format) which is
                to be uploaded to the Arduino.

  -p, --port    Required. Name of the COM port where the Arduino is attached
                (e.g. 'COM1', 'COM2', 'COM3'...).

  --help        Display this help screen.
```


## How to use the .NET library ##

Link the following nuget package in your project in order to use the ArduinoUploader: https://www.nuget.org/packages/ArduinoUploader/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoUploader
```

The library talks to the Arduino's bootloader directly through a dialect of the STK-500 protocol in order to flash the memory on the device with the contents of an Intel HEX file. This solution is fully self-contained (and native C#) and not just a wrapper for avrdude.

The following minimal snippet shows how to upload a .hex file to an Arduino (UNO) board with the library:

```csharp
using ArduinoUploader;

namespace ArduinoUploaderDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var uploader = new ArduinoSketchUploader(
                new ArduinoSketchUploaderOptions()
                {
                    FileName = @"C:\MyHexFiles\MyHexFile.hex",
                    PortName = "COM3"
                });

            uploader.UploadSketch();
        }
    }
}
```

