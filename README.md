# ArduinoSketchUploader

This repository contains a .NET library and a corresponding Windows command line utility that can be used to upload a compiled sketch (.HEX) directly to an Arduino board over USB. It talks to the boards bootloader over the serial (USB) connection, much like *avrdude* does (when invoked from the Arduino IDE, or from the command line).

![ArduinoSketchUploader](https://github.com/christophediericx/ArduinoSketchUploader/blob/master/Images/ArduinoSketchUploader.png)

## Compatibility ##

The library has been tested with the following configurations:

| Arduino Model | MCU           | Bootloader protocol |
| ------------- |:-------------:| -------------------:|
| Mega 2560     | ATMega2560    | [STK500v2](/Documentation/STK500v2.pdf)            |
| Micro         | ATMega32U4    | [AVR109](/Documentation/AVR109.pdf)                |
| Nano (R3)     | ATMega328P    | [STK500v1](/Documentation/STK500v1.pdf)            |
| Uno (R3)      | ATMega328P    | [STK500v1](/Documentation/STK500v1.pdf)            |

> *If you have a need for this library to run on another Arduino model, feel free to open an issue on GitHub, it should be relatively straightforward to add support (for most).*

## How to use the command line application ##

[Download the latest Windows binaries here (.zip file, version 2.1.0).](https://github.com/christophediericx/ArduinoSketchUploader/releases/download/v2.1.0/ArduinoSketchUploader-2.1.0.zip)

When running *ArduinoSketchUploader.exe* without arguments, the application will document it's usage:

```
ArduinoSketchUploader 2.2.0.0
Copyright c  2016

ERROR(S):
  -f/--file required option is missing.
  -p/--port required option is missing.
  -m/--model required option is missing.


  -f, --file     Required. Path to the input file (in intel HEX format) which
                 is to be uploaded to the Arduino.

  -p, --port     Required. Name of the COM port where the Arduino is attached
                 (e.g. 'COM1', 'COM2', 'COM3'...).

  -m, --model    Required. Arduino model. Valid parameters are any of the
                 following: [Mega2560, Micro, NanoR3, UnoR3].

  --help         Display this help screen.
```
A sample command line invocation (for a Mega2560 type Arduino attached to COM4):

```
ArduinoSketchUploader.exe --file=C:\MyHexFile\myHexFile.hex --port=COM4 --model=Mega2560
```

## How to use the .NET library ##

[![NuGet version](https://badge.fury.io/nu/ArduinoUploader.svg)](https://badge.fury.io/nu/ArduinoUploader)

Link the following nuget package in your project in order to use the ArduinoUploader: https://www.nuget.org/packages/ArduinoUploader/

Alternatively, install the package using the nuget package manager console:

```
Install-Package ArduinoUploader
```

The following minimal snippet shows how to upload a .HEX file to an Arduino (UNO) board attached at COM port 3:

```csharp
var uploader = new ArduinoSketchUploader(
    new ArduinoSketchUploaderOptions()
    {
        FileName = @"C:\MyHexFiles\UnoHexFile.ino.hex",
        PortName = "COM3",
        ArduinoModel = ArduinoModel.UnoR3
    });

uploader.UploadSketch();
```

## Logging ##

The library emits log messages (in varying levels, from *Info* to *Trace*) via NLog. Hook up an NLog dependency (and configuration) in any project that uses *ArduinoSketchUploader* to automagically emit these messages as well.

