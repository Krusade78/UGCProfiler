using ugcp_svc.EventQueue;

namespace ugcp_svc.ProcessInput.GenerateEvents
{
    static class CGenerateEvents
    {
        public enum Origin : byte
        {
            Button = 0,
            Hat,
            Axis,
            ButtonShort,
            HatShort,
        }

        public static void Mouse(ref EV_COMMAND pev_command)
        {
            CEventPacket pEvento = [];
            EV_COMMAND command = pev_command;
            pEvento.Add(command);
            ProcessOutput.CProcessOutput.Get()?.AddEvent(pEvento);
        }

        public static void Command(Profile.CProfile pProfile, uint idJoy, ushort actionId, byte origin, Origin originType)
        {
            EV_COMMAND dummy = default;
            Command(pProfile, idJoy, actionId, origin, originType, ref dummy);
        }

        public static void Command(Profile.CProfile pProfile, uint idJoy, ushort actionId, byte origin, Origin originType, ref EV_COMMAND refAxisData)
        {
            if ((originType == Origin.Hat) || (originType == Origin.HatShort) || (originType == Origin.Axis))
            {
                origin += 128;
            }

            if (actionId != 0)
            {
                CEventPacket pEvent = [];
                {
                    System.ReadOnlySpan<EV_COMMAND[]> actions = pProfile.GetProfile().ReadActions();
                    if (actions.Length == 0)
                    {
                        return;
                    }
                    else
                    {
                        System.ReadOnlySpan<EV_COMMAND> commands = actions[actionId - 1];
                        for (int idx = 0; idx < commands.Length; idx++)
                        {
                            EV_COMMAND pEvt = commands[idx];
                            if ((pEvt.Type == CommandType.Hold) && ((originType == Origin.ButtonShort) || (originType == Origin.HatShort)))
                            {
                                pEvt.Type = CommandType.Delay;
                                pEvt.Data.Basic.Data1 = 2;
                            }
                            if ((pEvt.Type == CommandType.Hold) || ((pEvt.Type & 0x7f) == CommandType.Repeat))
                            {
                                pEvt.Data.Extended.Origin = origin;
                                pEvt.Data.Extended.InputJoy = idJoy;
                                if (originType == Origin.Axis)
                                {
                                    pEvt.Data.Extended.Submode = refAxisData.Data.Extended.Submode;
                                    pEvt.Data.Extended.Mode = refAxisData.Data.Extended.Mode;
                                    pEvt.Data.Extended.Incremental = refAxisData.Data.Extended.Incremental;
                                    pEvt.Data.Extended.Band = refAxisData.Data.Extended.Band;
                                }
                                else if ((originType == Origin.Hat) || (originType == Origin.HatShort))
                                {
                                    pEvt.Data.Extended.Mode = 255; //Hat flag
                                }
                            }
                            pEvent.Add(pEvt);
                            idx++;
                        }
                    }
                }

                ProcessOutput.CProcessOutput.Get()?.AddEvent(pEvent);
            }
        }

        public static void DirectX(byte vJoyId, byte map, ref ProcessOutput.VHID_INPUT_DATA inputData)
        {
            CEventPacket pEvent = [];
            EV_COMMAND command = new()
            {
                Type = CommandType.Reserved_DxPosition
            };
            command.Data.VHid.OutputJoyId = vJoyId;
            command.Data.VHid.Map = map;
            command.Data.VHid.Data = inputData;

            pEvent.Add(command);
            ProcessOutput.CProcessOutput.Get()?.AddEvent(pEvent);
        }

        public static void CheckHolds()
        {
            CEventPacket pEvent = [];
            EV_COMMAND command = new()
            {
                Type = CommandType.Reserved_CheckHold
            };
            pEvent.Add(command);
            ProcessOutput.CProcessOutput.Get()?.AddEvent(pEvent);
        }

    }
}
