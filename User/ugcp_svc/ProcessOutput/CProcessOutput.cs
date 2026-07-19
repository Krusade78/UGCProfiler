using System;
using System.Threading;
using ugcp_svc.EventQueue;

namespace ugcp_svc.ProcessOutput
{
    sealed class CProcessOutput : IDisposable
    {
        public class TIMER_CTX
        {
            public Timer? TimerDelay;
            public required CProcessOutput Parent;
            public CEventPacket? Queue;
        }
        private static CProcessOutput? pInstance = null;

        private readonly Profile.CProfile pProfile;
        private readonly CVirtualHID pVhid;
        private readonly System.Threading.Channels.Channel<CEventPacket> queue;
        private readonly System.Collections.Generic.List<CEventPacket> eventsQueue = [];
        private readonly CancellationTokenSource cts = new();
        private readonly Thread readThread;
        private readonly Lock hWaitLockEvents = new();
        private readonly ManualResetEventSlim hEvEmptyQueue_OnlyHolds = new(false);
        private readonly System.Collections.Generic.List<TIMER_CTX> timersDelayList = [];
        //Mouse tick
        private readonly Timer hMouseTimer;
        private readonly ManualResetEventSlim mouseStopped = new(true);
        private volatile byte mouseStop = 0;

        public CProcessOutput(Profile.CProfile refProfile, CVirtualHID refVhid)
        {
            pProfile = refProfile;
            pVhid = refVhid;
            queue = System.Threading.Channels.Channel.CreateUnbounded<CEventPacket>(new()
            {
                SingleWriter = false,
                SingleReader = true
            });
            readThread = new(() => ThreadRead(cts.Token))
            {
                Priority = ThreadPriority.Highest
            };
            readThread.Start();
            hMouseTimer = new(EvtMouseTick, this, Timeout.Infinite, Timeout.Infinite);
            pInstance = this;

        }

        void IDisposable.Dispose()
        {
            Interlocked.Exchange(ref pInstance, null);
            queue.Writer.Complete();
            cts.Cancel();
            readThread.Join();
            ClearEvents();
            hMouseTimer.Dispose();
        }

        public void ClearEvents()
        {
            lock (hWaitLockEvents)
            {
                Interlocked.Exchange(ref mouseStop, 1);
                mouseStopped.Wait();
                Interlocked.Exchange(ref mouseStop, 0);
                eventsQueue.Clear();
                hEvEmptyQueue_OnlyHolds.Reset();

                while (timersDelayList.Count != 0)
                {
                    TIMER_CTX timerCtx = timersDelayList[0];
                    timerCtx.Queue = null;
                    timersDelayList.RemoveAt(0);
                }

                pVhid.GetStatus().Reset(); //lock is not required here
            }
        }

        public static CProcessOutput? Get() => Interlocked.CompareExchange(ref pInstance, null, null);

        public void AddDelayTimer(TIMER_CTX ctx)
        {
            lock (hWaitLockEvents)
            {
                timersDelayList.Add(ctx);
            }
        }


        #region Event Queue
        public async void AddEvent(CEventPacket packet)
        {
            await queue.Writer.WaitToWriteAsync();
            queue.Writer.TryWrite(packet);
        }

        /// <summary>
        /// WARNING: Must be protected before call
        /// </summary>
        public void RemoveEvent(int position)
        {
            eventsQueue.RemoveAt(position);
        }

        private async void ThreadRead(CancellationToken exit)
        {
            while (!exit.IsCancellationRequested)
            {
                try
                {
                    await queue.Reader.WaitToReadAsync(exit);
                    if (queue.Reader.TryRead(out CEventPacket? buff))
                    {
                        Process(buff);
                    }

                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private void Process(CEventPacket? packet)
        {
            if (packet != null)
            {
                lock (hWaitLockEvents)
                {
                    eventsQueue.Add(packet);
                    hEvEmptyQueue_OnlyHolds.Set();
                }
            }
            ProcessRequest();
        }
        #endregion

        private void ProcessRequest()
        {
            bool empty;
            lock (hWaitLockEvents)
            {
                empty = eventsQueue.Count == 0;
                if (!empty)
                {
                    bool onlyHolds = true;
                    bool cmds = false, ncmds = false;
                    int posEvent = 0;

                    while (posEvent < eventsQueue.Count)
                    {
                        if (cmds && ncmds)
                        {
                            break;
                        }
                        CEventPacket commandQueue = eventsQueue[posEvent];
                        bool deleted = false;
                        Span<EV_COMMAND> commands = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(commandQueue);
                        ref EV_COMMAND command = ref commands[0];
                        if (command.Type != CommandType.Hold)
                        {
                            onlyHolds = false;
                        }

                        if ((command.Type == CommandType.Delay) || (command.Type == CommandType.Hold) || ((command.Type & 0x7f) == CommandType.Repeat) || ((command.Type & 0x7f) == CommandType.RepeatN))
                        {
                            if (!cmds || !ncmds)
                            {
                                Commands.COther.Process(pProfile, ref posEvent, commandQueue, this);
                                if (commandQueue.Count == 0)
                                {
                                    eventsQueue.RemoveAt(posEvent);
                                }
                                continue;
                            }
                        }
                        else if (!ncmds)
                        {
                            if (command.Type == CommandType.Reserved_DxPosition)
                            {
                                ncmds = true;
                                Commands.CDirectX.Position(ref command, pVhid);
                                deleted = true;
                            }
                            if (command.Type == CommandType.Reserved_CheckHold)
                            {
                                deleted = true;
                            }
                            else if ((command.Type & 0x7f) == CommandType.DxButton)
                            {
                                ncmds = true;
                                Commands.CDirectX.Buttons_Hats(ref command, pVhid);
                                deleted = true;
                            }
                            else if ((command.Type & 0x7f) == CommandType.DxHat)
                            {
                                ncmds = true;
                                Commands.CDirectX.Buttons_Hats(ref command, pVhid);
                                deleted = true;
                            }
                            else if ((command.Type & 0x7f) == CommandType.DxAxis)
                            {
                                ncmds = true;
                                Commands.CDirectX.Axis(ref command, pVhid);
                                deleted = true;
                            }
                            else if ((command.Type & 0x7f) == CommandType.Key)
                            {
                                ncmds = true;
                                Commands.CKeyboard.Process(ref command);
                                deleted = true;
                            }
                            else if ((command.Type & 0x7f) == CommandType.PreciseMode)
                            {
                                //pProfile.GetStatus().LockStatus();
                                //{
                                //    if ((command.Type & 0x80) == CommandType.Release)
                                //    {
                                //        pProfile.GetStatus().AxisPreciseMode.SetStatus(0, command.Data.AxisPrecise.InputJoy, command.Data.AxisPrecise.Axis);
                                //    }
                                //    else
                                //    {
                                //        pProfile.GetStatus().AxisPreciseMode.SetStatus(1, command.Data.AxisPrecise.InputJoy, command.Data.AxisPrecise.Axis);
                                //    }
                                //}
                                //pProfile.GetStatus().UnlockStatus();
                                deleted = true;
                            }
                            else if (command.Type == CommandType.Mode)
                            {
                                pProfile.GetStatus().Mode = command.Data.Basic.Data1;
                                deleted = true;
                            }
                            else if (command.Type == CommandType.Submode)
                            {
                                pProfile.GetStatus().SubMode = command.Data.Basic.Data1;
                                deleted = true;
                            }
                            else if (Commands.CX52.Process(commandQueue))
                            {
                                ncmds = true;
                                deleted = true;
                            }
                            else if (Commands.CNXT.Process(commandQueue))
                            {
                                ncmds = true;
                                deleted = true;
                            }
                            else
                            {
                                bool setTimer = false;
                                if (Commands.CMouse.Process(pVhid, ref command, ref setTimer))
                                {
                                    if (setTimer)
                                    {
                                        byte tick = pProfile.GetProfile().MouseTick;
                                        mouseStopped.Reset();
                                        hMouseTimer.Change(tick, Timeout.Infinite);
                                    }
                                    else
                                    {
                                        hMouseTimer.Change(Timeout.Infinite, Timeout.Infinite);
                                    }
                                    ncmds = true;
                                    deleted = true;
                                }
                            }
                        }

                        if (deleted)
                        {
                            commandQueue.RemoveAt(0);
                        }

                        if (commandQueue.Count == 0)
                        {
                            eventsQueue.RemoveAt(posEvent);
                            continue;
                        }
                        posEvent++;
                    }

                    empty = onlyHolds;
                }
            }

            if (empty)
            {
                hEvEmptyQueue_OnlyHolds.Reset();
            }
        }

        private static void EvtMouseTick(object? state)
        {
            if (state == null)
            {
                return;
            }

            CProcessOutput local = (CProcessOutput)state;
            bool send1 = false, send2 = false;
            EV_COMMAND command1 = new(), command2 = new();

            lock (Commands.CMouse.GetLock())
            {
                if (local.pVhid.GetStatus().Mouse.X != 0)
                {
                    if (local.pVhid.GetStatus().Mouse.X < 0)
                    {
                        command1.Type = CommandType.MouseLeft;
                        command1.Data.Basic.Data1 = (byte)-local.pVhid.GetStatus().Mouse.X;
                    }
                    else
                    {
                        command1.Type = CommandType.MouseRight;
                        command1.Data.Basic.Data1 = (byte)local.pVhid.GetStatus().Mouse.X;
                    }
                    send1 = true;
                }
            }

            lock (Commands.CMouse.GetLock())
            {
                if (local.pVhid.GetStatus().Mouse.Y != 0)
                {
                    if (local.pVhid.GetStatus().Mouse.Y < 0)
                    {
                        command2.Type = CommandType.MouseUp;
                        command2.Data.Basic.Data1 = (byte)-local.pVhid.GetStatus().Mouse.Y;
                    }
                    else
                    {
                        command2.Type = CommandType.MouseDown;
                        command2.Data.Basic.Data1 = (byte)local.pVhid.GetStatus().Mouse.Y;
                    }
                    send2 = true;
                }
            }

            if (Interlocked.CompareExchange(ref local.mouseStop, 0, 0) == 0)
            {
                local.mouseStopped.Set();
                if (send1)
                {
                    ProcessInput.GenerateEvents.CGenerateEvents.Mouse(ref command1);
                }
                if (send2)
                {
                    ProcessInput.GenerateEvents.CGenerateEvents.Mouse(ref command2);
                }
            }
            else
            {
                local.mouseStopped.Set();
            }
        }

        public static void ProcessDelay(object? state)
        {
            TIMER_CTX? ctx = (TIMER_CTX?)state;
            if (ctx == null) return;

            lock (ctx.Parent.hWaitLockEvents)
            {
                if (ctx.Queue != null)
                {
                    ctx.Parent.AddEvent(ctx.Queue);
                    ctx.Queue = null;

                    ctx.Parent.hEvEmptyQueue_OnlyHolds.Set();

                    Span<TIMER_CTX> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(ctx.Parent.timersDelayList);
                    for (int pos = 0; pos < span.Length; pos++)
                    {
                        if (span[pos] == ctx)
                        {
                            ctx.Parent.timersDelayList.RemoveAt(pos);
                            break;
                        }
                    }
                }
            }

            ctx.TimerDelay?.Dispose();
        }
    }
}
