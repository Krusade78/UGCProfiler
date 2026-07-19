using System;
using ugcp_svc.EventQueue;

namespace ugcp_svc.ProcessOutput.Commands
{
    static class CX52
    {
        /// <summary>
        /// Comandos del X52
        /// </summary>
        /// <returns><para>TRUE: procesado y continuar</para><para>FALSE: no procesado</para></returns>
        public static bool Process(CEventPacket queue)
        {
            bool processed = true;
            ref EV_COMMAND command = ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(queue)[0];

            switch (command.Type.Get())
            {
                case CommandType.X52MfdLight:
                    {
                        byte _params = command.Data.Basic.Data1;
                        X52.CX52Write.Get()?.Light_MFD(_params);
                        break;
                    }
                case CommandType.X52Light:
                    {
                        byte _params = command.Data.Basic.Data1;
                        X52.CX52Write.Get()?.Light_Global(_params);
                        break;
                    }
                case CommandType.X52InfoLight:
                    {
                        byte _params = command.Data.Basic.Data1;
                        X52.CX52Write.Get()?.Light_Info(_params);
                        break;
                    }
                case CommandType.X52MfdPinkie:
                    {
                        byte _params = command.Data.Basic.Data1;
                        X52.CX52Write.Get()?.Set_Pinkie(_params);
                        break;
                    }
                case CommandType.X52MfdTextIni:
                    {
                        Span<byte> text = new byte[17];
                        byte size = 1;

                        text[0] = command.Data.Basic.Data1; //line
                        while (queue.Count != 1)
                        {
                            if (queue[1].Type == CommandType.X52MfdTextEnd)
                            {
                                queue.RemoveAt(1);
                                break;
                            }
                            if (size == 17)
                            {
                                throw new Exception("Error text buffer");
                            }
                            text[size++] = queue[1].Data.Basic.Data1;
                            queue.RemoveAt(1);
                        }
                        X52.CX52Write.Get()?.Set_Text(text.Slice(0, size));
                        break;
                    }
                case CommandType.X52MfdHour:
                    {
                        Span<byte> _params = new byte[3];
                        _params[0] = command.Data.Basic.Data1;

                        _params[1] = queue[1].Data.Basic.Data1;
                        queue.RemoveAt(1);

                        _params[2] = queue[1].Data.Basic.Data1;
                        queue.RemoveAt(1);

                        X52.CX52Write.Get()?.Set_Hour(_params);
                        break;
                    }
                case CommandType.X52MfdHour24:
                    {
                        Span<byte> _params = new byte[3];
                        _params[0] = command.Data.Basic.Data1;

                        _params[1] = queue[1].Data.Basic.Data1;
                        queue.RemoveAt(1);

                        _params[2] = queue[1].Data.Basic.Data1;
                        queue.RemoveAt(1);

                        X52.CX52Write.Get()?.Set_Hour24(_params);
                        break;
                    }
                case CommandType.MfdDate:
                    {
                        Span<byte> _params = new byte[2];
                        _params[0] = command.Data.Basic.Data1;

                        _params[1] = queue[1].Data.Basic.Data1;
                        queue.RemoveAt(1);

                        X52.CX52Write.Get()?.Set_Date(_params);
                        break;
                    }
                default:
                    processed = false;
                    break;
            }

            return processed;
        }

    }
}
