using Calibrator.APIs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Calibrator
{
    internal class DatosJoy
    {
        public class CUsage
        {
            public ushort Idx { get; set; }
            public byte Eje { get; set; }
            public byte Bits { get; set; }
            public ushort Rango { get; set; }
        }

        public uint Id { get; set; }
        public string Nombre { get; set; }

        public List<CUsage> Usages { get; } = new(); //está ordenada por idx

        public static DatosJoy GetInfo(string ninterface, uint joy)
        {
            if (ninterface == "WinUSBX52")
            {
                return UsbX52.GetInfo(joy);
            }
            IntPtr hDev = CWinUSB.CreateFileW(ninterface, 0x80000000 | 0x40000000, 1 | 2, IntPtr.Zero, 3, 0x00000080| 0x40000000, IntPtr.Zero);
            StringBuilder sb = new(256);
            HID.HidD_GetProductString(hDev, sb, 256);

            DatosJoy datos = new() { Id = joy, Nombre = sb.ToString() };

            IntPtr pdata = IntPtr.Zero;
            if (HID.HidD_GetPreparsedData(hDev, ref pdata))
            {
                if (HID.HidP_GetCaps(pdata, out HID.HIDP_CAPS caps) == 1114112)
                {
                    HID.HIDP_BUTTON_CAPS[] bcaps = new HID.HIDP_BUTTON_CAPS[caps.NumberInputButtonCaps];
                    HID.HIDP_VALUE_CAPS[] vcaps = new HID.HIDP_VALUE_CAPS[caps.NumberInputValueCaps];
                    {
                        ushort ustam = caps.NumberInputButtonCaps;
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_BUTTON_CAPS>() * ustam);
                        _ = HID.HidP_GetButtonCaps(0, ptr, ref ustam, pdata);
                        for (int i = 0; i < ustam; i++)
                        {
                            bcaps[i] = Marshal.PtrToStructure<HID.HIDP_BUTTON_CAPS>(ptr + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                        }
                        Marshal.FreeHGlobal(ptr);

                        ustam = caps.NumberInputValueCaps;
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * ustam);
                        _ = HID.HidP_GetValueCaps(0, ptr, ref ustam, pdata);
                        for (int i = 0; i < ustam; i++)
                        {
                            vcaps[i] = Marshal.PtrToStructure<HID.HIDP_VALUE_CAPS>(ptr + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                        }
                        Marshal.FreeHGlobal(ptr);
                    }

                    for (int idx = 0; idx < caps.NumberInputDataIndices; idx++)
                    {
                        foreach (HID.HIDP_BUTTON_CAPS bt in bcaps)
                        {
                            if (bt.Anonymous.Range.DataIndexMin == idx)
                            {
                                datos.Usages.Add(new()
                                {
                                    Eje = 255,
                                    Rango = 1,
                                    Bits = (byte)(bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1)
                                });
                                break;
                            }
                        }
                        foreach (HID.HIDP_VALUE_CAPS val in vcaps)
                        {
                            if (val.Anonymous.NotRange.DataIndex == idx)
                            {
                                datos.Usages.Add(new()
                                {
                                    Bits = (byte)val.BitSize,
                                    Eje = val.Anonymous.NotRange.Usage switch
                                    {
                                        48 => 0,
                                        49 => 1,
                                        50 => 2,
                                        51 => 3,
                                        52 => 4,
                                        53 => 5,
                                        54 => 6,
                                        55 => 7,
                                        57 => 254,
                                        _ => throw new NotImplementedException()
                                    },
                                    Rango = (ushort)val.LogicalMax
                                });
                                break;
                            }
                        }
                    }
                }
                HID.HidD_FreePreparsedData(pdata);
            }

            CWinUSB.CloseHandle(hDev);
            return datos;
        }

        public void ToHiddata(byte[] hiddata, ref uint[] hidreport, ref uint hidbotones, ref short[] povs)
        {
            List<ushort> pos = new();
            {
                ushort upos = 0;
                foreach (CUsage u in Usages)
                {
                    pos.Add(upos);
                    upos += u.Bits;
                }
            }
            byte povIdx = 0;
            for (int idx = 0; idx < Usages.Count; idx++)
            {
                CUsage u = Usages[idx];
                ushort ini = (ushort)((pos[idx] / 8) + 1);
                if (u.Eje == 255)
                {
                    hidbotones = hiddata[ini];
                    hidbotones |= (uint)(hiddata[ini + 1] << 8);
                    hidbotones |= (uint)(hiddata[ini + 2] << 16);
                    hidbotones |= (uint)(hiddata[ini + 3] << 24);
                }
                else if (u.Eje == 254)
                {
                    povs[povIdx] = (short)(hiddata[ini] | (hiddata[ini + 1] << 8));
                    povIdx++;
                }
                else
                {
                    ulong eje = hiddata[ini];
                    eje |= (ulong)hiddata[ini + 1] << 8;
                    eje |= (ulong)hiddata[ini + 2] << 16;
                    eje |= (ulong)hiddata[ini + 3] << 24;
                    eje |= (ulong)hiddata[ini + 4] << 32;
                    hidreport[u.Eje] = (uint)(eje >> (pos[idx] % 8));
                    hidreport[u.Eje] &= (uint)((1ul << u.Bits) - 1);
                }
            }
        }
    }
}
