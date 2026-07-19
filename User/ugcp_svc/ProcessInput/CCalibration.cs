using Shared;
using Shared.Calibration.LowLevel;
using System;

namespace ugcp_svc.ProcessInput
{
    static class CCalibration
    {
        public static void Calibrate(Profile.CProfile refProfile, uint joyId, HIDInput.HID_INPUT_DATA refHidData)
        {
            refProfile.GetCalibration().BeginCalibrationRead();

            CTypes.STLIMITS[] refLimits = [];
            CTypes.STJITTER[] refJitters = [];
            {
                ReadOnlySpan<JoyLimits> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(refProfile.GetCalibration().Limits);
                for (int lidx = 0; lidx < span.Length; lidx++)
                {
                    if (span[lidx].JoyId == joyId)
                    {
                        refLimits = span[lidx].Limits;
                        break;
                    }
                }
            }

            {
                ReadOnlySpan<JoyJitters> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(refProfile.GetCalibration().Jitters);
                for (int jidx = 0; jidx < span.Length; jidx++)
                {
                    if (span[jidx].JoyId == joyId)
                    {
                        refJitters = span[jidx].Jitters;
                        break;
                    }
                }
            }

            ReadOnlySpan<CTypes.STLIMITS> limits = refLimits;
            ReadOnlySpan<CTypes.STJITTER> jitters = refJitters;


            byte idx = 0;

            // Antijitter

            for (byte jidx = 0; jidx < jitters.Length; jidx++)
            {
                CTypes.STJITTER jitt = jitters[jidx];
                if (jitt.Antiv == 1)
                {
                    ushort pollAxis = refHidData.Axis[idx];

                    if ((pollAxis == jitt.PosChosen) || (pollAxis < (jitt.PosChosen - jitt.Margin)) || (pollAxis > (jitt.PosChosen + jitt.Margin)))
                    {
                        jitt.PosRepeated = 0;
                        jitt.PosChosen = pollAxis;
                    }
                    else
                    {
                        if (jitt.PosRepeated < jitt.Strength)
                        {
                            jitt.PosRepeated++;
                            refHidData.Axis[idx] = jitt.PosChosen;
                        }
                        else
                        {
                            jitt.PosRepeated = 0;
                            jitt.PosChosen = pollAxis;
                        }
                    }
                }
                idx++;
            }

            // Calibration

            idx = 0;
            for (byte lidx = 0; lidx < limits.Length; lidx++)
	        {
                CTypes.STLIMITS limit = limits[lidx];
                {
                    ushort pollAxis = refHidData.Axis[idx];
                    ushort width1, width2;
                    width1 = (ushort)(limit.Center - limit.Null - limit.Left);
                    width2 = (ushort)(limit.Right - (limit.Center + limit.Null));

                    if (((pollAxis >= (limit.Center - limit.Null)) && (pollAxis <= (limit.Center + limit.Null))))
                    {
                        //Null zone
                        refHidData.Axis[idx++] = limit.Center;
                        continue;
                    }
                    else
                    {
                        if (pollAxis < limit.Left)
                            pollAxis = 0;
                        else if (pollAxis >= limit.Right)
                            pollAxis = limit.Right;
                        else
                        {
                            if (pollAxis < limit.Center)
                            {
                                if (width1 != 0)
                                {
                                    pollAxis -= limit.Left;
                                    pollAxis = (ushort)(((pollAxis * limit.Center) + (width1 / 2)) / width1); //Equivalent to round function
                                }
                                else
                                {
                                    pollAxis = limit.Center;
                                }
                            }
                            else
                            {
                                if (width2 != 0)
                                {
                                    pollAxis -= (ushort)(limit.Center + limit.Null + 1); //move range to 0
                                    pollAxis = (ushort)(((pollAxis * (limit.Right - limit.Center)) + (width2 / 2)) / width2); //Equivalent to round function
                                    pollAxis += (ushort)(limit.Center + 1);
                                }
                                else
                                {
                                    pollAxis = limit.Center;
                                }
                            }
                        }
                    }

                    refHidData.Axis[idx] = pollAxis;
                }

                idx++;
            }

            refProfile.GetCalibration().EndCalibrationRead();
        }
    }
}
