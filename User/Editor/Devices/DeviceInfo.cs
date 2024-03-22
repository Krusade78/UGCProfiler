using API;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Profiler.Devices
{
    internal class DeviceInfo
    {
		public class HID_INPUT_DATA
		{
			public ushort[] Axis { get; set; } = new ushort[24];
            //ushort[] Hat = new ushort[4];
			public ulong[] Buttons { get; set; } = new ulong[2];
		};

		public class CUsage
        {
            public ushort ReportId { get; set; }
            public ushort ReportIdx { get; set; }
            public byte? Id { get; set; }
            public byte Type { get; set; }
            public byte Bits { get; set; }
            public ushort Range { get; set; }
        }

        public uint Id { get; set; }
        public string Name { get; set; }

        public byte NAxes { get; set; }
        public byte NHats { get; set; }
        public ushort NButtons { get; set; }
        public bool FromProfile { get; set; } = false;


        public List<CUsage> Usages { get; } = []; //sorted by idx

        public static DeviceInfo Get(string ninterface, uint joy)
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

			DeviceInfo devData = new() { Id = joy, Name = Marshal.PtrToStringAuto(buf).Trim() };
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
									Type = 255,
                                    Range = 1,
                                    Bits = (byte)(bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1)
                                });
                                devData.NButtons += (byte)(bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1);
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
                                        Type = 128,
                                        Range = 0
                                    });
                                }
                                else if (val.LogicalMin != 0) { throw new NotImplementedException(); }
                                else
                                {
                                    devData.Usages.Add(new()
                                    {
                                        ReportId = val.ReportID,
                                        ReportIdx = val.Anonymous.NotRange.DataIndex,
                                        Id = val.Anonymous.NotRange.Usage == 54 ? devData.NHats++ : devData.NAxes++,
                                        Bits = (byte)val.BitSize,
                                        Type = val.Anonymous.NotRange.Usage switch
                                        {
                                            48 => 0, //x
                                            49 => 1, //y
                                            50 => 2, //z
                                            51 => 3, //rx
                                            52 => 4, //ry
                                            53 => 5, //rz
                                            54 => 6, //slider
                                            55 => 6, //dial
                                            0x38 => 6, //Wheel
                                            57 => 254, //hat
                                            0x40 => 0, //vx
                                            0x41 => 1, //vy
                                            0x42 => 2, //vz
                                            0x43 => 3, //vbrx
                                            0x44 => 4, //vbry
                                            0x45 => 5, //vbrz
                                            0x46 => 6, //vno
                                            _ => throw new NotImplementedException()
                                        },
                                        Range = (ushort)val.LogicalMax
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
			uint idx = 0;
			byte idxAxis = 0;
			byte idxButton = 0;
            byte[] report = rawData.AsMemory(1, rawData.Length - 1).ToArray();

			foreach (CUsage mapIndex in Usages)
		    {
				if (mapIndex.ReportId != rawData[0]) { return false; }
				if (mapIndex.Bits > 0)
				{
					byte shift = (byte)(idx % 8);
					if (mapIndex.Type == 255)
					{
                        byte[] v = new byte[17];
                        unsafe
                        {
							fixed (byte* pv = &v[0])
                            {
                                fixed (byte* pr = &report[0])
                                {
                                    Buffer.MemoryCopy(pr + (idx / 8), pv, 17, ((idx + mapIndex.Bits - 1) / 8) + 1 - (idx / 8));
                                }
								v[(mapIndex.Bits + shift) / 8] &= (byte)((1 << ((mapIndex.Bits + shift) % 8)) - 1);
								
                                ulong* pv64 = (ulong*)pv;
                                if (shift != 0)
                                {
                                    *pv64 = (*pv64 >> shift) | *(ulong*)(pv + 8) << (64 - shift);
                                    pv64 = (ulong*)(pv + 8);
                                    *pv64 = (*pv64 >> shift) | (ulong)(v[16]) << (64 - shift);
                                    pv64 = (ulong*)pv;
                                }

                                byte shiftBt = (byte)(idxButton % 64);
                                if (idxButton > 63)
                                {
                                    hidData.Buttons[1] |= *pv64 << shiftBt;
                                }
                                else
                                {
                                    hidData.Buttons[0] |= *pv64 << shiftBt;
                                    if (shiftBt != 0) { hidData.Buttons[1] |= *pv64 >> (64 - shiftBt); }
                                    hidData.Buttons[1] |= *(ulong*)(pv + 8) << shiftBt;
                                }
                            }
						}

						idxButton += mapIndex.Bits;
					}
					else
					{
						uint v = 0;
                        unsafe
                        {
                            fixed (byte* pr = &report[0])
                            {
                                Buffer.MemoryCopy(pr + (idx / 8), (byte*)&v, 4, ((idx + mapIndex.Bits - 1) / 8) + 1 - (idx / 8));
                            }
						}
						v = (uint)((v >> shift) & ((1 << mapIndex.Bits) - 1));
						if (mapIndex.Type != 253)
						{
							hidData.Axis[idxAxis++] = (ushort)v;
						}
					}
					idx += mapIndex.Bits;
				}
			}

            return true;
		}
    }
}
