using System;
using System.Threading;
using System.Threading.Tasks;


namespace ugcp_svc.NXT
{
    static class CNXTDevice
    {
        public const uint HARDWARE_ID_NXT = 0x231d0200;
    }

    sealed class CNXTWrite : IDisposable
    {
        private struct ORDER
        {
            public byte[] buff = new byte[4];
            public ORDER() { }
        }

        private static CNXTWrite? pInstance = null;

        private readonly Lock mutexDriver = new();
        private string pathDriver = "";
        private Microsoft.Win32.SafeHandles.SafeFileHandle hDriver = new();
        private readonly System.Collections.Generic.Queue<ORDER> queue = new();
        private readonly object mutexQueue = new();
        private readonly Thread threadWk;
        private readonly CancellationTokenSource exit = new();

        private readonly byte[] hidPacket = new byte[81];

        private class StatusBaseLed
        {
            public byte Base;
            public byte[] Old1 = new byte[4];
            public byte[] Old2 = new byte[4];
        };
        private readonly StatusBaseLed statusBaseLed = new ();

        public CNXTWrite()
        {
            Interlocked.Exchange(ref pInstance, this);
            byte[] packetHeader = [0x59, 0xA5, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x01];
            packetHeader.AsSpan().CopyTo(hidPacket);
            threadWk = new(WkSend);
            threadWk.Start();
        }

        void IDisposable.Dispose()
        {
            Interlocked.Exchange(ref pInstance, null);
            exit.Cancel();
            lock (mutexQueue)
            {
                queue.Clear();
                Monitor.PulseAll(mutexQueue);
            }
            threadWk.Join();
            exit.Dispose();
        }

        public static CNXTWrite? Get() => pInstance;

        public void SetPath(ref string path)
        {
            lock (mutexDriver)
            {
                pathDriver = path;
            }
            if (path.Length != 0)
            {
                OpenDriver();
            }
        }

        private bool OpenDriver()
        {
            lock (mutexDriver)
            {
                var old = hDriver;
                hDriver = new();
                old.Dispose();
                if (pathDriver.Length == 0)
                {
                    return false;
                }
            }
            nint nhDriver = API.Win32.CreateFileW(pathDriver, API.Win32.GENERIC_READ | API.Win32.GENERIC_WRITE, API.Win32.FILE_SHARE_READ | API.Win32.FILE_SHARE_WRITE, nint.Zero, API.Win32.OPEN_EXISTING, 0, nint.Zero);
            Microsoft.Win32.SafeHandles.SafeFileHandle sh = new(nhDriver, true);
            if (!sh.IsInvalid)
            {
                lock (mutexDriver)
                {
                    hDriver = sh;
                }
                return true;
            }

            return false;
        }

        public void SetLed(Span<byte> _params)
        {
            if (_params[0] == 0)
	        {
		        byte mode = (byte)((_params[3] >> 5) & 0x7);
		        bool led2 = (mode == 0) || (mode == 5); //blue, color1
		        bool led1 = (mode == 1) || (mode == 6); //red, color2
		        if ((_params[3] & 0b0001_1100) == 0)
		        {
			        if (led1) statusBaseLed.Base &= 0b0010;
			        if (led2) statusBaseLed.Base &= 0b0001;
			        if ((statusBaseLed.Base & 0b0011) != 0)
			        {
                        if (statusBaseLed.Base == 1)
                        {
                            statusBaseLed.Old1.AsSpan().CopyTo(_params);
                        }
                        else
                        { 
                            statusBaseLed.Old2.AsSpan().CopyTo(_params);
                        }
			        }
		        }
		        else
		        {
			        statusBaseLed.Base |= (byte)(led1 ? 1 : 0);
			        statusBaseLed.Base |= (byte)(led2 ? 2 : 0);
			        if (led1) _params[..4].CopyTo(statusBaseLed.Old1.AsSpan());
			        if (led2) _params[..4].CopyTo(statusBaseLed.Old2.AsSpan());
		        }
		        if ((statusBaseLed.Base & 0b0011) == 3)
		        {
			        _params[1] = statusBaseLed.Old2[1];
			        _params[2] = (byte)(statusBaseLed.Old2[2] & 0b1);
			        _params[2] |= (byte)(statusBaseLed.Old1[2] & 0b1111_1110);
			        _params[3] &= 0b1111_1100;
			        _params[3] |= (byte)(statusBaseLed.Old1[3] & 0b11);
			        _params[3] = (byte)((_params[3] & 0b0001_1111) | (4 << 5));
		        }
	        }
	        SendOrder(_params);
        }

        private void SendOrder(ReadOnlySpan<byte> buffer)
        {
            ORDER order = new();
            buffer.CopyTo(order.buff);
            lock (mutexQueue)
            {
                queue.Enqueue(order);
                Monitor.Pulse(mutexQueue);
            }
        }

        private void WkSend()
        {
            while (true)
            {
                ORDER? order = null;
                lock (mutexQueue)
                {
                    while (!exit.IsCancellationRequested && queue.Count == 0)
                    {
                        Monitor.Wait(mutexQueue);
                    }
                    if (exit.IsCancellationRequested)
                    {
                        break;
                    }

                    order = queue.Dequeue();
                }

                if (!exit.IsCancellationRequested)
                {
                    for (byte i = 0; i < 2; i++)
                    {
                        lock (mutexDriver)
                        {
                            if (!hDriver.IsInvalid)
                            {
                                Span<byte> dest = hidPacket.AsSpan();
                                order.Value.buff.AsSpan().CopyTo(dest.Slice(8, 4));
                                ushort crc = CalculateCRC(hidPacket.AsSpan(5));
                                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(dest.Slice(3, 2), crc);
                                if (API.HID.HidD_SetFeature(hDriver.DangerousGetHandle(), hidPacket, 0x81))
                                {
                                    break;
                                }
                            }
                        }
                        if (!OpenDriver())
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static ushort CalculateCRC(ReadOnlySpan<byte> block)
        {
            ushort result = 0xffff; // 0xffff;

            for (byte i = 0; i < 6; i++)
            {
                byte v5 = block[i];
                result = (ushort)(v5 ^ result);
                for (byte j = 0; j < 8; j++)
                {
                    if ((result & 1) != 0)
                    {
                        result >>= 1;
                        result ^= 0xA001;
                    }
                    else
                    {
                        result >>= 1;
                    }
                }
            }
            return result;
        }
    }
}
