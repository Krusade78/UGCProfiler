using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using API;


namespace Launcher
{
    internal class CCalibration
    {
        public static bool Load(System.IO.Pipes.NamedPipeServerStream outputPipeSvc)
        {
            Shared.Calibration.HighLevel.Model dsc = new();
            if (System.IO.File.Exists("calibration.dat"))
            {
                try
                {
                    dsc = System.Text.Json.JsonSerializer.Deserialize<Shared.Calibration.HighLevel.Model> (System.IO.File.ReadAllText("calibration.dat")) ?? new();
                }
                catch { }
            }
            if (!LoadDefault(ref dsc))
            {
                return false;
            }

            List<Shared.Calibration.LowLevel.JoyJitters> jitters = [];
            List<Shared.Calibration.LowLevel.JoyLimits> limits = [];
            foreach (Shared.Calibration.HighLevel.Limits l in dsc.JoyLimits.DistinctBy(x => x.IdJoy))
            {
                int naxis = dsc.JoyLimits.Count(x => x.IdJoy == l.IdJoy);
                Shared.Calibration.LowLevel.JoyLimits jl = new() { JoyId = l.IdJoy, Limits = new Shared.CTypes.STLIMITS[naxis] };
                foreach (Shared.Calibration.HighLevel.Limits lAxis in dsc.JoyLimits.OrderBy(x => x.IdAxis).Where(x => x.IdJoy == l.IdJoy))
                {
                    jl.Limits[lAxis.IdAxis].Center = lAxis.Center;
                    jl.Limits[lAxis.IdAxis].Left = lAxis.Left;
                    jl.Limits[lAxis.IdAxis].Right = lAxis.Right;
                    jl.Limits[lAxis.IdAxis].Null = lAxis.Null;
                    jl.Limits[lAxis.IdAxis].Range = lAxis.Range;
                }
                limits.Add(jl);
            }
            foreach (Shared.Calibration.HighLevel.Jitter j in dsc.JoyJitters.DistinctBy(x => x.IdJoy))
            {
                int naxis = dsc.JoyJitters.Count(x => x.IdJoy == j.IdJoy);
                Shared.Calibration.LowLevel.JoyJitters jj = new() { JoyId = j.IdJoy, Jitters = new Shared.CTypes.STJITTER[naxis] };
                foreach (Shared.Calibration.HighLevel.Jitter jAxis in dsc.JoyJitters.OrderBy(x => x.IdAxis).Where(x => x.IdJoy == j.IdJoy))
                {
                    jj.Jitters[jAxis.IdAxis].Margin = jAxis.Margin;
                    jj.Jitters[jAxis.IdAxis].Strength = jAxis.Strength;
                    jj.Jitters[jAxis.IdAxis].Antiv = jAxis.Antiv;
                }
            }

            System.Text.StringBuilder buff = new();
            buff.Append((char)CService.MsgType.Calibration);
            buff.Append(System.Text.Json.JsonSerializer.Serialize(limits));
            outputPipeSvc.Write(System.Text.Encoding.UTF8.GetBytes(buff.ToString()));
            outputPipeSvc.Flush();
            buff = new();
            buff.Append((char)CService.MsgType.Antivibration);
            buff.Append(System.Text.Json.JsonSerializer.Serialize(jitters));
            outputPipeSvc.Write(System.Text.Encoding.UTF8.GetBytes(buff.ToString()));
            outputPipeSvc.Flush();

            return true;
        }

        private static bool LoadDefault(ref Shared.Calibration.HighLevel.Model cal)
        {
            SetupDi.SP_DEVICE_INTERFACE_DATA diData = new();
            Guid hidGuid = new();
            HID.HidD_GetHidGuid(ref hidGuid);
            nint diDevs = SetupDi.SetupDiGetClassDevsW(ref hidGuid, null, IntPtr.Zero, SetupDi.DIGCF_PRESENT | SetupDi.DIGCF_DEVICEINTERFACE);
            if (Win32.INVALID_HANDLE_VALUE == diDevs)
            {
                return false;
            }

            diData.cbSize = Marshal.SizeOf<SetupDi.SP_DEVICE_INTERFACE_DATA>();
            uint idx = 0;
            while (SetupDi.SetupDiEnumDeviceInterfaces(diDevs, nint.Zero, ref hidGuid, idx++, ref diData))
            {
                if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, nint.Zero, 0, out uint size, nint.Zero) && (122 != Marshal.GetLastWin32Error()))
                {
                    continue;
                }

                using Shared.RAII.HGlobalHandle buf = new((int)size);
                Marshal.WriteInt32(buf.DangerousGetHandle(), 8); //x64
                if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, buf.DangerousGetHandle(), size, out size, IntPtr.Zero))
                {
                    continue;
                }

                string ninterface = Marshal.PtrToStringAuto(buf.DangerousGetHandle() + 4) ?? "";
                uint joyId;
                {
                    int vIndex = ninterface.IndexOf("VID_", StringComparison.OrdinalIgnoreCase);
                    int pIndex = ninterface.IndexOf("PID_", StringComparison.OrdinalIgnoreCase);
                    if ((vIndex != -1) && (pIndex != -1))
                    {
                        joyId = uint.Parse(ninterface.Substring(vIndex + 4, 4), System.Globalization.NumberStyles.AllowHexSpecifier) << 16;
                        joyId |= uint.Parse(ninterface.Substring(pIndex + 4, 4), System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else
                    {
                        continue;
                    }
                }

                using Shared.RAII.UniqueHandle hDev = new(Win32.CreateFileW(ninterface, Win32.GENERIC_READ, Win32.FILE_SHARE_READ, IntPtr.Zero, Win32.OPEN_EXISTING, 0, IntPtr.Zero));
                if (hDev.IsInvalid)
                {
                    continue;
                }

                IntPtr pdata = IntPtr.Zero;
                if (!HID.HidD_GetPreparsedData(hDev.DangerousGetHandle(), ref pdata))
                {
                    continue;
                }

                using Shared.RAII.HGlobalHandle pcaps = new(Marshal.SizeOf<HID.HIDP_CAPS>());
                if (HID.HidP_GetCaps(pdata, pcaps.DangerousGetHandle()) == HID.HIDP_STATUS_SUCCESS)
                {
                    HID.HIDP_CAPS caps = Marshal.PtrToStructure<HID.HIDP_CAPS>(pcaps.DangerousGetHandle());
                    if ((caps.UsagePage == 1) && ((caps.Usage == 4) || (caps.Usage == 5)))
                    {
                        if (!LoadDefaultDeviceCalibration(ref cal, joyId, pdata, ref caps))
                        {
                            HID.HidD_FreePreparsedData(pdata);
                            continue;
                        }
                    }
                }

                HID.HidD_FreePreparsedData(pdata);
            }

            SetupDi.SetupDiDestroyDeviceInfoList(diDevs);

            return true;
        }

        private static bool LoadDefaultDeviceCalibration(ref Shared.Calibration.HighLevel.Model cal, uint joyId, IntPtr pdata, ref HID.HIDP_CAPS caps)
        {
            HID.HIDP_VALUE_CAPS[] vcaps = new HID.HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

            #region GetCaps
            if (caps.NumberInputValueCaps != 0)
            {
                ushort ustam = caps.NumberInputValueCaps;
                using Shared.RAII.HGlobalHandle ptr = new(Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * ustam);
                if (HID.HidP_GetValueCaps(0, ptr.DangerousGetHandle(), ref ustam, pdata) != HID.HIDP_STATUS_SUCCESS)
                {
                    return false;
                }
                for (int i = 0; i < ustam; i++)
                {
                    vcaps[i] = Marshal.PtrToStructure<HID.HIDP_VALUE_CAPS>(ptr.DangerousGetHandle() + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                }
            }
            #endregion

            byte axId = 0;
            foreach (HID.HIDP_VALUE_CAPS val in vcaps)
            {
                bool destinationAxis = false;
                switch (val.Anonymous.NotRange.Usage)
                {
                    case 48: //x
                    case 49: //y
                    case 50: //z
                    case 51: //rx
                    case 52: //ry
                    case 53: //rz
                    case 54: //slider
                    case 55: //dial
                    case 0x38: //Wheel
                               //57 => throw new NotImplementedException(), //254, //hat
                    case 0x40: //vx
                    case 0x41: //vy
                    case 0x42: //vz
                    case 0x43: //vbrx
                    case 0x44: //vbry
                    case 0x45:  //vbrz
                    case 0x46: //vno
                        destinationAxis = true;
                        break;
                    default:
                        break;

                };
                if (!destinationAxis)
                {
                    continue;
                }
                if (!cal.JoyLimits.Any(x => (x.IdJoy == joyId) && (x.IdAxis == axId)))
                {
                    cal.JoyLimits.Add(new()
                    {
                        IdAxis = axId,
                        IdJoy = joyId,
                        Null = 1,
                        Left = (ushort)val.LogicalMin,
                        Right = (ushort)val.LogicalMax,
                        Center = (ushort)(val.LogicalMax / 2)
                    });
                }
                if (!cal.JoyJitters.Any(x => (x.IdJoy == joyId) && (x.IdAxis == axId)))
                { 
                    cal.JoyJitters.Add(new()
                    {
                        IdAxis = axId,
                        IdJoy = joyId,
                        Antiv = 0,
                    });
                }
                axId++;
            }

            return true;

        }
    }
}
