using System;
using System.Threading;

namespace ugcp_svc.HIDInput
{
    class CPreprocess : IDisposable
    {
        public const byte CHANNEL_CAPACITY = 25;
        private readonly Profile.CProfile profile;
        private readonly ProcessInput.CProcessInput processInput;
        private readonly CHIDDevice device;
        private readonly CancellationTokenSource cts = new();
        private readonly Thread readThread;
        private readonly System.Threading.Channels.Channel<(byte[], short)> queue;
        public System.Buffers.ArrayPool<byte> PoolBuffer { get; } = System.Buffers.ArrayPool<byte>.Create(System.Runtime.InteropServices.Marshal.SizeOf<API.HID.HIDP_DATA>() * 140, CHANNEL_CAPACITY);//128 buttons, 8 axes, 4 hats


        public CPreprocess(Profile.CProfile refProfile, CHIDDevice refDevice)
        {
            profile = refProfile;
            device = refDevice;
            processInput = new(refProfile);
            queue = System.Threading.Channels.Channel.CreateBounded<(byte[], short)>(options: new(CHANNEL_CAPACITY)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait
            });
            readThread = new(() => ThreadRead(cts.Token))
            {
                Priority = ThreadPriority.Highest
            };
            readThread.Start();
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;

            disposed = true;
            queue.Writer.Complete();
            cts.Cancel();
            readThread.Join();
            processInput.Dispose();
        }

        public async System.Threading.Tasks.Task<bool> AddToQueue(byte[] buff, short size)
        {
            using CancellationTokenSource addCts = new(1000);
            try
            {
                await queue.Writer.WaitToWriteAsync(addCts.Token);
                queue.Writer.TryWrite((buff, size));
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("**Descartado");
                return false;
            }

            return true;
        }

        private async void ThreadRead(CancellationToken exit)
        {
            while(!exit.IsCancellationRequested)
            {
                (byte[], short) buff;
                try
                {
                    await queue.Reader.WaitToReadAsync(exit);
                    queue.Reader.TryRead(out buff);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                ReadOnlySpan<byte> span = buff.Item1;
                ConvertToCommon(span.Slice(0, buff.Item2));
            }
        }

        private void ConvertToCommon(ReadOnlySpan<byte> buffer)
        {
            HID_INPUT_DATA hidData = new();
            ReadOnlySpan<API.HID.HIDP_DATA> data = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, API.HID.HIDP_DATA>(buffer);

            device.LockDevice();
            {
                for (short idxData = 0; idxData < data.Length; idxData++)
                {
                    byte idxButton = 0;
                    byte idxAxis = 0;
                    byte idxHat = 0;
                    ReadOnlySpan<CHIDDevice.ST_MAP> mapSpan = device.GetMap();
                    for (short idxMap = 0; idxMap < mapSpan.Length; idxMap++)
                    {
                        CHIDDevice.ST_MAP mapIndex = mapSpan[idxMap];
                        if (mapIndex.IsButton)
                        {
                            if ((data[idxData].DataIndex >= mapIndex.Index) && (data[idxData].DataIndex < (mapIndex.Index + mapIndex.Bits)))
                            {
                                byte idx = (byte)(data[idxData].DataIndex - mapIndex.Index + idxButton);
                                if (idx > 63)
                                {
                                    hidData.Buttons[1] |= 1ul << (idx - 64);
                                }
                                else
                                {
                                    hidData.Buttons[0] |= 1ul << idx;
                                }
                            }
                            idxButton += mapIndex.Bits;
                        }
                        else if (mapIndex.IsHat)
                        {
                            if (data[idxData].DataIndex == mapIndex.Index)
                            {
                                byte hat = (byte)(mapIndex.IsHat ? 1 : 0);
                                hidData.Hats[idxHat] = (byte)(
                                    ((byte)data[idxData].Data.RawValue < (byte)(hat & 0xf)) || ((byte)data[idxData].Data.RawValue > (byte)(hat >> 4))
                                    ? 255
                                    : data[idxData].Data.RawValue - (hat & 0xf)
                                );
                            }
                            idxHat++;
                        }
                        else
                        {
                            if (data[idxData].DataIndex == mapIndex.Index)
                            {
                                hidData.Axis[idxAxis] = (ushort)(data[idxData].Data.RawValue);
                            }
                            idxAxis++;
                        }
                    }
                }
            }
            device.UnlockDevice();

            // Calibrate
            //if (!profile.CalibrationMode)
            {
                ProcessInput.CCalibration.Calibrate(profile, device.HardwareId, hidData);
            }

            processInput.Process(device.HardwareId, hidData);
        }

        //byte CPreprocess::Switch4To8(UCHAR in)
        //{
        //	switch (in)
        //	{
        //		case 0: return 0;
        //		case 1: return 1;
        //		case 2: return 3;
        //		case 3: return 2;
        //		case 4: return 5;
        //		case 6: return 4;
        //		case 8: return 7;
        //		case 9: return 8;
        //		case 12: return 6;
        //		default: return 0;
        //	}
        //}
    }
}
