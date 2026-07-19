using System.Linq;

namespace ugcp_svc.Profile
{
    sealed class CProgramming
    {
        #region Button data
        public struct BUTTONMODEL
        {
            public ushort[] Actions;
            public byte Type;  //0: normal, 1:Toggle
            public byte ButtonId;
        }

        public struct BUTTONMODEMODEL
        {
            public BUTTONMODEL[] Buttons;
            public byte ModeId;
        }
        public struct BUTTONS_MAP
        {
            public BUTTONMODEMODEL[] Modes;
            public uint JoyId;
        }
        #endregion

        #region Axes data
        public struct AXISMODEL
        {
            public ushort[] Actions;
            public double[] SensibilityX;//[20]; // curve points in X, last point MUST be 1.0
            public double[] SensibilityY;//[20]; // curve points in Y
            public double[] SensibilityS;//[20]; // slopes for interpolation
            public byte[] Bands;
            public double DampingK;
            //double Inertia;
            public byte MouseSensibility;
            public byte VJoyOutput;
            public byte Type;  //Mapped in bits 0:none, 1:Normal, 10:Inverted, 100:Mini, 1000:Mouse, 10000:Incremental, 100000: Bands
            public byte OutputAxis; //0:X, Y, Z, Rx, Ry, Rz, Sl1, Sl2
            public byte SoftDeadZone;
            public byte ToughnessInc;
            public byte ToughnessDec;
            public byte AxisId;
            public bool IsSlider;
        }

        public struct AXISMODEMODEL
        {
            public AXISMODEL[] Axes;
            public byte ModeId;
        }

        public struct AXES_MAP
        {
            public AXISMODEMODEL[] Modes;
            public uint JoyId;
        }
        #endregion

        private BUTTONS_MAP[] stButtonsMap;
        private BUTTONS_MAP[] stHatsMap;
        private AXES_MAP[] stAxesMap;

        public sealed class CButtonMap
        {
            private BUTTONS_MAP[] pStButtonsMap;

            public CButtonMap(ref BUTTONS_MAP[] pStButtonsMap) { this.pStButtonsMap = pStButtonsMap; }

            public void LoadProfile(System.Collections.Generic.Dictionary<uint, Shared.ProfileModel.ButtonMapModel> buttonProfile)
            {
                pStButtonsMap = new BUTTONS_MAP[buttonProfile.Count];
                byte idxm = 0;
                foreach (var map in buttonProfile)
                {
                    ref var newMap = ref pStButtonsMap[idxm++];
                    newMap.JoyId = map.Key;
                    newMap.Modes = new BUTTONMODEMODEL[map.Value.Modes.Count];
                    byte idxMode = 0;
                    foreach(var mode in map.Value.Modes.OrderBy(x => x.Key))
                    {
                        BUTTONMODEMODEL newMode = new() { ModeId = mode.Key, Buttons = new BUTTONMODEL[mode.Value.Buttons.Count] };
                        newMap.Modes[idxMode++] = newMode;
                        byte idxButton = 0;
                        foreach (var button in mode.Value.Buttons.OrderBy(x => x.Key))
                        {
                            ref var data = ref newMode.Buttons[idxButton++];
                            data.ButtonId = button.Key;
                            data.Type = button.Value.Type;
                            data.Actions = [.. button.Value.Actions];
                        }
                    }
                }
            }

            private static readonly BUTTONMODEL emptyButton;
            public ref readonly BUTTONMODEL GetConf(uint joyId, byte mode, byte button, out bool ok)
            {
                for (byte j = 0; j < pStButtonsMap.Length; j++)
                {
                    ref var joy = ref pStButtonsMap[j];
                    if (joy.JoyId == joyId)
                    {
                        for(byte m = 0; m < joy.Modes.Length; m++)
                        {
                            ref var jmode = ref joy.Modes[m];
                            if (jmode.ModeId == mode)
                            {
                                System.ReadOnlySpan<BUTTONMODEL> span = jmode.Buttons;
                                for (byte b = 0; b < span.Length; b++)
                                {
                                    ref readonly var bt = ref span[b];
                                    if (bt.ButtonId == button)
                                    {
                                        ok = true;
                                        return ref bt;
                                    }
                                }
                            }
                        }

                    }
                }
                ok = false;
                return ref emptyButton;
            }
        }

        public sealed class CAxesMap
        {
            private AXES_MAP[] pStAxesMap;

            public CAxesMap(ref AXES_MAP[] pStAxesMap) { this.pStAxesMap = pStAxesMap; }

            public void LoadProfile(System.Collections.Generic.Dictionary<uint, Shared.ProfileModel.AxisMapModel> axisProfile)
            {
                pStAxesMap = new AXES_MAP[axisProfile.Count];
                byte idxm = 0;
                foreach (var map in axisProfile)
                {
                    ref var newMap = ref pStAxesMap[idxm++];
                    newMap.JoyId = map.Key;
                    newMap.Modes = new AXISMODEMODEL[map.Value.Modes.Count];
                    byte idxMode = 0;
                    foreach (var mode in map.Value.Modes.OrderBy(x => x.Key))
                    {
                        AXISMODEMODEL newMode = new() { ModeId = mode.Key, Axes = new AXISMODEL[mode.Value.Axes.Count] };
                        newMap.Modes[idxMode++] = newMode;
                        byte idxButton = 0;
                        foreach (var axis in mode.Value.Axes.OrderBy(x => x.Key))
                        {
                            ref var data = ref newMode.Axes[idxButton++];
                            data.IsSlider = axis.Value.IsSensibilityForSlider;
                            data.AxisId = axis.Key;
                            data.MouseSensibility = axis.Value.Mouse;
                            data.VJoyOutput = axis.Value.IdJoyOutput;
                            data.Type = axis.Value.Type;
                            data.OutputAxis = axis.Value.OutputAxis;
                            data.SoftDeadZone = axis.Value.SoftDeadZone;
                            data.ToughnessInc = axis.Value.Resistance.Increment;
                            data.ToughnessDec = axis.Value.Resistance.Decrement;
                            data.DampingK = axis.Value.DampingK;
                            data.Bands = [.. axis.Value.Zones];
                            data.SensibilityX = axis.Value.SensibilityX;//[20]; // curve points in X, last point MUST be 1.0
                            data.SensibilityY = axis.Value.SensibilityY;//[20]; // curve points in Y
                            data.SensibilityS = axis.Value.SensibilityS;//[20]; // slopes for interpolation
                            data.Actions = [.. axis.Value.Actions];
                        }
                    }
                }
            }

            private static AXISMODEL emptyAxis;
            public ref readonly AXISMODEL GetConf(uint joyId, byte mode, byte axis, out bool ok)
            {
                for (byte j = 0; j < pStAxesMap.Length; j++)
                {
                    ref var joy = ref pStAxesMap[j];
                    if (joy.JoyId == joyId)
                    {
                        for (byte m = 0; m < joy.Modes.Length; m++)
                        {
                            ref var jmode = ref joy.Modes[m];
                            if (jmode.ModeId == mode)
                            {
                                System.ReadOnlySpan<AXISMODEL> span = jmode.Axes;
                                for (byte a = 0; a < span.Length; a++)
                                {
                                    ref readonly var ax = ref span[a];
                                    if (ax.AxisId == axis)
                                    {
                                        ok = true;
                                        return ref ax;
                                    }
                                }
                            }
                        }

                    }
                }
                ok = false;
                return ref emptyAxis;
            }
        }

        public CButtonMap ButtonsMap;
        public CButtonMap HatsMap;
        public CAxesMap AxesMap;
        private System.Collections.Generic.List<EventQueue.EV_COMMAND[]> _actions = [];
        public byte MouseTick = 10;
        
        public CProgramming()
        {
            stButtonsMap = [];
            stHatsMap = [];
            stAxesMap = [];
            ButtonsMap = new(ref stButtonsMap);
            HatsMap = new(ref stHatsMap);
            AxesMap = new(ref stAxesMap);
        }

        public System.ReadOnlySpan<EventQueue.EV_COMMAND[]> ReadActions() => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_actions);
        public System.Collections.Generic.List<EventQueue.EV_COMMAND[]> WriteActions() => _actions;


        public byte GetRawDevice(uint joyId) //to bypass profile and test/calibrate
        {
            System.Collections.Generic.Dictionary<uint, bool> ids = [];
            foreach (var id in stButtonsMap)
            {
                ids.TryAdd( id.JoyId, false);
            }
            foreach (var id in stHatsMap)
            {
                ids.TryAdd(id.JoyId, false);
            }
            foreach (var id in stAxesMap)
            {
                ids.TryAdd(id.JoyId, false);
            }
            byte retId = 1;
            foreach (var joy in ids)
            {
                if (joy.Key == joyId) return retId;
                retId++;
                if (retId == 4) break;
            }
            return 0;
        }

        public bool DeviceIncluded(uint joyId)
        {
            foreach (var id in stAxesMap)
            {
                if (id.JoyId == joyId) return true;
            }
            foreach (var id in stButtonsMap)
            {
                if (id.JoyId == joyId) return true;
            }
            foreach (var id in stHatsMap)
            {
                if (id.JoyId == joyId) return true;
            }
            return false;
        }
    }
}
