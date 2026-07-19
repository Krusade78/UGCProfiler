using System;
using System.Collections.Generic;
using System.Text;
using ugcp_svc.EventQueue;

namespace ugcp_svc.ProcessOutput.Commands
{
    static class COther
    {
        /// <summary>
        /// Hold, Delay and Repeats
        /// </summary>
        /// <returns><para>false: delete command[0] normal</para>
        /// <para>true: reprocess/skip action</para>
        /// </returns>
        public static void Process(Profile.CProfile pProfile, ref int posEvent, CEventPacket refCommandQueue, CProcessOutput refParent)
        {
            CEventPacket commandQueue = refCommandQueue;
            CProcessOutput local = refParent;
            ref EV_COMMAND command = ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(commandQueue)[0];

            if (command.Type == CommandType.Delay) // Delay
            {
                CProcessOutput.TIMER_CTX ctx = new()
                {
                    Parent = local,
                    Queue = commandQueue
                };

                commandQueue.RemoveAt(0);

                local.AddDelayTimer(ctx); //inside eventMutex
                ctx.TimerDelay = new(CProcessOutput.ProcessDelay, ctx, 100 * command.Data.Basic.Data1, System.Threading.Timeout.Infinite);
                refParent.RemoveEvent(posEvent);
            }
            else if (command.Type == CommandType.Hold) // Autorepeat hold
            {
                if (!IsHoldOn(pProfile, ref command))
                {
                    commandQueue.RemoveAt(0);
                }
                else
                {
                    posEvent++; //go to next action
                }
            }
            else if (command.Type == CommandType.Repeat)
            {
                if (!IsHoldOn(pProfile, ref command)) // end infinite autorepeat
                {
                    DeleteRepeatBlock(commandQueue, CommandType.Repeat);
                }
                else
                {
                    CopyQueueWithRepeat(commandQueue, CommandType.Repeat);
                }
            }
            else if (command.Type == CommandType.RepeatN)
            {
                if (command.Data.Basic.Data1 == 0)
                {
                    DeleteRepeatBlock(commandQueue, CommandType.RepeatN);
                }
                else
                {
                    command.Data.Basic.Data1--;
                    CopyQueueWithRepeat(commandQueue, CommandType.RepeatN);
                }
            }
        }

        private static bool IsHoldOn(Profile.CProfile pProfile, ref EV_COMMAND command)
        {
            bool pressed = false;

            if ((command.Data.Extended.Origin & 128) == 128)  //axis or hat
            {
                if (command.Data.Extended.Mode == 255) //hat
                {
                    byte hatPressed = 0;
                    if (pProfile.GetStatus().Hats.GetPressed(ref hatPressed, command.Data.Extended.InputJoy, (byte)(command.Data.Extended.Origin & 127)))
                    {
                        pressed = hatPressed == 1;
                    }
                }
                else //axis
                {
                    Profile.CStatus refStatus = pProfile.GetStatus();
                    byte mode = refStatus.Mode;
                    byte submode = refStatus.SubMode;
                    byte cmode = (byte)(mode | (byte)(submode << 4));
                    ref Profile.CStatus.ST_AXIS stAxis = ref refStatus.Axes.GetStatus(command.Data.Extended.InputJoy, cmode, (byte)(command.Data.Extended.Origin & 127), out bool ok);
                    if (ok)
                    {
                        pressed = !((command.Data.Extended.Mode == mode) && (command.Data.Extended.Submode == submode) && ((command.Data.Extended.Incremental != stAxis.IncrementalPos) || (command.Data.Extended.Band != stAxis.Band)));
                    }
                }
            }
            else //button
            {
                byte btPressed = 0;
                if (pProfile.GetStatus().Buttons.GetPressed(ref btPressed, command.Data.Extended.InputJoy, command.Data.Extended.Origin))
                {
                    pressed = btPressed == 1;
                }
            }

            return pressed;
        }

        private static void DeleteRepeatBlock(CEventPacket refQueue, byte commandType)
        {
            short pos = 0;
            pos++;
            byte nested = (byte)((commandType == CommandType.RepeatN) ? 1 : 0);

            ReadOnlySpan<EV_COMMAND> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(refQueue);
            while (pos < span.Length)
            {
                ref readonly EV_COMMAND end = ref span[pos];
                if ((end.Type & 0x7f) == CommandType.RepeatN)
                {
                    if ((end.Type & CommandType.Release) == CommandType.Release)
                        nested--;
                    else
                        nested++;
                }
                if (((end.Type & 0x7f) == commandType) && ((end.Type & CommandType.Release) == CommandType.Release) && (nested == 0))
                {
                    refQueue.RemoveAt(pos);
                    break;
                }
                pos++;
            }

            refQueue.RemoveAt(0);
        }

        private static void CopyQueueWithRepeat(CEventPacket refQueue, byte commandType)
        {
            ushort posEnd = 0;
            ushort posCopy = 0;
            posCopy++;
            byte nested = (byte)((commandType == CommandType.RepeatN) ? 1 : 0);

            ReadOnlySpan<EV_COMMAND> span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(refQueue);
            while (posCopy != span.Length)
            {
                ref readonly EV_COMMAND comOrigin = ref span[posCopy];
                if ((comOrigin.Type & 0x7f) == CommandType.RepeatN)
                {
                    if ((comOrigin.Type & CommandType.Release) == CommandType.Release)
                        nested--;
                    else
                        nested++;
                }
                if (((comOrigin.Type & 0x7f) == commandType) && ((comOrigin.Type & CommandType.Release) == CommandType.Release) && (nested == 0))
                {
                    break;
                }
                else
                {
                    posEnd = posCopy;
                }
                posCopy++;
            }

            for (ushort idx = posEnd; idx != 0; idx--)
            {
                refQueue.Insert(0, span[idx]);
            }
        }
    }
}
