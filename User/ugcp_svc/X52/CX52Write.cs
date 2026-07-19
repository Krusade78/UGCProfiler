using System;
using System.Threading;


namespace ugcp_svc.X52
{
    sealed class CX52Write : IDisposable
    {
        private struct ORDER
        {
            public ushort Value;
            public byte Idx;
        }

        private static CX52Write? pInstance = null;

        private readonly byte[] Date = [0, 0];
        private nint pwhUSB = nint.Zero;
        private readonly System.Collections.Generic.Queue<ORDER> queue = new();
        private readonly object mutexQueue = new();
        private readonly Thread threadWk;
        private readonly CancellationTokenSource exit = new();

        public CX52Write()
        {
            Interlocked.Exchange(ref pInstance, this);
            threadWk = new(WkSend)
            {
                Priority = ThreadPriority.BelowNormal
            };
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

        public static CX52Write? Get() => Interlocked.CompareExchange(ref pInstance, null, null);
        public void SetWinUSB(nint whUSB) => Interlocked.Exchange(ref pwhUSB, whUSB);

        #region "Orders"
        public void Light_MFD(byte SystemBuffer)
        {
            Span<byte> _params = [SystemBuffer, 0, 0xb1];
            SendOrder(_params, 1);
        }

        public void Light_Global(byte SystemBuffer)
        {
            Span<byte> _params = [SystemBuffer, 0, 0xb2];
            SendOrder(_params, 1);
        }

        public void Light_Info(byte SystemBuffer)
        {
            Span<byte> _params = [(byte)(SystemBuffer + 0x50), 0, 0xb4];
            SendOrder(_params, 1);
        }

        public void Set_Pinkie(byte SystemBuffer)
        {
            Span<byte> _params = [(byte)(SystemBuffer + 0x50), 0, 0xfd];
            SendOrder(_params, 1);
        }

        public void Set_Text(ReadOnlySpan<byte> SystemBuffer)
        {
            if ((SystemBuffer.Length - 1) > 16)
                return;
            
            Span<byte> _params = stackalloc byte[3 * 17];
            _params.Clear();
            var text = SystemBuffer[1..];
            byte nparams = 1;
            byte paramIdx = 0;

            _params[0] = 0; _params[1] = 0;
            switch (SystemBuffer[0]) //line
            {
                case 1:
                    _params[2] = 0xd9;
                    paramIdx = 0xd1;
                    break;
                case 2:
                    _params[2] = 0xda;
                    paramIdx = 0xd2;
                    break;
                case 3:
                    _params[2] = 0xdc;
                    paramIdx = 0xd4;
                    break;
            }
            for (byte i = 0; i < 16; i += 2)
            {
                if (text[i] == 0)
                    break;

                byte offset = (byte)(3 * nparams);
                _params[0 + offset] = text[i];
                _params[1 + offset] = text[i + 1];
                _params[2 + offset] = paramIdx;
                nparams++;
            }

            SendOrder(_params, nparams);
        }

        /// <param name="SystemBuffer">size = 3</param>
        public void Set_Hour(ReadOnlySpan<byte> SystemBuffer)
        {
            Span<byte> _params = [SystemBuffer[2], SystemBuffer[1], (byte)(SystemBuffer[0] + 0xbf)];
            SendOrder(_params, 1);
        }

        /// <param name="SystemBuffer">size = 3</param>
        public void Set_Hour24(ReadOnlySpan<byte> SystemBuffer)
        {
            Span<byte> _params = [SystemBuffer[2], (byte)(SystemBuffer[1] + 0x80), (byte)(SystemBuffer[0] + 0xbf)];
            SendOrder(_params, 1);
        }

        /// <param name="SystemBuffer">size = 2</param>
        public void Set_Date(ReadOnlySpan<byte> SystemBuffer)
        {
            Span<byte> _params = stackalloc byte[3];

            switch (SystemBuffer[0])
            {
                case 1:
                    _params[2] = 0xc4;
                    _params[1] = Date[1];
                    _params[0] = SystemBuffer[1];
                    Date[0] = _params[0]; Date[1] = _params[1];
                    break;
                case 2:
                    _params[2] = 0xc4;
                    _params[1] = SystemBuffer[1];
                    _params[0] = Date[0];
                    Date[0] = _params[0]; Date[1] = _params[1];
                    break;
                case 3:
                    _params[2] = 0xc8;
                    _params[1] = 0;
                    _params[0] = SystemBuffer[1];
                    break;
            }
            SendOrder(_params, 1);
        }
        #endregion

        private void SendOrder(ReadOnlySpan<byte> buffer, byte packets)
        {
            for (byte processed = 0; processed < packets; processed++)
            {
                ORDER order = new() {
                    Value = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(processed * 3, 2)),
                    Idx = buffer[2 + (processed * 3)]
                };
                lock (mutexQueue)
                {
                    queue.Enqueue(order);
                    Monitor.Pulse(mutexQueue);
                }
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
                    API.CWinUSB.WINUSB_SETUP_PACKET controlSetupPacket = new()
                    {
                        RequestType = 0b01000000,
                        Request = 0x91, // Request
                        Value = order.Value.Value, // Value
                        Index = order.Value.Idx, // Index  
                        Length = 0
                    };

                    nint hUSB = Volatile.Read(ref pwhUSB);
                    if (hUSB != nint.Zero)
                    {
                        API.CWinUSB.WinUsb_ControlTransfer(hUSB, controlSetupPacket, nint.Zero, 0, nint.Zero, nint.Zero);
                    }
                }
            }
        }
    }
}
