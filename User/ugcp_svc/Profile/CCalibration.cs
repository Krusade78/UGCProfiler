using System;
using System.Collections.Generic;

namespace ugcp_svc.Profile
{
    sealed class CCalibration : System.IDisposable
    {
        private readonly System.Threading.Lock hMutexCalibration = new();

        public List<Shared.Calibration.LowLevel.JoyJitters> Jitters { get; set; } = [];
        public List<Shared.Calibration.LowLevel.JoyLimits> Limits { get; set; } = [];

        void System.IDisposable.Dispose()
        {
            lock (hMutexCalibration)
            {
                Limits.Clear();
                Jitters.Clear();
            }
        }

        public void BeginCalibrationRead() { hMutexCalibration.Enter(); }
        public void EndCalibrationRead() { hMutexCalibration.Exit(); }


        private static readonly Shared.CTypes.STLIMITS lempty;
        public ref readonly Shared.CTypes.STLIMITS GetLimit(uint joyId, byte axis, out bool ok)
        {
            ReadOnlySpan<Shared.Calibration.LowLevel.JoyLimits> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(Limits);
            for (byte i = 0; i < span.Length; i++)
            {
                ref readonly var pl = ref span[i];
                if (pl.JoyId == joyId)
                {
                    if (axis < pl.Limits.Length)
                    {
                        ok = true;
                        return ref pl.Limits[axis];
                    }
                }
            }

            ok = false;
            return ref lempty;
        }

        private static readonly Shared.CTypes.STJITTER jempty;
        public ref readonly Shared.CTypes.STJITTER GetJitter(uint joyId, byte axis, out bool ok)
        {
            ReadOnlySpan<Shared.Calibration.LowLevel.JoyJitters> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(Jitters);
            for (byte i = 0; i < span.Length; i++)
            {
                ref readonly var pj = ref span[i];
                if (pj.JoyId == joyId)
                {
                    if (axis < pj.Jitters.Length)
                    {
                        ok = true;
                        return ref pj.Jitters[axis];
                    }
                }
            }

            ok = false;
            return ref jempty;
        }
    }
}
