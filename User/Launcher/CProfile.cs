using API;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace Launcher
{
    internal class CProfile(object main)
    {
        private readonly CMain main = (CMain)main;

        public bool Load(string? file, System.IO.Pipes.NamedPipeServerStream pipe)
        {
            Shared.ProfileModel? profile;
            if (string.IsNullOrEmpty(file))
            {
                profile = new();
                string? devices = null;
                if (LoadDefault(ref profile, ref devices))
                {
                    file = CTranslate.Get("default");
                    main.MessageBox(devices ?? "/------/", CTranslate.Get("default profile loaded ok"), MessageBoxImage.Information);
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
                    profile = System.Text.Json.JsonSerializer.Deserialize<Shared.ProfileModel>(json) ?? throw new Exception("Profile format");
                }
                catch (Exception ex)
                {
                    main.MessageBox(ex.Message, CTranslate.Get("error"), MessageBoxImage.Warning);
                    return false;
                }
                file = System.IO.Path.GetFileNameWithoutExtension(file);
            }

            #region "Mapping and macros"
            {
                System.Text.StringBuilder sb = new();
                sb.Append((char)CService.MsgType.Map);

                #region "X52 MFD Text"
                {
                    byte[] txtBuffer = new byte[17];
                    string pfName = file;
                    if (pfName.Length > 16)
                        pfName = pfName[..16];
                    else if (pfName.Length == 0)
                        pfName = "";

                    sb.Append((char)1);
                    sb.Append(pfName.PadRight(16, '\0'));
                }
                #endregion

                sb.Append(System.Text.Json.JsonSerializer.Serialize(profile));
                pipe.Write(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
                pipe.Flush();
            }
            #endregion

            return true;
        }

        private static bool LoadDefault(ref Shared.ProfileModel profile, ref string? ndevices)
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

            SetupDi.SP_DEVICE_INTERFACE_DATA diData = new();
            Guid hidGuid = new();
            HID.HidD_GetHidGuid(ref hidGuid);
            IntPtr diDevs = SetupDi.SetupDiGetClassDevsW(ref hidGuid, null, IntPtr.Zero, SetupDi.DIGCF_PRESENT | SetupDi.DIGCF_DEVICEINTERFACE);
            if (Win32.INVALID_HANDLE_VALUE == diDevs)
            {
                return false;
            }

            diData.cbSize = Marshal.SizeOf<SetupDi.SP_DEVICE_INTERFACE_DATA>();
            uint idx = 0;
            byte devicesDetected = 0;
            while (SetupDi.SetupDiEnumDeviceInterfaces(diDevs, IntPtr.Zero, ref hidGuid, idx++, ref diData))
            {
                if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, IntPtr.Zero, 0, out uint size, IntPtr.Zero) && (122 != Marshal.GetLastWin32Error()))
                {
                    continue;
                }

                using Shared.RAII.HGlobalHandle buf = new((int)size);
                Marshal.WriteInt32(buf.DangerousGetHandle(), 8);
                if (!SetupDi.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, buf.DangerousGetHandle(), size, out size, IntPtr.Zero))
                {
                    continue;
                }

                string ninterface = Marshal.PtrToStringAuto(buf.DangerousGetHandle() + 4) ?? "";
                if (!uint.TryParse(ninterface[12..16], System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint joyId))
                {
                    continue;
                }
                joyId <<= 16;
                if (!uint.TryParse(ninterface[21..25], System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint joyId_vid))
                {
                    continue;
                }
                joyId |= joyId_vid;

                using Shared.RAII.UniqueHandle hDev = new(Win32.CreateFileW(ninterface, Win32.GENERIC_READ | Win32.GENERIC_WRITE, Win32.FILE_SHARE_READ | Win32.FILE_SHARE_WRITE, IntPtr.Zero, Win32.OPEN_EXISTING, 0, IntPtr.Zero));
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
                        if (!LoadDefaultDeviceProfile(ref profile, joyId, devicesDetected, pdata, ref caps))
                        {
                            HID.HidD_FreePreparsedData(pdata);
                            continue;
                        }

                        using Shared.RAII.HGlobalHandle psBuf = new(256);
                        HID.HidD_GetProductString(hDev.DangerousGetHandle(), buf.DangerousGetHandle(), 0);
                        if (HID.HidD_GetProductString(hDev.DangerousGetHandle(), buf.DangerousGetHandle(), 256))
                        {
                            selected.Add(Marshal.PtrToStringAuto(buf.DangerousGetHandle())?.Trim() ?? $"{CTranslate.Get("unkown")} {devicesDetected}", joyId);
                        }
                        else
                        {
                            selected.Add($"{CTranslate.Get("unkown")} {devicesDetected}", joyId);
                        }
                        devicesDetected++;
                    }
                }

                HID.HidD_FreePreparsedData(pdata);
            }

            SetupDi.SetupDiDestroyDeviceInfoList(diDevs);


            if (devicesDetected > 0)
            {
                List<string> msj = [];
                byte idOutput = 0;
                foreach (KeyValuePair<string, uint> dev in selected)
                {
                    if (idOutput < 3)
                    {
                        msj.Add(dev.Key);
                    }
                    idOutput++;
                }
                if (idOutput > 3)
                {
                    msj.Add($"... ({idOutput - 3}+)");
                }
                ndevices = string.Join('\n', msj);
                //main.MessageBox(msj, "", MessageBoxImage.Information);
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
                using Shared.RAII.HGlobalHandle ptr = new(Marshal.SizeOf<HID.HIDP_BUTTON_CAPS>() * ustam);
                if (HID.HidP_GetButtonCaps(0, ptr.DangerousGetHandle(), ref ustam, pdata) != HID.HIDP_STATUS_SUCCESS)
                {
                    return false;
                }
                for (int i = 0; i < ustam; i++)
                {
                    bcaps[i] = Marshal.PtrToStructure<HID.HIDP_BUTTON_CAPS>(ptr.DangerousGetHandle() + Marshal.SizeOf<HID.HIDP_VALUE_CAPS>() * i);
                }
            }
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

            byte btId = 0;
            byte axId = 0;
            byte hatId = 0;
            List<byte> axisUsed = [];
            foreach (KeyValuePair<uint, Shared.ProfileModel.AxisMapModel> kvp in profile.AxesMap)
            {
                if (kvp.Value.Modes.TryGetValue(0, out Shared.ProfileModel.AxisMapModel.ModeModel? mode))
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
                        if (!profile.ButtonsMap.TryGetValue(joyId, out Shared.ProfileModel.ButtonMapModel? newv))
                        {
                            profile.ButtonsMap.Add(joyId, newv = new());
                        }
                        if (!newv.Modes.TryGetValue(0, out Shared.ProfileModel.ButtonMapModel.ModeModel? mode))
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
                        if (val.Anonymous.NotRange.Usage == 1) { continue; } //ignorar
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
                                if (!profile.HatsMap.TryGetValue(joyId, out Shared.ProfileModel.ButtonMapModel? newv))
                                {
                                    profile.HatsMap.Add(joyId, newv = new());
                                }
                                if (!newv.Modes.TryGetValue(0, out Shared.ProfileModel.ButtonMapModel.ModeModel? mode))
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
                            if (!profile.AxesMap.TryGetValue(joyId, out Shared.ProfileModel.AxisMapModel? newv))
                            {
                                profile.AxesMap.Add(joyId, newv = new());
                            }
                            if (!newv.Modes.TryGetValue(0, out Shared.ProfileModel.AxisMapModel.ModeModel? mode))
                            {
                                newv.Modes.Add(0, mode = new());
                            }
                            if (!axisUsed.Contains(destinationAxis))
                            {
                                Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel newAxis = new()
                                {
                                    IdJoyOutput = outJoy,
                                    Type = 1,
                                    OutputAxis = destinationAxis,
                                    IsSensibilityForSlider = KnownSliderAxes(joyId, destinationAxis),
                                    DampingK = KnownDamplingAxes(joyId, destinationAxis) ? 0.25 : 0
                                };
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

        private static bool KnownSliderAxes(uint joyId, byte axis)
        {
            if (joyId == 0x6A30763) //Logitech rudder pedals
            {
                return axis switch
                {
                    5 => false,
                    _ => true,
                };
            }
            if (joyId == 0x231D0200) //VKB gladiator nxt
            {
                return axis switch
                {
                    2 => true,
                    _ => false,
                };
            }
            if (joyId == 0x231D012D) //VKB STECS
            {
                return axis switch
                {
                    3 => false,
                    4 => false,
                    _ => true,
                };
            }

            return axis == 6;
        }

        private static bool KnownDamplingAxes(uint joyId, byte axis)
        {
            if (joyId == 0x6A30763)
            {
                return axis switch
                {
                    5 => true,
                    _ => false,
                };
            }
            if (joyId == 0x231D0200)
            {
                return axis switch
                {
                    0 => true,
                    1 => true,
                    5 => true,
                    _ => false,
                };
            }
            if (joyId == 0x231D012D)
            {
                return false;
            }

            return (axis == 0 || axis == 1);
        }
    }
}
