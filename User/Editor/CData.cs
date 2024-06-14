using System;
using System.Collections.Generic;
using Shared;
//using System.Windows.Media.Imaging;

namespace Profiler
{
    class CDatos : IDisposable
    {
        public ProfileModel Profile { get; private set; } = new();
        public bool Modified { get; set; } = false;

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Profile.AxesMap.Clear();
                    Profile.ButtonsMap.Clear();
                    Profile.Macros.Clear();
                }

                disposedValue = true;
            }
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion

        public void New()
        {
            Profile = new();

            Profile.Macros.Add(new() { Id = 0, Name = Translate.Get("profile none") });

            Modified = true;
        }

        public async System.Threading.Tasks.Task<bool> Load(string filename)
        {
            try
            {
                string json = System.IO.File.ReadAllText(filename);
                Profile = System.Text.Json.JsonSerializer.Deserialize<Shared.ProfileModel>(json);
            }
            catch (Exception ex)
            {
                try
                {
                    using OldProfile.DSPerfil old = new();
                    old.ReadXml(filename);
                    ConvertOld(old);
                    Modified = false;
                    return true;
                }
                catch { }
                await MessageBox.Show(ex.Message, Translate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Modified = false;
            return true;
        }

        public async System.Threading.Tasks.Task<bool> Save(string filename, List<ProfileModel.DeviceInfo> devs)
        {
            //Reorder action ids
            ushort id = 0;
            foreach (ProfileModel.MacroModel ar in Profile.Macros)
            {
                if (ar.Id != id)
                {
                    foreach (KeyValuePair<uint, ProfileModel.AxisMapModel> amm in Profile.AxesMap)
                    {
                        foreach (KeyValuePair<byte, ProfileModel.AxisMapModel.ModeModel> mm in amm.Value.Modes)
                        {
                            foreach (KeyValuePair<byte, ProfileModel.AxisMapModel.ModeModel.AxisModel> am in mm.Value.Axes)
                            {
                                for (int i = 0; i < am.Value.Actions.Count; i++)
                                {
                                    if (am.Value.Actions[i] == ar.Id) { am.Value.Actions[i] = id; }
                                }
                            }
                        }
                    }
                    foreach (KeyValuePair<uint, ProfileModel.ButtonMapModel> bmm in Profile.ButtonsMap)
                    {
                        foreach (KeyValuePair<byte, ProfileModel.ButtonMapModel.ModeModel> mm in bmm.Value.Modes)
                        {
                            foreach (KeyValuePair<byte, ProfileModel.ButtonMapModel.ModeModel.ButtonModel> bm in mm.Value.Buttons)
                            {
                                for (int i = 0; i < bm.Value.Actions.Count; i++)
                                {
                                    if (bm.Value.Actions[i] == ar.Id) { bm.Value.Actions[i] = id; }
                                }
                            }
                        }
                    }
                }
                id++;
            }

            List<uint> ids = [];
            foreach (KeyValuePair<uint, ProfileModel.ButtonMapModel> kv in Profile.ButtonsMap)
            {
                if (!ids.Contains(kv.Key))
                {
                    ids.Add(kv.Key);
                }
            }
            foreach (KeyValuePair<uint, ProfileModel.ButtonMapModel> kv in Profile.HatsMap)
            {
                if (!ids.Contains(kv.Key))
                {
                    ids.Add(kv.Key);
                }
            }
            foreach (KeyValuePair<uint, ProfileModel.AxisMapModel> kv in Profile.AxesMap)
            {
                if (!ids.Contains(kv.Key))
                {
                    ids.Add(kv.Key);
                }
            }
            Profile.DevicesIncluded.Clear();

            foreach (uint hId in ids)
            {
                Profile.DevicesIncluded.Add(devs.Find(x => x.Id == hId));
            }

            try
            {
                System.IO.File.WriteAllText(filename, System.Text.Json.JsonSerializer.Serialize(Profile));
            }
            catch (Exception ex)
            {
                await MessageBox.Show(ex.Message, Translate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Modified = false;
            return true;
        }

        private void ConvertOld(OldProfile.DSPerfil old)
        {
            Profile = new()
            {
                MouseTick = old.GENERAL[0].TickRaton
            };

            using (System.IO.StreamReader r = new(typeof(App).Assembly.GetManifestResourceStream("Profiler.OldProfile.Pedals.json")))
            {
                Profile.DevicesIncluded.Add(System.Text.Json.JsonSerializer.Deserialize<ProfileModel.DeviceInfo>(r.ReadToEnd()));
            }
            using (System.IO.StreamReader r = new(typeof(App).Assembly.GetManifestResourceStream("Profiler.OldProfile.X52.json")))
            {
                Profile.DevicesIncluded.Add(System.Text.Json.JsonSerializer.Deserialize<ProfileModel.DeviceInfo>(r.ReadToEnd()));
            }
            using (System.IO.StreamReader r = new(typeof(App).Assembly.GetManifestResourceStream("Profiler.OldProfile.GladiatosNXT.json")))
            {
                Profile.DevicesIncluded.Add(System.Text.Json.JsonSerializer.Deserialize<ProfileModel.DeviceInfo>(r.ReadToEnd()));
            }


            foreach (OldProfile.DSPerfil.ACCIONESRow ar in old.ACCIONES)
            {
                ProfileModel.MacroModel macro = new()
                {
                    Id = ar.idAccion,
                    Name = ar.Nombre
                };
                if (!ar.IsComandosNull())
                {
                    foreach (ushort action in ar.Comandos)
                    {
                        if (((action & 0x7f) == (byte)CTypes.CommandType.DxButton) || ((action & 0x7f) == (byte)CTypes.CommandType.DxHat))
                        {
                            byte joy = ((byte)((action >> 8) & 0b0111));
                            byte bt = (byte)((action >> 8) >> 3);
                            macro.Commands.Add((uint)((joy << 16) | (bt << 8) | (action & 0xff)));
                        }
                        else
                        {
                            macro.Commands.Add(action);
                        }
                    }
                }
                Profile.Macros.Add(macro);
            }

            foreach (OldProfile.DSPerfil.MAPABOTONESRow br in old.MAPABOTONES)
            {
                byte size = br.TamIndices;
                uint joyId = GetId((OldProfile.Types.JoyType)br.idJoy);
                if (size == 0)
                {
                    size = 2;
                }
                for (byte i = 0; i < size; i++)
                {
                    OldProfile.DSPerfil.INDICESBOTONESRow ibr = old.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(br.idJoy, br.idPinkie, br.idModo, br.idBoton, i);
                    if (ibr != null)
                    {
                        if (!Profile.ButtonsMap.TryGetValue(joyId, out ProfileModel.ButtonMapModel bmm))
                        {
                            bmm = new();
                            Profile.ButtonsMap.Add(joyId, bmm);
                        }
                        if (!bmm.Modes.TryGetValue((byte)((ibr.idPinkie << 4) | ibr.idModo), out ProfileModel.ButtonMapModel.ModeModel mm))
                        {
                            mm = new();
                            bmm.Modes.Add((byte)((ibr.idPinkie << 4) | ibr.idModo), mm);
                        }
                        if (!mm.Buttons.TryGetValue(MapOldButton((OldProfile.Types.JoyType)br.idJoy, (byte)ibr.idBoton), out ProfileModel.ButtonMapModel.ModeModel.ButtonModel bm))
                        {
                            bm = new();
                            mm.Buttons.Add(MapOldButton((OldProfile.Types.JoyType)br.idJoy, (byte)ibr.idBoton), bm);
                        }
                        bm.Type = (byte)(br.TamIndices == 0 ? 0 : 1);
                        bm.Actions.Add(ibr.idAccion);
                    }
                }
            }

            foreach (OldProfile.DSPerfil.MAPAEJESRow er in old.MAPAEJES)
            {
                uint joyId = GetId((OldProfile.Types.JoyType)er.idJoy);
                if (!Profile.AxesMap.TryGetValue(joyId, out ProfileModel.AxisMapModel amm))
                {
                    amm = new();
                    Profile.AxesMap.Add(joyId, amm);
                }
                if (!amm.Modes.TryGetValue((byte)((er.idPinkie << 4) | er.idModo), out ProfileModel.AxisMapModel.ModeModel mm))
                {
                    mm = new();
                    amm.Modes.Add((byte)((er.idPinkie << 4) | er.idModo), mm);
                }
                if (!mm.Axes.TryGetValue(MapOldAxis((OldProfile.Types.JoyType)er.idJoy, er.idEje), out ProfileModel.AxisMapModel.ModeModel.AxisModel am))
                {
                    am = new();
                    mm.Axes.Add(MapOldAxis((OldProfile.Types.JoyType)er.idJoy, er.idEje), am);
                }
                am.Mouse = er.Mouse;
                am.IdJoyOutput = er.IsJoySalidaNull() ? (byte)0 : er.JoySalida;
                am.Type = er.TipoEje;
                am.OutputAxis = er.Eje;
                er.Sensibilidad.CopyTo(am.Sensibility, 0);
                am.IsSensibilityForSlider = er.Slider == 1;
                am.Zones = [.. er.Bandas];
                am.Resistance = (er.ResistenciaInc, er.ResistenciaDec);

                for (byte i= 0; i <= byte.MaxValue; i++)
                {
                    OldProfile.DSPerfil.INDICESEJESRow ier = old.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(er.idJoy, er.idPinkie, er.idModo, er.idEje, i);
                    if (ier == null)
                    {
                        break;
                    }
                    else
                    {
                        am.Actions.Add(ier.idAccion);
                    }
                }
            }

            foreach (OldProfile.DSPerfil.MAPASETASRow br in old.MAPASETAS)
            {
                byte size = br.TamIndices;
                uint joyId = GetId((OldProfile.Types.JoyType)br.idJoy);
                if (size == 0)
                {
                    size = 2;
                }
                for (byte i = 0; i < size; i++)
                {
                    OldProfile.DSPerfil.INDICESSETASRow ibr = old.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(br.idJoy, br.idPinkie, br.idModo, br.IdSeta, i);
                    if (ibr != null)
                    {
                        ProfileModel.ButtonMapModel bmm;
                        if (((OldProfile.Types.JoyType)br.idJoy == OldProfile.Types.JoyType.NXT) && (br.IdSeta > 7))
                        {
                            if (!Profile.ButtonsMap.TryGetValue(joyId, out bmm))
                            {
                                bmm = new();
                                Profile.HatsMap.Add(joyId, bmm);
                            }
                        }
                        else
                        {
                            if (!Profile.HatsMap.TryGetValue(joyId, out bmm))
                            {
                                bmm = new();
                                Profile.HatsMap.Add(joyId, bmm);
                            }
                        }

                        if (!bmm.Modes.TryGetValue((byte)((ibr.idPinkie << 4) | ibr.idModo), out ProfileModel.ButtonMapModel.ModeModel mm))
                        {
                            mm = new();
                            bmm.Modes.Add((byte)((ibr.idPinkie << 4) | ibr.idModo), mm);
                        }
                        if (!mm.Buttons.TryGetValue(MapOldHat((OldProfile.Types.JoyType)br.idJoy, (byte)ibr.idSeta), out ProfileModel.ButtonMapModel.ModeModel.ButtonModel bm))
                        {
                            bm = new();
                            mm.Buttons.Add(MapOldHat((OldProfile.Types.JoyType)br.idJoy, (byte)ibr.idSeta), bm);
                        }
                        bm.Type = (byte)(br.TamIndices == 0 ? 0 : 1);
                        bm.Actions.Add(ibr.idAccion);
                    }
                }
            }
        }

        private static byte MapOldAxis(OldProfile.Types.JoyType joyType, byte axis)
        {
            if (joyType == OldProfile.Types.JoyType.X52_Joy)
            {
                return axis switch
                {
                    3 => 2,
                    _ => axis
                };
            }
            if (joyType == OldProfile.Types.JoyType.X52_Throttle)
            {
                return axis switch
                {
                    0 => 7,
                    1 => 8,
                    2 => 3,
                    3 => 5,
                    //4 => 4,
                    //6 => 6,
                    _ => axis
                };
            }
            else if (joyType == OldProfile.Types.JoyType.Pedals)
            {
                return axis switch
                {
                    5 => 2,
                    _ => axis
                };
            }
            else
            {
                return axis switch
                {
                    2 => 3,
                    3 => 2,
                    6 => 4,
                    7 => 5,
                    _ => axis
                };
            }
        }

        private static byte MapOldHat(OldProfile.Types.JoyType joyType, byte hat)
        {
            if ((joyType == OldProfile.Types.JoyType.NXT) && (hat > 7))
            {
                return hat switch
                {
                    8 => 25,
                    10 => 27,
                    12 => 24,
                    14 => 26,
                    16 => 21,
                    18 => 23,
                    20 => 20,
                    22 => 22,
                    24 => 29,
                    26 => 31,
                    //28 => 28,
                    //30 => 30,
                    _ => hat
                };
            }

            return hat;
        }

        private static byte MapOldButton(OldProfile.Types.JoyType joyType, byte button)
        {
            if (joyType == OldProfile.Types.JoyType.NXT)
            {
                return button switch
                {
                    //0 => 0, //encoder
                    //1 => 1, //encoder
                    //2 => 2, //encoder
                    //3 => 3, //encoder
                    4 => 6,// base 1
                    5 => 5, // base 2
                    6 => 4, // base 3
                    //7 //-
                    //8 => 8, //trigger 2
                    //9 trigger 1
                    //10 => 10, //pinkie
                    //11 => 11, // launch
                    //12 => 12, //hat 1 - center
                    13 => 32, //hat 4 - center
                    14 => 15, //bt 1
                    15 => 35, //fast trigger
                    _ => button
                };
            }

            return button;
        }

        private static uint GetId(OldProfile.Types.JoyType joy)
        {
            return joy switch
            {
                OldProfile.Types.JoyType.NXT => 0x231d0200,
                OldProfile.Types.JoyType.Pedals => 0x06a30763,
                OldProfile.Types.JoyType.X52_Joy => 0x06a30255,
                OldProfile.Types.JoyType.X52_Throttle => 0x06a30255,
                _ => 0x231d012d
            };
        }
    }
}
