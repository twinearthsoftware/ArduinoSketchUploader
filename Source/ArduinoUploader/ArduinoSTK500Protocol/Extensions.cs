using System;
using System.IO;
using ArduinoUploader.ArduinoSTK500Protocol.Messages;

namespace ArduinoUploader.ArduinoSTK500Protocol
{
    internal static class Extensions
    {
        internal static void EstablishSync(this UploaderSerialPort port)
        {
            int i;
            for (i = 0; i < UploaderSerialPort.MaxSyncRetries; i++)
            {
                port.Send(new GetSyncRequest());
                var result = port.Receive<GetSyncResponse>();
                if (result == null) continue;
                if (result.IsInSync) break;
            }

            if (i == UploaderSerialPort.MaxSyncRetries)
                UploaderLogger.LogAndThrowError<IOException>(string.Format("Unable to establish sync after {0} retries!", UploaderSerialPort.MaxSyncRetries));

            var nextByte = port.ReceiveNext();

            if (nextByte != CommandConstants.CommandConstants.Resp_STK_OK)
                UploaderLogger.LogAndThrowError<IOException>("Unable to establish sync.");
        }

        internal static void Send(this UploaderSerialPort port, IRequest request)
        {
            var bytes = request.Bytes;
            port.Write(bytes, 0, bytes.Length);
        }

        internal static void SendWithSyncRetry(this UploaderSerialPort port, IRequest request)
        {
            byte nextByte;
            while (true)
            {
                port.Send(request);
                nextByte = (byte) port.ReceiveNext();
                if (nextByte == CommandConstants.CommandConstants.Resp_STK_NOSYNC)
                {
                    port.EstablishSync();
                    continue;
                }
                break;
            }
            if (nextByte != CommandConstants.CommandConstants.Resp_STK_INSYNC)
                UploaderLogger.LogAndThrowError<IOException>(
                    string.Format("Unable to aqcuire sync in SendWithSyncRetry for request of type {0}!", request.GetType()));
        }

        internal static TResponse Receive<TResponse>(this UploaderSerialPort port, int length = 1) where TResponse : Response
        {
            var bytes = new byte[length];
            try
            {
                port.Read(bytes, 0, length);
                var result = (TResponse) Activator.CreateInstance(typeof(TResponse));
                result.Bytes = bytes;
                return result;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        internal static int ReceiveNext(this UploaderSerialPort port)
        {
            var bytes = new byte[1];
            try
            {
                port.Read(bytes, 0, 1);
                return bytes[0];
            }
            catch (TimeoutException)
            {
                return -1;
            }
        }
    }
}
