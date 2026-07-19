namespace ugcp_svc.ProcessInput.GenerateEvents
{
    static class CAxes
    {
        public static void SensibilityAndMapping(Profile.CProfile pProfile, uint joyId, ref HIDInput.HID_INPUT_DATA old, ref HIDInput.HID_INPUT_DATA input)
        {
            byte idx;
            byte mode;
            {
                Profile.CStatus refStatus = pProfile.GetStatus();
                mode = (byte)((refStatus.SubMode << 4) | refStatus.Mode);
            }

            byte[] mapped = [ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ]; //16 = Max vJoy devs
            ProcessOutput.VHID_INPUT_DATA[] output = new ProcessOutput.VHID_INPUT_DATA[16];

            //Sensibility
            for (idx = 0; idx < 24; idx++)
            {
                double x1 = 0, x2 = 0, y1 = 0, y2 = 0, m1 = 0, m2 = 0;
                double range, center;

                bool slider = false;
                //double inertia = 0;
                double dampingK = 0;
                double softDeadZone = 0;
                {
                    ref readonly Profile.CProgramming.AXISMODEL axisMap = ref pProfile.GetProfile().AxesMap.GetConf(joyId, mode, idx, out bool ok);
                    if (!ok)
                    {
                        continue;
                    }
                    slider = axisMap.IsSlider;
                    //inertia = axisMap->Inertia;
                    dampingK = axisMap.DampingK;
                    softDeadZone = axisMap.SoftDeadZone / 100.0;
                }

                pProfile.GetCalibration().BeginCalibrationRead();
                {
                    ref readonly Shared.CTypes.STLIMITS cal = ref pProfile.GetCalibration().GetLimit(joyId, idx, out bool ok);
                    if (ok)
                    {
                        range = cal.Right;
                        center = cal.Center;
                    }
                    else
                    {
                        pProfile.GetCalibration().EndCalibrationRead();
                        continue;
                    }
                }
                pProfile.GetCalibration().EndCalibrationRead();

                //normalize 0.0 - 1.0 for interpolation
                double normalPos;
                bool left = input.Axis[idx] < center;
                bool truncatedMax = false;
                if (slider)
                {
                    normalPos = input.Axis[idx] / range;
                }
                else
                {
                    if (input.Axis[idx] == center)
                    {
                        input.Axis[idx] = 16383;
                        continue;
                    }
                    normalPos = left ? ((center - input.Axis[idx]) / center) : (input.Axis[idx] - center) / (range - center);
                }

                //soft dead zone
                if ((softDeadZone > 0) && (normalPos < softDeadZone))
                {
                    double u = normalPos / softDeadZone;
                    normalPos = softDeadZone * (6 * u * u * u * u * u - 15 * u * u * u * u + 10 * u * u * u);
                }

                {
                    ref readonly Profile.CProgramming.AXISMODEL axisMap = ref pProfile.GetProfile().AxesMap.GetConf(joyId, mode, idx, out bool ok);
                    if (ok)
                    {
                        if (normalPos >= axisMap.SensibilityX[19])
                        {
                            truncatedMax = true;
                            normalPos = axisMap.SensibilityY[19];
                        }
                        else
                        {
                            if (normalPos < axisMap.SensibilityX[0])
                            {
                                x1 = 0;
                                x2 = axisMap.SensibilityX[0];
                                y1 = 0;
                                y2 = axisMap.SensibilityY[0];
                                m1 = axisMap.SensibilityY[0] / axisMap.SensibilityX[0];
                                m2 = axisMap.SensibilityS[0];
                            }
                            else
                            {
                                for (byte pos = 0; pos < 19; pos++)
                                {
                                    if (normalPos < axisMap.SensibilityX[pos + 1])
                                    {
                                        x1 = axisMap.SensibilityX[pos];
                                        x2 = axisMap.SensibilityX[pos + 1];
                                        y1 = axisMap.SensibilityY[pos];
                                        y2 = axisMap.SensibilityY[pos + 1];
                                        m1 = axisMap.SensibilityS[pos];
                                        m2 = axisMap.SensibilityS[pos + 1];
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!truncatedMax) // interpolation
                {
                    double h = x2 - x1;
                    double s0 = (normalPos - x1) / h;

                    double h00 = (1 + 2 * s0) * (1 - s0) * (1 - s0);
                    double h10 = s0 * (1 - s0) * (1 - s0);
                    double h01 = s0 * s0 * (3 - 2 * s0);
                    double h11 = s0 * s0 * (s0 - 1);

                    normalPos = h00 * y1 + h10 * h * m1 + h01 * y2 + h11 * h * m2;
                }

                //damping & inertia
                {
                    double lastPos = normalPos;
                    double lastVelocity = 0;
                    //double lastInertia = 0;

                    ref Profile.CStatus.ST_AXIS pStatus = ref pProfile.GetStatus().Axes.GetStatus(joyId, mode, idx, out bool ok);
                    if (ok)
                    {
                        lastPos = pStatus.LastPos;
                        lastVelocity = pStatus.LastVelocity;
                        //lastInertia = pStatus->LastInertiaPos;
                    }

                    double vel = normalPos - lastPos;
                    double acc = vel - lastVelocity;
                    double centerFactor = 1.0 - normalPos;
                    double dampedPos = normalPos - dampingK * acc * centerFactor; //damping k = 0.25 (flight)

                    // clamp
                    if (dampedPos > 1.0) { dampedPos = 1.0; }

                    if (dampingK == 0) //disabled
                    {
                        dampedPos = normalPos;
                    }

                    //inertia
                    //normalPos = (dampedPos * (1.0 - inertia)) + (lastInertia * inertia);
                    normalPos = dampedPos;

                    if (ok)
                    {
                        pStatus.LastPos = dampedPos;
                        pStatus.LastVelocity = vel;
                        //pStatus->LastInertiaPos = normalPos;
                    }
                }


                if (slider) //scale to vJoy
                {
                    input.Axis[idx] = (ushort)(normalPos * 32767);
                }
                else
                {
                    input.Axis[idx] = (ushort)(left ? 16383 * (1.0 - normalPos) : 16383 + (normalPos * 16384));
                }
            }

            //Mapping
            for (idx = 0; idx < 24; idx++)
            {
                byte vJoy;
                byte axisType;
                byte outputAxis;
                byte mouseSens; //t

                {
                    ref readonly Profile.CProgramming.AXISMODEL axisMap = ref pProfile.GetProfile().AxesMap.GetConf(joyId, mode, idx, out bool ok);
                    if (!ok)
                    {
                        continue;
                    }
                    vJoy = axisMap.VJoyOutput;
                    axisType = axisMap.Type;
                    outputAxis = axisMap.OutputAxis;
                    mouseSens = axisMap.MouseSensibility;
                }

                if (axisType == 0)
                {
                    continue;
                }
                else if ((axisType & 1) == 1) // normal axis
                {
                    output[vJoy].Axes[outputAxis] = input.Axis[idx];
                    if ((axisType & 0x2) == 2) //inverted normal
                    {
                        output[vJoy].Axes[outputAxis] = (ushort)(32767 - output[vJoy].Axes[outputAxis]);
                    }
                    mapped[vJoy] |= (byte)(1 << outputAxis);
                }
                else if ((axisType & 0x8) != 0) //map to mouse
                {
                    if (input.Axis[idx] != old.Axis[idx])
                    {
                        if (input.Axis[idx] == 16383)
                        {
                            Axis2Mouse(outputAxis, 0);
                        }
                        else
                        {
                            int axisTransformed = input.Axis[idx] - 16383;
                            if ((axisType & 0x2) == 0x2) //inverted
                            {
                                Axis2Mouse(outputAxis, (sbyte)(-axisTransformed * mouseSens));
                            }
                            else
                            {
                                Axis2Mouse(outputAxis, (sbyte)(axisTransformed * mouseSens));
                            }

                        }
                    }
                }
            }

            for (idx = 0; idx < 16; idx++)
            {
                if (mapped[idx] != 0)
                {
                    CGenerateEvents.DirectX(idx, mapped[idx], ref output[idx]);
                }
            }
        }

        private static void Axis2Mouse(byte axis, sbyte mov)
        {
            EventQueue.EV_COMMAND evt = new();

            if (axis == 0)
            {
                if (mov == 0)
                {
                    evt.Type = EventQueue.CommandType.Release | EventQueue.CommandType.MouseLeft;
                    evt.Data.Basic.Data1 = 0;
                }
                else
                {
                    if (mov >= 0)
                    {
                        evt.Type = EventQueue.CommandType.MouseRight;
                        evt.Data.Basic.Data1 = (byte)mov;
                    }
                    else
                    {
                        evt.Type = EventQueue.CommandType.MouseLeft;

                        evt.Data.Basic.Data1 = (byte)-mov;
                    }
                }
            }
            else
            {
                if (mov == 0)
                {
                    evt.Type = EventQueue.CommandType.Release | EventQueue.CommandType.MouseUp;
                    evt.Data.Basic.Data1 = 0;
                }
                else
                {
                    if (mov >= 0)
                    {
                        evt.Type = EventQueue.CommandType.MouseDown;
                        evt.Data.Basic.Data1 = (byte)mov;
                    }
                    else
                    {
                        evt.Type = EventQueue.CommandType.MouseUp;
                        evt.Data.Basic.Data1 = (byte)-mov;
                    }
                }
            }

            CGenerateEvents.Mouse(ref evt);
        }

        public static void MoveAxis(Profile.CProfile pProfile, uint joyId, byte idx, ushort _new)
        {
            ushort actionId = 0;
            byte change;
            EventQueue.EV_COMMAND axisData = new();
            byte mode;

            {
                Profile.CStatus refStatus = pProfile.GetStatus();
                mode = (byte)((refStatus.SubMode << 4) | refStatus.Mode);
                change = TranslateRotary(pProfile, joyId, idx, _new, mode);
                ref Profile.CStatus.ST_AXIS pStatus = ref refStatus.Axes.GetStatus(joyId, mode, idx, out bool ok);
                if (ok)
                {
                    axisData.Data.Extended.Incremental = pStatus.IncrementalPos;
                    axisData.Data.Extended.Band = pStatus.Band;
                }
            }
            if (change != 255)
            {
                ref readonly var axisMap = ref pProfile.GetProfile().AxesMap.GetConf(joyId, mode, idx, out bool ok);
                if (ok)
                {
                    actionId = axisMap.Actions[change];
                    axisData.Data.Extended.Mode = (byte)(mode & 0xf);
                    axisData.Data.Extended.Submode = (byte)(mode >> 4);
                }
            }


            if (change != 255)
            {
                if (actionId == 0)
                {
                    CGenerateEvents.CheckHolds();
                }
                else
                {
                    CGenerateEvents.Command(pProfile, joyId, actionId, idx, CGenerateEvents.Origin.Axis, ref axisData);
                }
            }
        }

        /// <summary>
        /// Inside LockStatus and LockProfile
        /// </summary>
        private static byte TranslateRotary(Profile.CProfile pProfile, uint joyId, byte axis, ushort _new, byte mode)
        {
            byte idn = 255;
            bool incremental;
            bool bands;

            ref readonly var axisMap = ref pProfile.GetProfile().AxesMap.GetConf(joyId, mode, axis, out bool ok);
            if (!ok)
            {
                return 255;
            }

            incremental = (axisMap.Type & 16) == 16;
            bands = (axisMap.Type & 32) == 32;


            ref readonly Shared.CTypes.STLIMITS pl = ref pProfile.GetCalibration().GetLimit(joyId, axis, out ok);
            if (!ok)
            {
                return 255;
            }
            ushort range = pl.Range;
            ushort newPos = _new;

            ref Profile.CStatus.ST_AXIS status = ref pProfile.GetStatus().Axes.GetStatus(joyId, mode, axis, out ok);
            if (!ok)
            {
                return 255;
            }

            if (incremental)
            {
                ushort old = status.IncrementalPos;
                if (_new > old)
                {
                    byte positions = axisMap.ToughnessInc;
                    if (old < (range - positions))
                    {
                        if (newPos > (old + positions))
                        {
                            status.IncrementalPos = newPos;
                            idn = 0;
                        }
                    }
                }
                else
                {
                    byte positions = axisMap.ToughnessDec;
                    if (old > positions)
                    {
                        if (newPos < (old - positions))
                        {
                            status.IncrementalPos = newPos;
                            idn = 1;
                        }
                    }
                }
            }
            else if (bands)
            {

                byte oldBand = status.Band;
                byte currentBand = 255;
                ushort previousPos = 0;
                byte idc = 0;

                for (byte i = 0; i < axisMap.Bands.Length; i++)
                {
                    bool exit = false;
                    byte band = axisMap.Bands[i];

                    if ((band == 0) || (band >= 100))
                    {
                        band = 100;
                        exit = true;
                    }

                    if ((newPos >= previousPos) && (newPos < ((band * range) / 100)))
                    {
                        currentBand = idc;
                        break;
                    }
                    if (exit)
                    {
                        break;
                    }
                    previousPos = (ushort)((band * range) / 100);
                    idc++;
                }
                if ((currentBand == 255) && (newPos >= previousPos))
                {
                    currentBand = idc;
                }
                if ((currentBand != 255) && (currentBand != oldBand))
                {
                    status.Band = currentBand;

                    idn = currentBand;
                }
            }

            return idn;
        }
    }
}
