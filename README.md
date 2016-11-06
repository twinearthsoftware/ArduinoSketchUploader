# ArduinoSketchUploader

This repository contains both a .NET library and a Windows command line utility to upload a compiled sketch (.HEX file) directly to an Arduino board over USB. It talks to the onboard bootloader over a serial connection, much like *avrdude* would do (e.g. when invoked from the Arduino IDE).

## Compatibility ##

The library has been tested with the following configurations only:

| Arduino Model | MCU           | Bootloader protocol |
| ------------- |:-------------:| -------------------:|
| Uno (R3)      | ATMega328P    | STK500v1            |
| Mega 2560     | ATMega2560    | STK500v2            |

> *These are the boards I have myself at the moment. If you have a need for this library to run on another Arduino board, feel free to open a support issue.*

## How to use the command line application ##

[Download the latest Windows binaries here (.zip file, version 2.0.2).](https://github.com/christophediericx/ArduinoSketchUploader/releases/download/v2.0.2/ArduinoSketchUploader-2.0.2.zip)

When running *ArduinoSketchUploader.exe* without arguments, the application will document it's usage:

```
ArduinoSketchUploader 2.0.2.0
Copyright c  2016

ERROR(S):
  -f/--file required option is missing.
  -p/--port required option is missing.
  -m/--model required option is missing.


  -f, --file     Required. Path to the input file (in intel HEX format) which
                 is to be uploaded to the Arduino.

  -p, --port     Required. Name of the COM port where the Arduino is attached
                 (e.g. 'COM1', 'COM2', 'COM3'...).

  -m, --model    Required. Arduino model. Valid parameters are one of the
                 following: [UnoR3, Mega2560].

  --help         Display this help screen.
```


## How to use the .NET library ##

Link the following nuget package in your project in order to use the ArduinoUploader: https://www.nuget.org/packages/ArduinoUploader/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoUploader
```

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

