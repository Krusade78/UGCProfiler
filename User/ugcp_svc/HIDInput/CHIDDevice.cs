using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using API;

namespace ugcp_svc.HIDInput
{
    class CHIDDevice : IHIDInput, IDisposable
    {
        public struct ST_MAP
        {
            public byte Bits;
            public bool IsButton;
            public bool IsHat;
            public ushort Index;
        }

        private readonly List<ST_MAP> map = []; //usar CollectionsMarshal.AsSpan(list)
        public uint HardwareId { get; private set; }
        protected string pathInterface = string.Empty;
        protected Lock mutex = new();
        protected Shared.RAII.UniqueHandle hdev = new();
        protected nint preparsed = nint.Zero;
        protected byte[] reportBuffer = [];
        private uint getDataSize = 0;
        private byte powerSuspended = 0;
        private readonly CancellationTokenSource exit = new();
        private readonly Thread thRead;
        private readonly CPreprocess processHID;

        public CHIDDevice(uint hardwareId, Profile.CProfile refProfile)
        {
            HardwareId = hardwareId;
            processHID = new(refProfile, this);
            thRead = new(() => ThreadRead(exit.Token))
            {
                Priority = ThreadPriority.Highest
            };
            thRead.Start();
        }

        private byte disposed;
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1) return;

            SetSuspended(true);
            exit.Cancel();
            Close();
            HardwareId = 0;
            thRead.Join();
            processHID.Dispose();
        }

        public void LockDevice() { mutex.Enter(); }
        public void UnlockDevice() { mutex.Exit(); }
        public ReadOnlySpan<ST_MAP> GetMap() => CollectionsMarshal.AsSpan(map);
        public void SetSuspended(bool onoff) => Interlocked.Exchange(ref powerSuspended, (byte)(onoff  ? 1 : 0));

        protected override bool Prepare()
        {
            Close();

            nint diDevs;
            Guid hidGuid = new();
            SetupDi.SP_DEVICE_INTERFACE_DATA diData = new();

            HID.HidD_GetHidGuid(ref hidGuid);
            diDevs = SetupDi.SetupDiGetClassDevsW(ref hidGuid, null, nint.Zero, SetupDi.DIGCF_PRESENT | SetupDi.DIGCF_DEVICEINTERFACE);
            if (diDevs == Win32.INVALID_HANDLE_VALUE)
            {
                return false;
            }

            diData.cbSize = Marshal.SizeOf<SetupDi.SP_DEVICE_INTERFACE_DATA>();
            uint idx = 0;
            while (SetupDi.SetupDiEnumDeviceInterfaces(diDevs, nint.Zero, ref hidGuid, idx++, ref diData))
            {
                if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, nint.Zero, 0, out uint size, nint.Zero) && (122 != Marshal.GetLastWin32Error()))
                {
                    SetupDi.SetupDiDestroyDeviceInfoList(diDevs);
                    return false;
                }

                using Shared.RAII.HGlobalHandle buf = new((int)size);
                Marshal.WriteInt32(buf.DangerousGetHandle(), 8); //x64
                if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, buf.DangerousGetHandle(), size, out size, nint.Zero))
                {
                    SetupDi.SetupDiDestroyDeviceInfoList(diDevs);
                    return false;
                }

                string? ninterface = Marshal.PtrToStringAuto(buf.DangerousGetHandle() + 4);
                if (ninterface == null)
                {
                    SetupDi.SetupDiDestroyDeviceInfoList(diDevs);
                    return false;
                }

                uint hId = GetHardwareId(ref ninterface);
                lock(mutex)
                {
                    if (hId == HardwareId)
                    {
                        pathInterface = ninterface;
                        break;
                    }
                }

            }

            SetupDi.SetupDiDestroyDeviceInfoList(diDevs);
            return Marshal.GetLastWin32Error() == 0;
        }

        protected override bool Open()
        {
            lock (mutex)
            {
                if ((HardwareId != 0) && !string.IsNullOrEmpty(pathInterface) && hdev.IsInvalid)
                {
                    using Shared.RAII.UniqueHandle nhdev = new(Win32.CreateFileW(pathInterface, Win32.GENERIC_READ, Win32.FILE_SHARE_READ | Win32.FILE_SHARE_WRITE, nint.Zero, Win32.OPEN_EXISTING, 0, nint.Zero));
                    if (nhdev.IsInvalid)
                    {
                        return false;
                    }

                    uint newReportLenght = 0;
                    nint newPrepased = nint.Zero;
                    if (HID.HidD_GetPreparsedData(nhdev.DangerousGetHandle(), ref newPrepased))
                    {
                        using Shared.RAII.HGlobalHandle caps = new(Marshal.SizeOf<HID.HIDP_CAPS>());
                        if (HID.HidP_GetCaps(newPrepased, caps.DangerousGetHandle()) == HID.HIDP_STATUS_SUCCESS)
                        {
                            if (GetDeviceMap(newPrepased, caps.DangerousGetHandle()))
                            {
                                HID.HIDP_CAPS mcaps = Marshal.PtrToStructure<HID.HIDP_CAPS>(caps.DangerousGetHandle());
                                newReportLenght = mcaps.InputReportByteLength;
                            }
                            if (newReportLenght != 0)
                            {
                                byte[] dummyReport = new byte[newReportLenght];
                                if (HID.HidP_GetData(0, nint.Zero, ref getDataSize, newPrepased, dummyReport, (uint)dummyReport.Length) != HID.HIDP_STATUS_BUFFER_TOO_SMALL)
                                {
                                    newReportLenght = 0;
                                    getDataSize = 0;
                                }
                            }
                        }
                        if (newReportLenght != 0)
                        {
                            reportBuffer = new byte[newReportLenght];
                            preparsed = newPrepased;
                            hdev = new(nhdev.Move());
                            if (HardwareId == NXT.CNXTDevice.HARDWARE_ID_NXT)
                            {
                                NXT.CNXTWrite.Get()?.SetPath(ref pathInterface);
                            }
                        }
                    }
                }
            }

            return true;
        }

        protected override void Close()
        {
            lock(mutex)
            {
                using Shared.RAII.UniqueHandle old = new(hdev.Move());
                if (!old.IsInvalid)
                {
                    if (HardwareId == NXT.CNXTDevice.HARDWARE_ID_NXT)
                    {
                        string empty = string.Empty;
                        NXT.CNXTWrite.Get()?.SetPath(ref empty);
                    }
                    Win32.CancelIoEx(old.DangerousGetHandle(), nint.Zero);
                }
                reportBuffer = [];
                if (preparsed != nint.Zero)
                {
                    HID.HidD_FreePreparsedData(preparsed);
                    preparsed = nint.Zero;
                }
                pathInterface = string.Empty;
                map.Clear();
            }
        }

        protected bool GetDeviceMap(nint pData, nint pCaps)
        {
            HID.HIDP_CAPS caps = Marshal.PtrToStructure<HID.HIDP_CAPS>(pCaps);
            HID.HIDP_BUTTON_CAPS[] bCaps = new HID.HIDP_BUTTON_CAPS[caps.NumberInputButtonCaps];
            HID.HIDP_VALUE_CAPS[] vCaps = new HID.HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

            #region GetCaps
            if (caps.NumberInputButtonCaps != 0)
            {
                ushort ustam = caps.NumberInputButtonCaps;
                using Shared.RAII.HGlobalHandle buf = new(Marshal.SizeOf<HID.HIDP_BUTTON_CAPS>() * ustam);
                if (HID.HidP_GetButtonCaps(0, buf.DangerousGetHandle(), ref ustam, pData) != HID.HIDP_STATUS_SUCCESS)
                {
                    return false;
                }
                for (int i = 0; i < ustam; i++)
                {
                    bCaps[i] = Marshal.PtrToStructure<HID.HIDP_BUTTON_CAPS>(buf.DangerousGetHandle() + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                }
            }
            if (caps.NumberInputValueCaps != 0)
            {
                ushort ustam = caps.NumberInputValueCaps;
                using Shared.RAII.HGlobalHandle buf = new(Marshal.SizeOf<HID.HIDP_BUTTON_CAPS>() * ustam);
                if (HID.HidP_GetValueCaps(0, buf.DangerousGetHandle(), ref ustam, pData) != HID.HIDP_STATUS_SUCCESS)
                {
                    return false;
                }
                for (int i = 0; i < ustam; i++)
                {
                    vCaps[i] = Marshal.PtrToStructure<HID.HIDP_VALUE_CAPS>(buf.DangerousGetHandle() + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                }
            }
            #endregion

            byte btId = 0;
            byte axId = 0;
            byte hatId = 0;
            for (ushort idx = 0; idx < caps.NumberInputDataIndices; idx++)
            {
                for (ushort i = 0; i < caps.NumberInputButtonCaps; i++)
                {
                    HID.HIDP_BUTTON_CAPS bt = bCaps[i];
                    if (bt.Anonymous.Range.DataIndexMin == idx)
                    {
                        ST_MAP bmap = new()
                        {
                            //bmap->ReportId = bt.ReportID;
                            Bits = (byte)(bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1),
                            IsButton = true,
                            IsHat = false,
                            Index = bt.Anonymous.Range.DataIndexMin
                        };
                        //bmap->Skip = FALSE;
                        map.Add(bmap);
                        btId++;
                        if (btId == 128) { return false; }
                        break;
                    }
                }

                for (ushort i = 0; i < caps.NumberInputValueCaps; i++)
                {
                    HID.HIDP_VALUE_CAPS val = vCaps[i];
                    if (val.Anonymous.NotRange.DataIndex == idx)
                    {
                        if ((val.Anonymous.NotRange.Usage == 0) || (val.Anonymous.NotRange.Usage == 1))
                        {
                            //ST_MAP* amap = new ST_MAP;
                            //amap->ReportId = val.ReportID;
                            //amap->Bits = static_cast<UCHAR>(val.BitSize);
                            //amap->IsButton = FALSE;
                            //amap->IsHat = FALSE;
                            //amap->Index = val.NotRange.DataIndex;
                            //amap->Skip = TRUE;
                            //map.push_back(amap);
                        }
                        else
                        {
                            if ((val.Anonymous.NotRange.Usage != 57) && (val.LogicalMin != 0)) { return false; }
                            if ((val.Anonymous.NotRange.Usage == 57) && (val.LogicalMax + 1 - val.LogicalMin) != 8 && (val.LogicalMax + 1 - val.LogicalMin) != 4) { return false; }
                            ST_MAP amap = new()
                            {
                                //amap->ReportId = val.ReportID;
                                Bits = (byte)val.BitSize,
                                IsButton = false,
                                IsHat = (val.Anonymous.NotRange.Usage == 57) && ((val.LogicalMin | ((val.LogicalMax & 0xf) << 4)) != 0),
                                Index = val.Anonymous.NotRange.DataIndex
                            };
                            //amap->Skip = FALSE;
                            map.Add(amap);
                            if (!amap.IsHat) { axId++; } else { hatId++; }
                            if ((axId == 24) || (hatId == 4)) { return false; }
                        }
                        break;
                    }
                }
            }

            return true;
        }

        public override ushort Read(Span<byte> buff)
        {
            bool opened;
            bool prepared;
            lock (mutex)
            {
                if (HardwareId == 0) //disposing
                {
                    return 0;
                }
                prepared = !string.IsNullOrEmpty(pathInterface);
                opened = !hdev.IsInvalid;
            }
            if (!prepared)
            {
                Prepare();
            }
            if (!opened)
            { 
                Open();
                System.Threading.Tasks.Task.Delay(1500).Wait();
                return 0;
            }
            mutex.Enter();
            {
                if (!hdev.IsInvalid)
                {
                    mutex.Exit();
                    if (Win32.ReadFile(hdev.DangerousGetHandle(), reportBuffer, (uint)reportBuffer.Length, out _, nint.Zero))
                    {
                        lock (mutex)
                        {
                            uint size = getDataSize;
                            if ((preparsed != nint.Zero) && (HID.HidP_GetData(0, buff, ref size, preparsed, reportBuffer, (uint)reportBuffer.Length) == HID.HIDP_STATUS_SUCCESS))
                            {
                                return (ushort)size;
                            }
                        }
                    }
                }
                else
                {
                    mutex.Exit();
                }
            }

            System.Threading.Tasks.Task.Delay(500).Wait();
            return 0;
        }

        private async void ThreadRead(CancellationToken token)
        {
            int hidp_size = Marshal.SizeOf<HID.HIDP_DATA>();
            int buffer_size = Marshal.SizeOf<HID.HIDP_DATA>() * 140;
            while (!token.IsCancellationRequested)
            {
                if (Interlocked.CompareExchange(ref powerSuspended, 0, 0) == 1)
                {
                    Thread.Sleep(500);
                    continue;
                }
                byte[] buff = processHID.PoolBuffer.Rent(buffer_size);
                ushort tam = Read(buff);
                if (tam > 0)
                {
                    if (await processHID.AddToQueue(buff, (short)(tam * hidp_size)))
                    {
                        continue;
                    }
                }
                processHID.PoolBuffer.Return(buff);
            }
        }
    }
}
