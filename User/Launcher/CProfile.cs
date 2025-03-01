﻿using System;
using System.Windows;
using System.Collections.Generic;
using API;
using System.Runtime.InteropServices;

namespace Launcher
{
	internal class CProfile(object main)
	{
		private readonly CMain main = (CMain)main;

		public bool Load(string file, System.IO.BinaryWriter pipe)
		{
			Shared.ProfileModel profile;
			if (string.IsNullOrEmpty(file))
			{
				profile = new();
				if (LoadDefault(ref profile))
				{
					file = CTranslate.Get("default");
				}
				else
				{
					return false;
				}
			}
			else
			{
				try
				{
					string json = System.IO.File.ReadAllText(file);
					profile = System.Text.Json.JsonSerializer.Deserialize<Shared.ProfileModel>(json);
				}
				catch (Exception ex)
				{
					main.MessageBox(ex.Message, CTranslate.Get("error"), MessageBoxImage.Warning);
					return false;
				}
				file = System.IO.Path.GetFileNameWithoutExtension(file);
			}

			#region "Macros"
			{
				uint tamBuffer = 0;
				uint nActions = 0;
				foreach (Shared.ProfileModel.MacroModel mm in profile.Macros)
				{
					if (mm.Id == 0) //skip No action macro
					{
						continue;
					}
					nActions++;
					tamBuffer += 1 + (uint)(4 * mm.Commands.Count); //1 list size
				}
				byte[] bufferCommands = new byte[tamBuffer + 1];
				bufferCommands[0] = (byte)CService.MsgType.Macros;

				int pos = 1;
				foreach (Shared.ProfileModel.MacroModel mm in profile.Macros)
				{
					if (mm.Id == 0) //skip No action macro
					{
						continue;
					}
					bufferCommands[pos++] = (byte)mm.Commands.Count;
					for (byte i = 0; i < (byte)mm.Commands.Count; i++)
					{
						bufferCommands[pos++] = (byte)(mm.Commands[i] & 0xff);
						bufferCommands[pos++] = (byte)((mm.Commands[i] >> 8) & 0xff);
						bufferCommands[pos++] = (byte)((mm.Commands[i] >> 16) & 0xff);
                        bufferCommands[pos++] = (byte)((mm.Commands[i] >> 24) & 0xff);
                    }
				}

				if (pipe != null)
				{
					pipe.Write(bufferCommands, 0, bufferCommands.Length);
					pipe.Flush();
				}
			}
			#endregion

			#region "Mapping"
			{
				List<byte> mapBuffer = [];

				mapBuffer.Add((byte)CService.MsgType.Map);

				#region "X52 MFD Text"
				{
					byte[] txtBuffer = new byte[17];
					string pfName = file;
					if (pfName.Length > 16)
						pfName = pfName[..16];
					else if (pfName.Length == 0)
						pfName = "";
					pfName = pfName.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
					byte[] text = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(pfName));
					for (byte i = 0; i < 16; i++)
					{
						if (text.Length >= (i + 1))
							txtBuffer[i + 1] = text[i];
						else
							txtBuffer[i + 1] = 0;
					}

					txtBuffer[0] = 1;

					mapBuffer.AddRange(txtBuffer);
				}
				#endregion

				//MouseTick
				mapBuffer.Add(profile.MouseTick);

				//buttons
				mapBuffer.Add((byte)profile.ButtonsMap.Count);
				foreach (KeyValuePair<uint, Shared.ProfileModel.ButtonMapModel> bm in profile.ButtonsMap)
				{
					mapBuffer.Add((byte)(bm.Key & 0xff));
					mapBuffer.Add((byte)((bm.Key >> 8) & 0xff));
					mapBuffer.Add((byte)((bm.Key >> 16) & 0xff));
					mapBuffer.Add((byte)((bm.Key >> 24) & 0xff));
					mapBuffer.Add((byte)bm.Value.Modes.Count);
					foreach (KeyValuePair<byte, Shared.ProfileModel.ButtonMapModel.ModeModel> bmm in bm.Value.Modes)
					{
						mapBuffer.Add(bmm.Key);
						mapBuffer.Add((byte)bmm.Value.Buttons.Count);
						foreach (KeyValuePair<byte, Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel> button in bmm.Value.Buttons)
						{
							mapBuffer.Add(button.Key);
							mapBuffer.Add(button.Value.Type);
							mapBuffer.Add((byte)button.Value.Actions.Count);
							foreach (ushort index in button.Value.Actions)
							{
								mapBuffer.Add((byte)(index & 0xff));
								mapBuffer.Add((byte)(index >> 8));
							}
						}
					}
				}

				//hats
                mapBuffer.Add((byte)profile.HatsMap.Count);
                foreach (KeyValuePair<uint, Shared.ProfileModel.ButtonMapModel> bm in profile.HatsMap)
                {
                    mapBuffer.Add((byte)(bm.Key & 0xff));
                    mapBuffer.Add((byte)((bm.Key >> 8) & 0xff));
                    mapBuffer.Add((byte)((bm.Key >> 16) & 0xff));
                    mapBuffer.Add((byte)((bm.Key >> 24) & 0xff));
                    mapBuffer.Add((byte)bm.Value.Modes.Count);
                    foreach (KeyValuePair<byte, Shared.ProfileModel.ButtonMapModel.ModeModel> bmm in bm.Value.Modes)
                    {
                        mapBuffer.Add(bmm.Key);
                        mapBuffer.Add((byte)bmm.Value.Buttons.Count);
                        foreach (KeyValuePair<byte, Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel> button in bmm.Value.Buttons)
                        {
                            mapBuffer.Add(button.Key);
                            mapBuffer.Add(button.Value.Type);
                            mapBuffer.Add((byte)button.Value.Actions.Count);
                            foreach (ushort index in button.Value.Actions)
                            {
                                mapBuffer.Add((byte)(index & 0xff));
                                mapBuffer.Add((byte)(index >> 8));
                            }
                        }
                    }
                }

                //axes
                mapBuffer.Add((byte)profile.AxesMap.Count);
				foreach (KeyValuePair<uint, Shared.ProfileModel.AxisMapModel> am in profile.AxesMap)
				{
					mapBuffer.Add((byte)(am.Key & 0xff));
					mapBuffer.Add((byte)((am.Key >> 8) & 0xff));
					mapBuffer.Add((byte)((am.Key >> 16) & 0xff));
					mapBuffer.Add((byte)((am.Key >> 24) & 0xff));
					mapBuffer.Add((byte)am.Value.Modes.Count);
					foreach (KeyValuePair<byte, Shared.ProfileModel.AxisMapModel.ModeModel> amm in am.Value.Modes)
					{
						mapBuffer.Add(amm.Key);
						mapBuffer.Add((byte)amm.Value.Axes.Count);
						foreach (KeyValuePair<byte, Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel> axis in amm.Value.Axes)
						{
							mapBuffer.Add(axis.Key);
							mapBuffer.Add(axis.Value.Mouse);
							mapBuffer.Add(axis.Value.IdJoyOutput);
							mapBuffer.Add(axis.Value.Type);
							mapBuffer.Add(axis.Value.OutputAxis);
							foreach (byte sens in axis.Value.Sensibility)
							{
								mapBuffer.Add(sens);
							}
							mapBuffer.Add((byte)(axis.Value.IsSensibilityForSlider ? 1 : 0));
							mapBuffer.Add((byte)axis.Value.Zones.Count);
							foreach (byte band in axis.Value.Zones)
							{
								mapBuffer.Add(band);
							}
							mapBuffer.Add((byte)axis.Value.Actions.Count);
							foreach (ushort actionId in axis.Value.Actions)
							{
								ushort index = actionId;
								mapBuffer.Add((byte)(index & 0xff));
								mapBuffer.Add((byte)(index >> 8));
							}
							mapBuffer.Add(axis.Value.Resistance.Item1);
							mapBuffer.Add(axis.Value.Resistance.Item2);
						}
					}
				}

				if (pipe != null)
				{
					pipe.Write(mapBuffer.ToArray(), 0, mapBuffer.Count);
					pipe.Flush();
				}
			}
			#endregion

			return true;
		}

		private bool LoadDefault(ref Shared.ProfileModel profile)
		{
			#region Button Macros
			{
				ushort id = 1; //bypass no action macro (id == 0);

				for (byte j = 0; j < 3; j++)
				{
					for (byte bt = 0; bt < 128; bt++)
					{
						profile.Macros.Add(new()
						{
							Id = id++,
							Name = $"<Btn. J{j} - {bt}>",
							Commands = [
								(byte)Shared.CTypes.CommandType.DxButton + (uint)(((j << 20) | bt) << 8),
								(byte)Shared.CTypes.CommandType.Hold,
								(byte)(Shared.CTypes.CommandType.Release | Shared.CTypes.CommandType.DxButton) + (uint)(((j << 20) | bt) << 8)]
						});
					}
				}
			}
			#endregion

			#region Hat Macros
			{
				ushort id = 385;

				for (byte j = 0; j < 3; j++)
				{
					for (byte h = 0; h < 4; h++)
					{
						for (byte pos = 0; pos < 8; pos++)
						{
							profile.Macros.Add(new()
							{
								Id = id++,
								Name = $">Hat J{j} H{h} P{pos}>",
								Commands = [
									(byte)Shared.CTypes.CommandType.DxHat + (uint)(((h) << 16) + (pos << 8) + (j << 28)),
									(byte)Shared.CTypes.CommandType.Hold,
									(byte)(Shared.CTypes.CommandType.Release | Shared.CTypes.CommandType.DxHat) + (uint)((h << 16) + (pos << 8) + (j << 28))]
							});
						}
					}
				}
			}
			#endregion

			SortedList<string, uint> selected = [];

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
			byte devicesDetected = 0;
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
						if (!LoadDefaultDeviceProfile(ref profile, joyId, devicesDetected, pdata, ref caps))
						{
							HID.HidD_FreePreparsedData(pdata);
							CWinUSB.CloseHandle(hDev);
							continue;
						}

						buf = Marshal.AllocHGlobal(256);
						HID.HidD_GetProductString(hDev, buf, 0);
						if (HID.HidD_GetProductString(hDev, buf, 256))
						{
							selected.Add(Marshal.PtrToStringAuto(buf).Trim(), joyId);
						}
						else
						{
							selected.Add($"{CTranslate.Get("unkown")} {devicesDetected}", joyId);
						}
						Marshal.FreeHGlobal(buf);
						devicesDetected++;
					}
				}
				else
				{
					Marshal.FreeHGlobal(pcaps);
				}

				HID.HidD_FreePreparsedData(pdata);
				CWinUSB.CloseHandle(hDev);
				if (devicesDetected == 3)
				{
					break;
				}
			}

			CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);



			if (devicesDetected > 0)
			{
				string msj = $"{CTranslate.Get("detected")}\n";
				byte idOutput = 0;
				foreach (KeyValuePair<string, uint> dev in selected)
				{
					msj += dev.Key + "\n";
					idOutput++;
				}
				main.MessageBox(msj, "", MessageBoxImage.Information);
			}

			return true;
		}

		private static bool LoadDefaultDeviceProfile(ref Shared.ProfileModel profile, uint joyId, byte outJoy, IntPtr pdata, ref HID.HIDP_CAPS caps)
		{
			HID.HIDP_BUTTON_CAPS[] bcaps = new HID.HIDP_BUTTON_CAPS[caps.NumberInputButtonCaps];
			HID.HIDP_VALUE_CAPS[] vcaps = new HID.HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

			#region GetCaps
			if (caps.NumberInputButtonCaps != 0)
			{
				ushort ustam = caps.NumberInputButtonCaps;
				IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<HID.HIDP_BUTTON_CAPS>() * ustam);
				if (HID.HidP_GetButtonCaps(0, ptr, ref ustam, pdata) != 0x110000)
				{
					Marshal.FreeHGlobal(ptr);
					return false;
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

			byte btId = 0;
			byte axId = 0;
			byte hatId = 0;
			List<byte> axisUsed = [];
			foreach (KeyValuePair<uint, Shared.ProfileModel.AxisMapModel> kvp in profile.AxesMap)
			{
				if (kvp.Value.Modes.TryGetValue(0, out Shared.ProfileModel.AxisMapModel.ModeModel mode))
				{
					foreach (KeyValuePair<byte ,Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel> axis in mode.Axes)
					{
						if (axis.Value.IdJoyOutput == outJoy)
						{
							axisUsed.Add(axis.Value.OutputAxis);
						}
					}
				}
			}

			for (int idx = 0; idx < caps.NumberInputDataIndices; idx++)
			{
				foreach (HID.HIDP_BUTTON_CAPS bt in bcaps)
				{
					if (bt.Anonymous.Range.DataIndexMin == idx)
					{
						if (!profile.ButtonsMap.TryGetValue(joyId, out Shared.ProfileModel.ButtonMapModel newv))
						{
							profile.ButtonsMap.Add(joyId, newv = new());
						}
						if (!newv.Modes.TryGetValue(0, out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
						{
							newv.Modes.Add(0, mode = new());
						}

						for (byte i = 0; i < (bt.Anonymous.Range.DataIndexMax - bt.Anonymous.Range.DataIndexMin + 1); i++)
						{
							Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = new() { Type = 0 };
							button.Actions.Add((ushort)((outJoy * 128) + btId + 1));
							mode.Buttons.Add(btId, button);
							btId++;
						}
						break;
					}
				}

				foreach (HID.HIDP_VALUE_CAPS val in vcaps)
				{
					if (val.Anonymous.NotRange.DataIndex == idx)
					{
						if (val.Anonymous.NotRange.Usage == 0)
						{
							break;
						}
						if ((val.Anonymous.NotRange.Usage != 57) && (val.LogicalMin != 0)) { throw new NotImplementedException(); }

						byte destinationAxis = val.Anonymous.NotRange.Usage switch
						{
							36 => 0,
							38 => 1,
							48 => 0, //x
							49 => 1, //y
							50 => 2, //z
							51 => 3, //rx
							52 => 4, //ry
							53 => 5, //rz
							54 => 6, //slider
							55 => 6, //dial
							0x38 => 6, //Wheel
							57 => 253, //hat
							0x40 => 9, //vx
							0x41 => 10, //vy
							0x42 => 11, //vz
							0x43 => 12, //vbrx
							0x44 => 13, //vbry
							0x45 => 14, //vbrz
							0x46 => 6, //vno
							_ => throw new NotImplementedException()
						};

						if (destinationAxis == 253)
						{
							if (hatId == 4)
							{
								//	MessageBox.Show(CTranslate.Get("not enough axes"), CTranslate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Warning);
							}
							else
							{
								if ((val.LogicalMax + 1 - val.LogicalMin) != 8 && (val.LogicalMax + 1 - val.LogicalMin) != 4) { throw new NotImplementedException(); }
								if (!profile.HatsMap.TryGetValue(joyId, out Shared.ProfileModel.ButtonMapModel newv))
								{
									profile.HatsMap.Add(joyId, newv = new());
								}
								if (!newv.Modes.TryGetValue(0, out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
								{
									newv.Modes.Add(0, mode = new());
								}
								for (byte pos = 0; pos < 8; pos++)
								{
									Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel hat = new() {
										Type = 0,
										Actions = [(ushort)(385 + (outJoy * 32) + (hatId * 8) + pos)]
									};
									mode.Buttons.Add((byte)((hatId * 8) + pos), hat);
								}
								hatId++;
							}
						}
						else
						{
							if (!profile.AxesMap.TryGetValue(joyId, out Shared.ProfileModel.AxisMapModel newv))
							{
								profile.AxesMap.Add(joyId, newv = new());
							}
							if (!newv.Modes.TryGetValue(0, out Shared.ProfileModel.AxisMapModel.ModeModel mode))
							{
								newv.Modes.Add(0, mode = new());
							}
							if (!axisUsed.Contains(destinationAxis))
							{
								Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel newAxis = new()
								{
									IdJoyOutput = outJoy,
									Type = 1,
									OutputAxis = destinationAxis
								};
								if (destinationAxis == 6)
								{
									newAxis.IsSensibilityForSlider = true;
								}
								mode.Axes.Add(axId++, newAxis);
								axisUsed.Add(destinationAxis);
							}
							else if (destinationAxis == 6)
							{
								//bool limit = true;
								for (byte na = 7; na < 9; na++)
								{
									if (!axisUsed.Contains(na))
									{
										Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel newSlider = new()
										{
											IdJoyOutput = outJoy,
											Type = 1,
											OutputAxis = na,
											IsSensibilityForSlider = true
										};
										mode.Axes.Add(axId++, newSlider);
										axisUsed.Add(na);
										//limit = false;
										break;
									}
								}
								//if (limit)
								//{
								//	MessageBox.Show(CTranslate.Get("not enough axes"), CTranslate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Warning);
								//}
							}
							//else
							//{
							//	MessageBox.Show(CTranslate.Get("not enough axes"), CTranslate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Warning);
							//}
						}

						break;
					}
				}
			}

			return true;
		}
	}
}
