using System;
using System.Linq;
using System.Threading;
using ArduinoUploader.Hardware;
using ArduinoUploader.Hardware.Memory;
using IntelHexFormatReader.Model;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class BootloaderProgrammer : IBootloaderProgrammer
    {
        protected IArduinoUploaderLogger Logger => ArduinoSketchUploader.Logger;

        protected IMcu Mcu { get; }

        protected BootloaderProgrammer(IMcu mcu)
        {
            Mcu = mcu;
        }

        public abstract void Open();
        public abstract void Close();
        public abstract void EstablishSync();
        public abstract void CheckDeviceSignature();
        public abstract void InitializeDevice();
        public abstract void EnableProgrammingMode();
        public abstract void LeaveProgrammingMode();
        public abstract void LoadAddress(IMemory memory, int offset);
        public abstract void ExecuteWritePage(IMemory memory, int offset, byte[] bytes);
        public abstract byte[] ExecuteReadPage(IMemory memory);

        public virtual void ProgramDevice(MemoryBlock memoryBlock, IProgress<double> progress = null)
        {
            var sizeToWrite = memoryBlock.HighestModifiedOffset + 1;
            var flashMem = Mcu.Flash;
            var pageSize = flashMem.PageSize;
            Logger?.Info($"Preparing to write {sizeToWrite} bytes...");
            Logger?.Info($"Flash page size: {pageSize}.");

            int offset;
            for (offset = 0; offset < sizeToWrite; offset += pageSize)
            {
                progress?.Report((double) offset / sizeToWrite);

                var needsWrite = false;
                for (var i = offset; i < offset + pageSize; i++)
                {
                    if (!memoryBlock.Cells[i].Modified) continue;
                    needsWrite = true;
                    break;
                }
                if (needsWrite)
                {
                    Logger?.Debug($"Executing paged write @ address {offset} (page size {pageSize})...");
                    var bytesToCopy = memoryBlock.Cells.Skip(offset).Take(pageSize).Select(x => x.Value).ToArray();

                    Logger?.Trace($"Checking if bytes at offset {offset} need to be overwritten...");
                    LoadAddress(flashMem, offset);
                    var bytesAlreadyPresent = ExecuteReadPage(flashMem);
                    if (bytesAlreadyPresent.SequenceEqual(bytesToCopy))
                    {
                        Logger?.Trace(
                            "Bytes to be written are identical to bytes already present - skipping actual write!");
                        continue;
                    }
                    Logger?.Trace($"Writing page at offset {offset}.");
                    LoadAddress(flashMem, offset);
                    ExecuteWritePage(flashMem, offset, bytesToCopy);

                    Logger?.Trace("Page written, now verifying...");
                    Thread.Sleep(10);
                    LoadAddress(flashMem, offset);
                    var verify = ExecuteReadPage(flashMem);
                    var succeeded = verify.SequenceEqual(bytesToCopy);
                    if (!succeeded)
                        throw new ArduinoUploaderException(
                            "Difference encountered during verification, write failed!");
                }
                else
                {
                    Logger?.Trace("Skip writing page...");
                }
            }
            Logger?.Info($"{sizeToWrite} bytes written to flash memory!");
        }
    }
}