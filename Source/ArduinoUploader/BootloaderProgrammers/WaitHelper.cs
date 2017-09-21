using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoUploader.BootloaderProgrammers
{
    internal static class WaitHelper
    {
        private static IArduinoUploaderLogger Logger => ArduinoSketchUploader.Logger;

        internal static T WaitFor<T>(int timeout, int interval, Func<T> toConsider, Func<int, T, int, string> format)
            where T : class
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var task = Task.Run(() =>
            {
                var i = 0;
                while (!token.IsCancellationRequested)
                {
                    var item = toConsider();
                    Logger?.Info(format(i, item, interval));
                    if (item != null) return item;
                    i++;
                    Thread.Sleep(interval);
                }
                return null;
            }, token);

            if (task.Wait(TimeSpan.FromMilliseconds(timeout)))
                return task.Result;

            // Timeout expired, nothing matched...
            cancellationTokenSource.Cancel();
            return null;
        }
    }
}