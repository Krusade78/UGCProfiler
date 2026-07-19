namespace ugcp_svc.ProcessInput.GenerateEvents
{
    sealed class CButtonsHats : System.IDisposable
    {
        private class TIMER_CTX
        {
            public System.Threading.Timer Timer = null!;
            public CButtonsHats Parent = null!;
            public uint JoyId;
            public byte HatButton_Idx;
        }
        private static CButtonsHats? instance;
        private readonly Profile.CProfile pProfile;
        private readonly System.Threading.Lock hMutexStatus = new();
        private readonly System.Collections.Generic.List<(ulong Id, System.Threading.Timer? Timer)> pStButtonsMap = [];
        private readonly System.Collections.Generic.List<(ulong Id, System.Threading.Timer? Timer)> pStHatsMap = [];

        private CButtonsHats(Profile.CProfile refProfile)
        {
            pProfile = refProfile;
        }

        private byte disposed;
        public void Dispose()
        {
            if (System.Threading.Interlocked.CompareExchange(ref disposed, 1, 0) == 1) return;

            if (instance != null)
            {
                lock (hMutexStatus)
                {
                    foreach (var (_, Timer) in pStButtonsMap)
                    {
                        Timer?.Change(0, System.Threading.Timeout.Infinite);
                    }
                    pStButtonsMap.Clear();

                    foreach (var (_, Timer) in pStHatsMap)
                    {
                        Timer?.Change(0, System.Threading.Timeout.Infinite);
                    }
                    pStHatsMap.Clear();
                }
            }
        }

        public static CButtonsHats GetInstance(Profile.CProfile refProfile)
        {
            instance ??= new CButtonsHats(refProfile);
            return instance;
        }

        public void PressButton(uint joyId, byte idx, bool longPress)
        {
            ushort actionId = 0;
            bool shortPress = false;

            {
                Profile.CStatus refStatus = pProfile.GetStatus();
                byte mode = (byte)(refStatus.Mode | (refStatus.SubMode << 4));

                byte pos = 0;
                if (refStatus.Buttons.GetPos(ref pos, joyId, mode, idx))
                {
                    ref readonly Profile.CProgramming.BUTTONMODEL actions = ref pProfile.GetProfile().ButtonsMap.GetConf(joyId, mode, idx, out bool ok);
                    if (ok)
                    {
                        actionId = actions.Actions[pos];
                        if (actions.Type == 1)
                        {
                            refStatus.Buttons.SetPos(1, false, joyId, mode, idx);
                            if ((byte)(pos + 1) >= actions.Actions.Length)
                            {
                                refStatus.Buttons.SetPos(0, true, joyId, mode, idx);
                            }
                        }
                        else if (actions.Type == 2)
                        {
                            if (pos == 2)
                            {
                                shortPress = true;
                                refStatus.Buttons.SetPos(0, true, joyId, mode, idx);
                            }
                            if (!longPress)
                            {
                                TIMER_CTX ctx = new() { Parent = this, JoyId = joyId, HatButton_Idx = idx  };
                                ctx.Timer = new(EvtLongPress, ctx, 1200, System.Threading.Timeout.Infinite);
                                lock (hMutexStatus)
                                {
                                    pStButtonsMap.Add((((ulong)joyId << 8) | idx, ctx.Timer));
                                }
                                return;
                            }
                        }
                    }
                    refStatus.Buttons.SetPressed(1, joyId, idx);
                }
            }

            CGenerateEvents.Command(pProfile, joyId, actionId, idx, shortPress ? CGenerateEvents.Origin.ButtonShort : CGenerateEvents.Origin.Button);
        }

        public void ReleaseButton(uint joyId, byte idx, bool shortRelease)
        {
            Profile.CStatus refStatus = pProfile.GetStatus();
            byte mode = (byte)(refStatus.Mode | (refStatus.SubMode << 4));
            refStatus.Buttons.SetPressed(0, joyId, idx);


            ref readonly Profile.CProgramming.BUTTONMODEL actions = ref pProfile.GetProfile().ButtonsMap.GetConf(joyId, mode, idx, out bool ok);
            if (ok)
            {
                bool longRelease = false;
                if (actions.Type == 2)
                {
                    if (shortRelease)
                    {
                        ushort accionId = 0;
                        if (actions.Actions.Length == 4)
                        {
                            accionId = actions.Actions[3];
                        }
                        if (accionId != 0)
                        {
                            CGenerateEvents.Command(pProfile, joyId, accionId, idx, CGenerateEvents.Origin.Button);
                            return;
                        }
                    }
                    else
                    {
                        hMutexStatus.Enter();
                        {
                            ulong id = ((ulong)joyId << 8) | idx;
                            var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(pStButtonsMap);
                            for (int i = 0; i < span.Length; i++)
                            {
                                if (span[idx].Id == id)
                                {
                                    pStButtonsMap.RemoveAt(i);
                                    pProfile.GetStatus().Buttons.SetPos(2, true, joyId, mode, idx);
                                    hMutexStatus.Exit();
                                    PressButton(joyId, idx, true);

                                    TIMER_CTX ctx = new() { Parent  = this, JoyId = joyId, HatButton_Idx = idx};
                                    ctx.Timer = new(EvtShortRelease, ctx, 300, System.Threading.Timeout.Infinite);
                                    lock (hMutexStatus)
                                    {
                                        pStButtonsMap.Add(( (1ul << 48) | ((ulong)joyId << 8) | idx, ctx.Timer));
                                    }

                                    return;
                                }
                            }
                        }
                        hMutexStatus.Exit();
                        longRelease = true;
                    }
                }
                if ((actions.Type == 0) || ((actions.Type == 2) && longRelease))
                {
                    ushort accionId = 0;
                    if (actions.Actions.Length > 1)
                    {
                        accionId = actions.Actions[1];
                    }
                    if (accionId != 0)
                    {
                        CGenerateEvents.Command(pProfile, joyId, accionId, idx, CGenerateEvents.Origin.Button);
                        return;
                    }
                }
            }

            CGenerateEvents.CheckHolds();
        }

        public void PressHat(uint joyId, byte idx, bool longPress)
        {
            ushort actionId = 0;
            bool shortPress = false;

            {
                Profile.CStatus refStatus = pProfile.GetStatus();
                byte mode = (byte)(refStatus.Mode | (refStatus.SubMode << 4));

                byte pos = 0;
                if (refStatus.Hats.GetPos(ref pos, joyId, mode, idx))
                {
                    ref readonly Profile.CProgramming.BUTTONMODEL actions = ref pProfile.GetProfile().HatsMap.GetConf(joyId, mode, idx, out bool ok);
                    if (ok)
                    {
                        actionId = actions.Actions[pos];
                        if (actions.Type == 1)
                        {
                            refStatus.Hats.SetPos(1, false, joyId, mode, idx);
                            if ((byte)(pos + 1) >= actions.Actions.Length)
                            {
                                refStatus.Buttons.SetPos(0, true, joyId, mode, idx);
                            }
                        }
                        else if (actions.Type == 2)
                        {
                            if (pos == 2)
                            {
                                shortPress = true;
                                refStatus.Hats.SetPos(0, true, joyId, mode, idx);
                            }
                            if (!longPress)
                            {
                                TIMER_CTX ctx = new() { Parent = this, JoyId = joyId, HatButton_Idx = (byte)(256 | idx) };
                                ctx.Timer = new(EvtLongPress, ctx, 1200, System.Threading.Timeout.Infinite);
                                lock (hMutexStatus)
                                {
                                    pStHatsMap.Add((((ulong)joyId << 8) | idx, ctx.Timer));
                                }
                                return;
                            }
                        }
                    }
                    refStatus.Hats.SetPressed(1, joyId, idx);
                }
            }

            CGenerateEvents.Command(pProfile, joyId, actionId, idx, shortPress ? CGenerateEvents.Origin.HatShort : CGenerateEvents.Origin.Hat);
        }

        public void ReleaseHat(uint joyId, byte idx, bool shortRelease)
        {
            Profile.CStatus refStatus = pProfile.GetStatus();
            byte mode = (byte)(refStatus.Mode | (refStatus.SubMode << 4));
            refStatus.Hats.SetPressed(0, joyId, idx);

            ref readonly Profile.CProgramming.BUTTONMODEL actions = ref pProfile.GetProfile().HatsMap.GetConf(joyId, mode, idx, out bool ok);
            if (ok)
            {
                bool longRelease = false;
                if (actions.Type == 2)
                {
                    if (shortRelease)
                    {
                        ushort accionId = 0;
                        if (actions.Actions.Length == 4)
                        {
                            accionId = actions.Actions[3];
                        }
                        if (accionId != 0)
                        {
                            CGenerateEvents.Command(pProfile, joyId, accionId, idx, CGenerateEvents.Origin.Hat);
                            return;
                        }
                    }
                    else
                    {
                        hMutexStatus.Enter();
                        {
                            ulong id = ((ulong)joyId << 8) | idx;
                            var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(pStHatsMap);
                            for (int i = 0; i < span.Length; i++)
                            {
                                if (span[idx].Id == id)
                                {
                                    pStHatsMap.RemoveAt(i);
                                    pProfile.GetStatus().Hats.SetPos(2, true, joyId, mode, idx);
                                    hMutexStatus.Exit();
                                    PressButton(joyId, idx, true);

                                    TIMER_CTX ctx = new() { Parent = this, JoyId = joyId, HatButton_Idx = (byte)(256 | idx) };
                                    ctx.Timer = new(EvtShortRelease, ctx, 300, System.Threading.Timeout.Infinite);
                                    lock (hMutexStatus)
                                    {
                                        pStHatsMap.Add(((1ul << 48) | ((ulong)joyId << 8) | idx, ctx.Timer));
                                    }

                                     return;
                                }
                            }
                        }
                        hMutexStatus.Exit();
                        longRelease = true;
                    }
                }
                if ((actions.Type == 0) || ((actions.Type == 2) && longRelease))
                {
                    ushort accionId = 0;
                    if (actions.Actions.Length > 1)
                    {
                        accionId = actions.Actions[1];
                    }
                    if (accionId != 0)
                    {
                        CGenerateEvents.Command(pProfile, joyId, accionId, idx, CGenerateEvents.Origin.Hat);
                        return;
                    }
                }
            }

            CGenerateEvents.CheckHolds();
        }

        private static void EvtLongPress(object? state)
        {
            if (state != null)
            {
                var ctx = (TIMER_CTX)state;
                ulong id = ((ulong)ctx.JoyId << 8) | (byte)(ctx.HatButton_Idx & 0xff);
                bool unlock = true;

                if ((ctx.HatButton_Idx >> 8) > 0) // hat
                {
                    ctx.Parent.hMutexStatus.Enter();
                    var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(ctx.Parent.pStHatsMap);
                    for (int idx = 0; idx < span.Length; idx++)
                    {
                        if (span[idx].Id == id)
                        {
                            ctx.Parent.pStHatsMap.RemoveAt(idx);
                            ctx.Parent.hMutexStatus.Exit();
                            ctx.Parent.PressHat(ctx.JoyId, (byte)(ctx.HatButton_Idx & 0xff), true);
                            unlock = false;
                            break;
                        }
                    }
                    if (unlock)
                    {
                        ctx.Parent.hMutexStatus.Exit();
                    }
                }
                else
                {
                    ctx.Parent.hMutexStatus.Enter();
                    var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(ctx.Parent.pStButtonsMap);
                    for (int idx = 0; idx < span.Length; idx++)
                    {
                        if (span[idx].Id == id)
                        {
                            ctx.Parent.pStButtonsMap.RemoveAt(idx);
                            ctx.Parent.hMutexStatus.Exit();
                            ctx.Parent.PressButton(ctx.JoyId, (byte)(ctx.HatButton_Idx & 0xff), true);
                            unlock = false;
                            break;
                        }
                    }
                    if (unlock)
                    {
                        ctx.Parent.hMutexStatus.Exit();
                    }
                }

                ctx.Timer.Dispose();
            }
        }

        private static void EvtShortRelease(object? state)
        {
            if (state != null)
            {
                var ctx = (TIMER_CTX)state;
                ulong id = (1ul << 48) | ((ulong)ctx.JoyId << 8) | ((ulong)ctx.HatButton_Idx & 0xff);
                bool unlock = true;

                if ((ctx.HatButton_Idx >> 8) > 0) // hat
                {
                    ctx.Parent.hMutexStatus.Enter();
                    var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(ctx.Parent.pStHatsMap);
                    for (int idx = 0; idx < span.Length; idx++)
                    {
                        if (span[idx].Id == id)
                        {
                            ctx.Parent.pStHatsMap.RemoveAt(idx);
                            ctx.Parent.hMutexStatus.Exit();
                            ctx.Parent.ReleaseHat(ctx.JoyId, (byte)(ctx.HatButton_Idx & 0xff), true);
                            unlock = false;
                            break;
                        }
                    }
                    if (unlock)
                    {
                        ctx.Parent.hMutexStatus.Exit();
                    }
                }
                else
                {
                    ctx.Parent.hMutexStatus.Enter();
                    var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(ctx.Parent.pStButtonsMap);
                    for (int idx = 0; idx < span.Length; idx++)
                    {
                        if (span[idx].Id == id)
                        {
                            ctx.Parent.pStButtonsMap.RemoveAt(idx);
                            ctx.Parent.hMutexStatus.Exit();
                            ctx.Parent.ReleaseButton(ctx.JoyId, (byte)(ctx.HatButton_Idx & 0xff), true);
                            unlock = false;
                            break;
                        }
                    }
                    if (unlock)
                    {
                        ctx.Parent.hMutexStatus.Exit();
                    }
                }
                ctx.Timer.Dispose();
            }
        }
    }
}
