using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ugcp_svc.Profile
{
    sealed class CStatus
    {
        #region Button definitions
        public struct ST_BUTTON
        {
            public byte CurrentPosition;
            public byte ButtonId;
        }
        public struct BUTTONMODEMODEL
        {
            public ST_BUTTON[] Status;
            public byte ModeId;
        }

        public struct BUTTONSTATUSMODEL
        {
            public BUTTONMODEMODEL[] Modes;
            public uint JoyId;
        }

        public struct HIDBUTTONMODEL
        {
            public byte[] Status;//[16]{};
            public uint JoyId;
        }
        #endregion

        #region Axis definitions
        public struct ST_AXIS
        {
            public double LastPos;
            public double LastVelocity;
            //double LastInertiaPos = 0;
            public ushort IncrementalPos;
            public byte Band;
            public byte AxisId;
        }

        public struct AXISMODEMODEL
        {
            public ST_AXIS[] Status;
            public byte ModeId;
        }
        public struct AXISSTATUSMODEL
        {
            public AXISMODEMODEL[] Modes;
            public uint JoyId;
        }
        #endregion

        public class CButtons
        {
            private List<BUTTONSTATUSMODEL> pStButtons = [];
            private List<HIDBUTTONMODEL> pStHIDButtons = [];
            public CButtons() {}

            public void NewStatus(Dictionary<uint, Shared.ProfileModel.ButtonMapModel> profileButtons)
            {
                List<BUTTONSTATUSMODEL> newStButtons = [];
                List<HIDBUTTONMODEL> newStHIDButtons = [];
                foreach (var joys in profileButtons)
                {
                    BUTTONSTATUSMODEL nbsm = new() { JoyId = joys.Key, Modes = new BUTTONMODEMODEL[joys.Value.Modes.Count] };
                    newStButtons.Add(nbsm);
                    newStHIDButtons.Add(new() { JoyId = joys.Key, Status = new byte[16] });
                    byte idx1 = 0;
                    foreach (var bmm in joys.Value.Modes)
                    {
                        nbsm.Modes[idx1].ModeId = bmm.Key;
                        nbsm.Modes[idx1].Status = new ST_BUTTON[bmm.Value.Buttons.Count];
                        byte idx2 = 0;
                        foreach (var bt in bmm.Value.Buttons)
                        {
                            nbsm.Modes[idx1].Status[idx2++].ButtonId = bt.Key;
                        }
                        idx1++;
                    }
                }

                System.Threading.Interlocked.Exchange(ref pStButtons, newStButtons);
                System.Threading.Interlocked.Exchange(ref pStHIDButtons, newStHIDButtons);
            }

            public bool GetPos(ref byte pos, uint inputJoyId, byte mode, byte button)
            {
                var spButtons = CollectionsMarshal.AsSpan(pStButtons);
                for(byte j = 0; j < spButtons.Length; j++)
                {
                    ref var map = ref spButtons[j];
                    if (map.JoyId == inputJoyId)
                    {
                        for (byte m = 0; m < map.Modes.Length; m++)
                        {
                            ref var jmode = ref map.Modes[m];
                            if (jmode.ModeId == mode)
                            {
                                for(byte b = 0; b < jmode.Status.Length; b++)
                                {
                                    ref var bt = ref jmode.Status[b];
                                    if (bt.ButtonId == button)
                                    {
                                        pos = bt.CurrentPosition;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            public void SetPos(byte pos, bool _fixed, uint inputJoyId, byte mode, byte button)
		    {
                var spButtons = CollectionsMarshal.AsSpan(pStButtons);
                for (byte j = 0; j < spButtons.Length; j++)
                {
                    ref var map = ref spButtons[j];
                    if (map.JoyId == inputJoyId)
                    {
                        for (byte m = 0; m < map.Modes.Length; m++)
                        {
                            ref var jmode = ref map.Modes[m];
                            if (jmode.ModeId == mode)
                            {
                                for (byte b = 0; b < jmode.Status.Length; b++)
                                {
                                    ref var bt = ref jmode.Status[b];
                                    if (bt.ButtonId == button)
                                    {
                                        bt.CurrentPosition = (byte)(_fixed ? pos : bt.CurrentPosition + pos);
                                    }
                                }
                            }
                        }
                    }
                }
		    }

		    public bool GetPressed(ref byte pressed, uint inputJoyId, byte button)
		    {
                var spHIDButtons = CollectionsMarshal.AsSpan(pStHIDButtons);
                for (byte j = 0; j < spHIDButtons.Length; j++)
                {
                    ref var map = ref spHIDButtons[j];
                    if (map.JoyId == inputJoyId)
                    {
                        pressed = (byte)((map.Status[button / 8] >> (button % 8)) & 1);
                        return true;
                    }
                }
                return false;
		    }

		    public void SetPressed(byte pressed, uint inputJoyId, byte button)
		    {
                var spHIDButtons = CollectionsMarshal.AsSpan(pStHIDButtons);
                for (byte j = 0; j < spHIDButtons.Length; j++)
                {
                    ref var map = ref spHIDButtons[j];
                    if (map.JoyId == inputJoyId)
                    {
                        if (pressed == 1)
                        {
                            map.Status[button / 8] |= (byte)(1 << (button % 8));
                        }
                        else
                        {
                            map.Status[button / 8] &= (byte)~(1 << (button % 8));
                        }
                    }
                }
    		}
	    }

        public class CAxes
        {
            private List<AXISSTATUSMODEL> pStAxes = [];
            public CAxes() {}

            public void NewStatus(Dictionary<uint, Shared.ProfileModel.AxisMapModel> profileAxes)
            {
                List<AXISSTATUSMODEL> newStAxes = [];
                foreach (var joys in profileAxes)
                {
                    AXISSTATUSMODEL nasm = new() { JoyId = joys.Key, Modes = new AXISMODEMODEL[joys.Value.Modes.Count] };
                    newStAxes.Add(nasm);
                    byte idx1 = 0;
                    foreach (var amm in joys.Value.Modes)
                    {
                        nasm.Modes[idx1].ModeId = amm.Key;
                        nasm.Modes[idx1].Status = new ST_AXIS[amm.Value.Axes.Count];
                        byte idx2 = 0;
                        foreach (var ax in amm.Value.Axes)
                        {
                            nasm.Modes[idx1].Status[idx2++].AxisId = ax.Key;
                        }
                        idx1++;
                    }
                }

                System.Threading.Interlocked.Exchange(ref pStAxes, newStAxes);
            }

            private static ST_AXIS empty;
            public ref ST_AXIS GetStatus(uint inputJoyId, byte mode, byte axis, out bool ok)
            {
                var spAxes = CollectionsMarshal.AsSpan(pStAxes);
                for (byte j = 0; j < spAxes.Length; j++)
                {
                    ref var map = ref spAxes[j];
                    if (map.JoyId == inputJoyId)
                    {
                        for (byte m = 0; m < map.Modes.Length; m++)
                        {
                            ref var jmode = ref map.Modes[m];
                            if (jmode.ModeId == mode)
                            {
                                for (byte a = 0; a < jmode.Status.Length; a++)
                                {
                                    ref var ax = ref jmode.Status[a];
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

                ok =  false;
                return ref empty;
            }
        }

        public class CAxesPreciseMode
        {
            private Dictionary<uint, Dictionary<byte, byte>> stAxisPreciseMode;
            public CAxesPreciseMode(ref Dictionary<uint, Dictionary<byte, byte>> stAxes) { this.stAxisPreciseMode = stAxes; }

            public bool GetStatus(ref byte status, uint inputJoyId, byte axis)
            {
                if (stAxisPreciseMode.TryGetValue(inputJoyId, out var ijoy))
                {
                    if (ijoy.TryGetValue(axis, out var iaxis))
                    {
                        status = iaxis;
                        return true;
                    }
                }

                return false;
            }

            public void SetStatus(byte preciseMode, uint inputJoyId, byte axis)
            {
                if (stAxisPreciseMode.TryGetValue(inputJoyId, out var ijoy))
                {
                    if (ijoy.ContainsKey(axis))
                    {
                        ijoy[axis] = preciseMode;
                    }
                }
            }
        }

        public CButtons Buttons;
        public CButtons Hats;
        public CAxes Axes;
        //public CAxesPreciseMode AxisPreciseMode ;
        public byte SubMode = 0;
        public byte Mode = 0;

        public CStatus()
        {
            Buttons = new();
            Hats = new();
            Axes = new();
        }

        public void Reset(Shared.ProfileModel profileHighLevel)
        {
            System.Threading.Interlocked.Exchange(ref Buttons, new CButtons());
            System.Threading.Interlocked.Exchange(ref Hats, new CButtons());
            System.Threading.Interlocked.Exchange(ref Axes, new CAxes());
            //AxisPreciseMode = new(ref stAxisPreciseMode);

            Buttons.NewStatus(profileHighLevel.ButtonsMap);
            Hats.NewStatus(profileHighLevel.HatsMap);
            Axes.NewStatus(profileHighLevel.AxesMap);
        }
    }
}
