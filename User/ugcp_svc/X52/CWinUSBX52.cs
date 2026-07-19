using System;
using System.Runtime.InteropServices;
using API;

namespace ugcp_svc.X52
{
    sealed class CWinUSBX52 : HIDInput.CHIDDevice
    {
        //constexpr auto HARDWARE_ID_X52 = L"\\\\?\\USB#VID_06A3&PID_0255";
        public const uint HARDWARE_ID_X52 = 0x063a0255;
        private Guid guidInterface = new("{ 0xA57C1168, 0x7717, 0x4AF0, { 0xB3, 0x0E, 0x6A, 0x4C, 0x62, 0x30, 0xBB, 0x10 }}");

        private nint _hwusb = nint.Zero;
        private nint HwUSB
        {
            get
            {
                return System.Threading.Interlocked.CompareExchange(ref _hwusb, nint.Zero, nint.Zero);
            }
            set
            {
                System.Threading.Interlocked.Exchange(ref _hwusb, value);
                CX52Write.Get()?.SetWinUSB(value);
            }
        }
        CWinUSB.WINUSB_PIPE_INFORMATION pipe = new();
        private byte paused;

        public CWinUSBX52(Profile.CProfile refProfile) : base(HARDWARE_ID_X52, refProfile)
        {
        }

        protected override bool Prepare()
        {
            Close();

            nint diDevs;
            SetupDi.SP_DEVICE_INTERFACE_DATA diData = new();

            diDevs = SetupDi.SetupDiGetClassDevsW(ref guidInterface, null, nint.Zero, SetupDi.DIGCF_PRESENT | SetupDi.DIGCF_DEVICEINTERFACE);
            if (diDevs == Win32.INVALID_HANDLE_VALUE)
            {
                return false;
            }

            diData.cbSize = Marshal.SizeOf<SetupDi.SP_DEVICE_INTERFACE_DATA>();
            if (!SetupDi.SetupDiEnumDeviceInterfaces(diDevs, nint.Zero, ref guidInterface, 0, ref diData))
            {
                SetupDi.SetupDiDestroyDeviceInfoList(diDevs);
                return false;
            }

            if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, nint.Zero, 0, out uint size, nint.Zero) && (Marshal.GetLastWin32Error() != 122)) //122 = ERROR_INSUFFICIENT_BUFFER
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

            lock(mutex)
            {
                if (HardwareId == HARDWARE_ID_X52)
                {
                    pathInterface = ninterface;
                }
            }

            SetupDi.SetupDiDestroyDeviceInfoList(diDevs);
            return true;
        }

        protected override bool Open()
        {
            lock (mutex)
            {
                if ((HardwareId != 0) && !string.IsNullOrEmpty(pathInterface) && (HwUSB == nint.Zero))
                {
                    using Shared.RAII.UniqueHandle nhdev = new(Win32.CreateFileW(pathInterface, Win32.GENERIC_READ | Win32.GENERIC_WRITE, Win32.FILE_SHARE_READ | Win32.FILE_SHARE_WRITE, nint.Zero, Win32.OPEN_EXISTING, 0x80 | 0x40000000, nint.Zero)); // 0x80 | 0x40000000 = FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED
                    if (nhdev.IsInvalid)
                    {
                        return false;
                    }

                    if (!CWinUSB.WinUsb_Initialize(nhdev.DangerousGetHandle(), out nint wih))
                    {
                        if (wih != nint.Zero)
                        {
                            CWinUSB.WinUsb_Free(wih);
                        }
                        return false;
                    }

                    pipe = new();
                    if (!CWinUSB.WinUsb_QueryPipe(wih, 0, 0, ref pipe))
                    {
                        CWinUSB.WinUsb_Free(wih);
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
                        }
                    }
                    if (newReportLenght != 0)
                    {
                        reportBuffer = new byte[newReportLenght];
                        preparsed = newPrepased;
                        hdev = new(nhdev.Move());
                        HwUSB = wih;
                        CMFDMenu.Get()?.SetWelcome();
                    }
                    else
                    {
                        CWinUSB.WinUsb_Free(wih);
                        return false;
                    }
                }
            }

            return true;
        }

        protected override void Close()
        {
            lock(mutex)
            {
                nint old = HwUSB;
                HwUSB = nint.Zero;
                if (old != nint.Zero)
                {
                    Win32.CancelIoEx(old, nint.Zero);
                    CWinUSB.WinUsb_Free(old);
                }
            }
            base.Close();
        }

        public override ushort Read(Span<byte> buff)
        {
            if (System.Threading.Interlocked.CompareExchange(ref paused, 0, 0) == 1)
            {
                System.Threading.Tasks.Task.Delay(1500).Wait();
                return 0;
            }

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

            lock (mutex)
            {
                if (!hdev.IsInvalid)
                {
                    uint readSize = 0;
                    buff[0] = 0;
                    if (CWinUSB.WinUsb_ReadPipe(HwUSB, pipe.PipeId, buff.Slice(1), (uint)(reportBuffer.Length - 1), ref readSize, nint.Zero))
                    {
                        readSize++;
                        uint size = 0;
                        if (HID.HidP_GetData(0, nint.Zero, ref size, preparsed, reportBuffer, (uint)reportBuffer.Length) == HID.HIDP_STATUS_BUFFER_TOO_SMALL)
                        {
                            if (HID.HidP_GetData(0, buff, ref size, preparsed, reportBuffer, (uint)reportBuffer.Length) == HID.HIDP_STATUS_SUCCESS)
                            {
                                return (ushort)size;
                            }
                        }
                    }
                }
            }

            System.Threading.Tasks.Task.Delay(500).Wait();
            return 0;
        }

        public void SetPause(bool onoff)
        {
            if (onoff)
            {
                System.Threading.Interlocked.Exchange(ref paused, 1);
            }
            else
            {
                System.Threading.Interlocked.Exchange(ref paused, 0);
            }
        }
    }
}
