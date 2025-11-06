using API;
using System;
using System.Runtime.InteropServices;

namespace Profiler.Devices
{
    public class DeviceInfo : Shared.ProfileModel.DeviceInfo
    {
        public class HID_INPUT_DATA
        {
            public ushort[] Axis { get; set; } = new ushort[24];
            public byte[] Hats { get; set; } = new byte[4];
            public ulong[] Buttons { get; set; } = new ulong[2];
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct HIDP_DATA
        {
            [FieldOffset(0)]
            public ushort DataIndex;
            [FieldOffset(2)]
            public ushort Reserved;
            [FieldOffset(4)]
            public uint RawValue;
            [FieldOffset(4)]
            public byte On;
        }

        public bool FromProfile { get; set; } = false;

        public static DeviceInfo? Get(string ninterface, uint joy)
        {
            if (ninterface.StartsWith("_WUSBX52"))
            {
                return UsbX52.GetInfo(joy);
            }

            IntPtr hDev = CWinUSB.CreateFileW(ninterface, 0x80000000 | 0x40000000, 1 | 2, IntPtr.Zero, 3, 0x00000080| 0x40000000, IntPtr.Zero);
            if (hDev == CWinUSB.INVALID_HANDLE_VALUE)
            {
                goto error;
            }

            IntPtr buf = Marshal.AllocHGlobal(256);
            if (!HID.HidD_GetProductString(hDev, buf, 256))
            {
                Marshal.FreeHGlobal(buf);
                goto error;
            }

            DeviceInfo devData = new() { Id = joy, Name = Marshal.PtrToStringAuto(buf)?.Trim() };
            Marshal.FreeHGlobal(buf);

            IntPtr pdata = IntPtr.Zero;
            if (!HID.HidD_GetPreparsedData(hDev, ref pdata))
            {
                goto error;
            }
            else
            {
                IntPtr pcaps = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_CAPS>());
                if (HID.HidP_GetCaps(pdata, pcaps) == 0x110000)
                {
                    HID.HIDP_CAPS caps = Marshal.PtrToStructure<HID.HIDP_CAPS>(pcaps);
                    Marshal.FreeHGlobal(pcaps);
                    HID.HIDP_BUTTON_CAPS[] bcaps = new HID.HIDP_BUTTON_CAPS[caps.NumberInputButtonCaps];
                    HID.HIDP_VALUE_CAPS[] vcaps = new HID.HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

                    #region GetCaps
                    if (caps.NumberInputButtonCaps != 0)
                    {
                        ushort ustam = caps.NumberInputButtonCaps;
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_BUTTON_CAPS>() * ustam);
                        if (HID.HidP_GetButtonCaps(0, ptr, ref ustam, pdata) != 0x110000)
                        {
                            goto errorPD;
                        }
                        for (int i = 0; i < ustam; i++)
                        {
                            bcaps[i] = Marshal.PtrToStructure<HID.HIDP_BUTTON_CAPS>(ptr + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                        }
                        Marshal.FreeHGlobal(ptr);
                    }
                    if (caps.NumberInputValueCaps != 0)
                    { 
                        ushort ustam = caps.NumberInputValueCaps;
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * ustam);
                        if (HID.HidP_GetValueCaps(0, ptr, ref ustam, pdata) != 0x110000)
                        {
                            goto errorPD;
                        }
                        for (int i = 0; i < ustam; i++)
                        {
                            vcaps[i] = Marshal.PtrToStructure<HID.HIDP_VALUE_CAPS>(ptr + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                        }
                        Marshal.FreeHGlobal(ptr);
                    }
                    #endregion

                    for (int idx = 0; idx < caps.NumberInputDataIndices; idx++)
                    {
                        foreach (HID.HIDP_BUTTON_CAPS bt in bcaps)
                        {
                            if (bt.Anonymous.Range.DataIndexMin == idx)
                            {
                                devData.Usages.Add(new()
                                {
                                    ReportId = bt.ReportID,
                                    ReportIdx = bt.Anonymous.Range.DataIndexMin,
                                    Id = (byte)(devData.NButtons == 0 ? 0 : devData.NButtons - 1),
                                    Type = (byte)CEnums.ElementType.Button,
                                    Range = bt.Anonymous.Range.DataIndexMax,
                                    Bits = (byte)(bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1)
                                });
                                devData.NButtons += (byte)(bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1);
                                idx = bt.Anonymous.Range.DataIndexMax;

                                break;
                            }
                        }
                        foreach (HID.HIDP_VALUE_CAPS val in vcaps)
                        {
                            if (val.Anonymous.NotRange.DataIndex == idx)
                            {
                                if (val.Anonymous.NotRange.Usage == 0)
                                {
                                    devData.Usages.Add(new()
                                    {
                                        ReportId = val.ReportID,
                                        ReportIdx = val.Anonymous.NotRange.DataIndex,
                                        Bits = (byte)val.BitSize,
                                        Type = (byte)CEnums.ElementType.None,
                                        Range = 0
                                    });
                                }
                                else if ((val.Anonymous.NotRange.Usage != 57) && (val.LogicalMin != 0)) { throw new NotImplementedException(); }
                                else if (val.Anonymous.NotRange.Usage == 1) { }//ignorar
                                else
                                {
                                    devData.Usages.Add(new()
                                    {
                                        ReportId = val.ReportID,
                                        ReportIdx = val.Anonymous.NotRange.DataIndex,
                                        Id = val.Anonymous.NotRange.Usage == 57 ? devData.NHats++ : devData.NAxes++,
                                        Bits = (byte)val.BitSize,
                                        Type = val.Anonymous.NotRange.Usage switch
                                        {
                                            36 => (byte)CEnums.ElementType.AxisX, //usage page 5 (game controls)
                                            38 => (byte)CEnums.ElementType.AxisY, //usage page 5 (game controls)
                                            48 => (byte)CEnums.ElementType.AxisX,
                                            49 => (byte)CEnums.ElementType.AxisY,
                                            50 => (byte)CEnums.ElementType.AxisZ,
                                            51 => (byte)CEnums.ElementType.AxisRx,
                                            52 => (byte)CEnums.ElementType.AxisRy,
                                            53 => (byte)CEnums.ElementType.AxisRz,
                                            54 => (byte)CEnums.ElementType.AxisSl,
                                            55 => (byte)CEnums.ElementType.AxisSl,
                                            0x38 => (byte)CEnums.ElementType.AxisSl,
                                            57 => (byte)CEnums.ElementType.Hat,
                                            0x40 => (byte)CEnums.ElementType.AxisX,
                                            0x41 => (byte)CEnums.ElementType.AxisY,
                                            0x42 => (byte)CEnums.ElementType.AxisZ,
                                            0x43 => (byte)CEnums.ElementType.AxisRx,
                                            0x44 => (byte)CEnums.ElementType.AxisRy,
                                            0x45 => (byte)CEnums.ElementType.AxisRz,
                                            0x46 => (byte)CEnums.ElementType.AxisSl,
                                            _ => throw new NotImplementedException()
                                        },
                                        Range = (ushort)(val.Anonymous.NotRange.Usage != 57 ? val.LogicalMax : ((byte)val.LogicalMin & 0xf) | (((byte)val.LogicalMax & 0xf) << 4)),
                                    });
                                }
                                break;
                            }
                        }
                    }

                    HID.HidD_FreePreparsedData(pdata);
                }
                else
                {
                    Marshal.FreeHGlobal(pcaps);
                    goto errorPD;
                }
            }

            CWinUSB.CloseHandle(hDev);
            return devData;

        errorPD:
            HID.HidD_FreePreparsedData(pdata);
        error:
            CWinUSB.CloseHandle(hDev);
            return null;
        }

        public bool ToHiddata(byte[] rawData, ref HID_INPUT_DATA hidData)
        {
            const byte SIZEOF_HIDP_DATA = 8;

            GCHandle ptr  = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            for (int i = 0; i < rawData.Length; i += SIZEOF_HIDP_DATA)
            {
                HIDP_DATA indexData = Marshal.PtrToStructure<HIDP_DATA>(ptr.AddrOfPinnedObject() + i);
                foreach (CUsage mapIndex in Usages)
                {
                    if ((mapIndex.Type != 255) && (mapIndex.Type != 254) && (mapIndex.ReportIdx == indexData.DataIndex))
                    {
                        ushort v = (ushort)indexData.RawValue;
                        if (mapIndex.Type == 253)
                        {
                            hidData.Hats[mapIndex.Id] = (byte)v;
                        }
                        else
                        {
                            hidData.Axis[mapIndex.Id] = v;
                        }
                        break;
                    }
                    else if ((mapIndex.Type == 254) && (indexData.DataIndex >= mapIndex.ReportIdx) && (indexData.DataIndex <= mapIndex.Range))
                    {
                        byte btIdx = (byte)(indexData.DataIndex - mapIndex.ReportIdx);
                        if ((btIdx + mapIndex.Id) > 63)
                        {
                            hidData.Buttons[1] |= 1ul << (btIdx + mapIndex.Id);
                        }
                        else
                        {
                            hidData.Buttons[0] |= 1ul << (btIdx + mapIndex.Id);
                        }
                        break;
                    }
                }
            }
            ptr.Free();

            return true;
        }
    }
}
