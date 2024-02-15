using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using API;


namespace Launcher
{
	internal class CCalibration
	{
		public bool Load(System.IO.BinaryWriter outputPipeSvc)
		{
			Shared.Calibration.CCalibration dsc = new();
			try
			{
				dsc = System.Text.Json.JsonSerializer.Deserialize<Shared.Calibration.CCalibration>(System.IO.File.ReadAllText("calibration.dat"));
			}
			catch
			{
				if (!LoadDefault(ref dsc))
				{
					return false;
				}
			}

			Dictionary<uint, List<Shared.CTypes.STJITTER>> jitter = [];
			Dictionary<uint, List<Shared.CTypes.STLIMITS>> limits = [];
			int axesJitter = 5;
			int axesLimits = 5;
			foreach (Shared.Calibration.Limits l in dsc.Limits.DistinctBy(x => x.IdJoy))
			{
				limits.Add(l.IdJoy, []);
				axesLimits += 5;
				foreach (Shared.Calibration.Limits lAxis in dsc.Limits.OrderBy(x => x.IdAxis).Where(x => x.IdJoy == l.IdJoy))
				{
					limits[l.IdJoy].Add(new Shared.CTypes.STLIMITS() { Center = lAxis.Center, Left = lAxis.Left, Right = lAxis.Right, Null = lAxis.Null, Cal = lAxis.Cal, Range = lAxis.Range });
					axesLimits += Marshal.SizeOf(typeof(Shared.CTypes.STLIMITS));
				}
			}
			foreach (Shared.Calibration.Jitter j in dsc.Jitters.DistinctBy(x => x.IdJoy))
			{
				jitter.Add(j.IdJoy, []);
				axesJitter += 5;
				foreach (Shared.Calibration.Jitter jAxis in dsc.Jitters.OrderBy(x => x.IdAxis).Where(x => x.IdJoy == j.IdJoy))
				{
					jitter[j.IdJoy].Add(new Shared.CTypes.STJITTER() { Margin = jAxis.Margin, Strength = jAxis.Strength, Antiv = jAxis.Antiv });
					axesJitter += Marshal.SizeOf(typeof(Shared.CTypes.STJITTER));
				}
			}

			if (outputPipeSvc != null)
			{
				byte[] bufCal = new byte[axesLimits];
				byte[] bufJit = new byte[axesJitter];
				bufCal[0] = (byte)CService.MsgType.Calibration;
				bufJit[0] = (byte)CService.MsgType.Antivibration;
				int posPtr = 1;
				bufCal[posPtr++] = (byte)(limits.Count & 0xff);
				bufCal[posPtr++] = (byte)((limits.Count >> 8) & 0xff);
				bufCal[posPtr++] = (byte)((limits.Count >> 16) & 0xff);
				bufCal[posPtr++] = (byte)((limits.Count >> 24) & 0xff);
				foreach (KeyValuePair<uint, List<Shared.CTypes.STLIMITS>> ll in limits)
				{
					bufCal[posPtr++] = (byte)(ll.Key & 0xff);
					bufCal[posPtr++] = (byte)((ll.Key >> 8) & 0xff);
					bufCal[posPtr++] = (byte)((ll.Key >> 16) & 0xff);
					bufCal[posPtr++] = (byte)((ll.Key >> 24) & 0xff);
					bufCal[posPtr++] = (byte)ll.Value.Count;
					foreach (Shared.CTypes.STLIMITS l in ll.Value)
					{
						IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Shared.CTypes.STLIMITS)));
						Marshal.StructureToPtr(l, ptr, true);
						Marshal.Copy(ptr, bufCal, posPtr, Marshal.SizeOf(typeof(Shared.CTypes.STLIMITS)));
						Marshal.FreeHGlobal(ptr);
						posPtr += Marshal.SizeOf(typeof(Shared.CTypes.STLIMITS));
					}
				}

				posPtr = 1;
				bufJit[posPtr++] = (byte)(jitter.Count & 0xff);
				bufJit[posPtr++] = (byte)((jitter.Count >> 8) & 0xff);
				bufJit[posPtr++] = (byte)((jitter.Count >> 16) & 0xff);
				bufJit[posPtr++] = (byte)((jitter.Count >> 24) & 0xff);
				foreach (KeyValuePair<uint, List<Shared.CTypes.STJITTER>> lj in jitter)
				{
					bufJit[posPtr++] = (byte)(lj.Key & 0xff);
					bufJit[posPtr++] = (byte)((lj.Key >> 8) & 0xff);
					bufJit[posPtr++] = (byte)((lj.Key >> 16) & 0xff);
					bufJit[posPtr++] = (byte)((lj.Key >> 24) & 0xff);
					bufJit[posPtr++] = (byte)lj.Value.Count;
					foreach (Shared.CTypes.STJITTER j in lj.Value)
					{
						IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Shared.CTypes.STJITTER)));
						Marshal.StructureToPtr(j, ptr, true);
						Marshal.Copy(ptr, bufJit, posPtr, Marshal.SizeOf(typeof(Shared.CTypes.STJITTER)));
						Marshal.FreeHGlobal(ptr);
						posPtr += Marshal.SizeOf(typeof(Shared.CTypes.STJITTER));
					}
				}
				outputPipeSvc.Write(bufCal, 0, bufCal.Length);
				outputPipeSvc.Flush();
				outputPipeSvc.Write(bufJit, 0, bufJit.Length);
				outputPipeSvc.Flush();

				return true;
			}

			return false;
		}

		private static bool LoadDefault(ref Shared.Calibration.CCalibration cal)
		{
			CWinUSB.SP_DEVICE_INTERFACE_DATA diData = new();
			Guid hidGuid = new();
			HID.HidD_GetHidGuid(ref hidGuid);
			IntPtr diDevs = CWinUSB.SetupDiGetClassDevsW(ref hidGuid, null, IntPtr.Zero, 0x2 | 0x10);
			if (new IntPtr(-1) == diDevs)
			{
				return false;
			}

			diData.cbSize = Marshal.SizeOf<CWinUSB.SP_DEVICE_INTERFACE_DATA>();
			uint idx = 0;
			while (CWinUSB.SetupDiEnumDeviceInterfaces(diDevs, IntPtr.Zero, ref hidGuid, idx++, ref diData))
			{
				uint tam = 0;
				if ((false == CWinUSB.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, IntPtr.Zero, 0, ref tam, IntPtr.Zero)) && (122 != CWinUSB.GetLastError()))
				{
					continue;
				}

				IntPtr buf = Marshal.AllocHGlobal((int)tam);
				Marshal.WriteInt32(buf, 8);
				if (!CWinUSB.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, buf, tam, ref tam, IntPtr.Zero))
				{
					Marshal.FreeHGlobal(buf);
					continue;
				}

				string ninterface = Marshal.PtrToStringAuto(buf + 4);
				Marshal.FreeHGlobal(buf);
				if (!ninterface.Contains("vid", StringComparison.InvariantCultureIgnoreCase) || !ninterface.Contains("pid", StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}
				uint joyId;
				try
				{
					joyId = uint.Parse(ninterface[12..16], System.Globalization.NumberStyles.AllowHexSpecifier) << 16;
					joyId |= uint.Parse(ninterface[21..25], System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				catch
				{
					continue;
				}

				IntPtr hDev = CWinUSB.CreateFileW(ninterface, 0x80000000 | 0x40000000, 1 | 2, IntPtr.Zero, 3, 0x00000080 | 0x40000000, IntPtr.Zero);
				if (hDev == CWinUSB.INVALID_HANDLE_VALUE)
				{
					continue;
				}

				IntPtr pdata = IntPtr.Zero;
				if (!HID.HidD_GetPreparsedData(hDev, ref pdata))
				{
					CWinUSB.CloseHandle(hDev);
					continue;
				}

				IntPtr pcaps = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_CAPS>());
				if (HID.HidP_GetCaps(pdata, pcaps) == 0x110000)
				{
					HID.HIDP_CAPS caps = Marshal.PtrToStructure<HID.HIDP_CAPS>(pcaps);
					Marshal.FreeHGlobal(pcaps);
					if ((caps.UsagePage == 1) && ((caps.Usage == 4) || (caps.Usage == 5)))
					{
						if (!LoadDefaultDeviceCalibration(ref cal, joyId, pdata, ref caps))
						{
							HID.HidD_FreePreparsedData(pdata);
							CWinUSB.CloseHandle(hDev);
							continue;
						}
					}
				}
				else
				{
					Marshal.FreeHGlobal(pcaps);
				}

				HID.HidD_FreePreparsedData(pdata);
				CWinUSB.CloseHandle(hDev);
			}

			CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);

			return true;
		}

		private static bool LoadDefaultDeviceCalibration(ref Shared.Calibration.CCalibration cal, uint joyId, IntPtr pdata, ref HID.HIDP_CAPS caps)
		{
			HID.HIDP_VALUE_CAPS[] vcaps = new HID.HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

			#region GetCaps
			if (caps.NumberInputValueCaps != 0)
			{
				ushort ustam = caps.NumberInputValueCaps;
				IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * ustam);
				if (HID.HidP_GetValueCaps(0, ptr, ref ustam, pdata) != 0x110000)
				{
					Marshal.FreeHGlobal(ptr);
					return false;
				}
				for (int i = 0; i < ustam; i++)
				{
					vcaps[i] = Marshal.PtrToStructure<HID.HIDP_VALUE_CAPS>(ptr + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
				}
				Marshal.FreeHGlobal(ptr);
			}
			#endregion

			byte axId = 0;
			foreach (HID.HIDP_VALUE_CAPS val in vcaps)
			{
				if (!cal.Limits.Any(x => (x.IdJoy == joyId) && (x.IdAxis == axId)))
				{
					bool destinationAxis = false;
					switch(val.Anonymous.NotRange.Usage)
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
							destinationAxis = true;
							break;
						//0x46 => 6, //vno
						default:
							break;

					};
					if (!destinationAxis)
					{
						continue;
					}
					cal.Limits.Add(new()
					{
						IdAxis = axId,
						IdJoy = joyId,
						Cal = 1,
						Null = 1,
						Left = (ushort)val.LogicalMin,
						Right = (ushort)val.LogicalMax,
						Center = (ushort)(val.LogicalMax / 2)						
					});
					cal.Jitters.Add(new()
					{
						IdAxis = axId,
						IdJoy = joyId,
						Antiv = 0,
					});
					axId++;
				}
			}

			return true;

		}
	}
}
