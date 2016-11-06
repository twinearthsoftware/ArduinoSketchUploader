using System;
using System.Linq;
using ArduinoUploader.Hardware;
using IntelHexFormatReader.Model;
using NLog;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal abstract class BootloaderProgrammer : IBootloaderProgrammer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected MCU MCU { get; private set; }
        protected MemoryBlock MemoryBlock { get; private set; }

        protected BootloaderProgrammer(Func<int, MemoryBlock> memoryBlockGenerator, MCU mcu)
        {
            MemoryBlock = memoryBlockGenerator(mcu.FlashSize);
            MCU = mcu;
        }

        public abstract void Open();
        public abstract void Close();
        public abstract void EstablishSync();
        public abstract void CheckDeviceSignature();
        public abstract void InitializeDevice();
        public abstract void EnableProgrammingMode();
        public abstract void ExecuteWritePage(MemoryType memType, int offset, byte[] bytes);
        public abstract byte[] ExecuteReadPage(MemoryType memType, int offset, int pageSize);

        public virtual void ProgramDevice()
        {
            var sizeToWrite = MemoryBlock.HighestModifiedOffset + 1;
            var pageSize = MCU.FlashPageSize;
            logger.Info("Preparing to write {0} bytes...", sizeToWrite);
            logger.Info("Flash page size: {0}.", pageSize);

            int offset;
            for (offset = 0; offset < sizeToWrite; offset += pageSize)
            {
                var needsWrite = false;
                for (var i = offset; i < offset + pageSize; i++)
                {
                    if (!MemoryBlock.Cells[i].Modified) continue;
                    needsWrite = true;
                    break;
                }
                if (needsWrite)
                {
                    logger.Trace("Executing paged write from address {0} (page size {1})...", offset, pageSize);
                    var bytesToCopy = MemoryBlock.Cells.Skip(offset).Take(pageSize).Select(x => x.Value).ToArray();

                    logger.Trace("Checking if bytes at offset {0} need to be overwritten...", offset);
                    var bytesAlreadyPresent = ExecuteReadPage(MemoryType.FLASH, offset, pageSize);
                    if (bytesAlreadyPresent.SequenceEqual(bytesToCopy))
                    {
                        logger.Trace("Bytes to be written are identical to bytes already present - skipping actual write!");
                        continue;
                    }
                    logger.Trace("Writing page at offset {0}.", offset);
                    ExecuteWritePage(MemoryType.FLASH, offset, bytesToCopy);
                }
                else
                {
                    logger.Trace("Skip writing page...");
                }
            }
            logger.Info("{0} bytes written to flash memory!", sizeToWrite);
        }
    }
}
