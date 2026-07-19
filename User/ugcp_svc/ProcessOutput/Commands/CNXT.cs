using System;
using System.Collections.Generic;
using System.Text;

namespace ugcp_svc.ProcessOutput.Commands
{
    static class CNXT
    {
        /// <summary>
        /// Gladiator NXT commands
        /// </summary>
        /// <returns><para>TRUE: processed and continue</para><para>FALSE: not processed</para></returns>
        public static bool Process(EventQueue.CEventPacket queue)
        {
            bool processed = true;

            ref EventQueue.EV_COMMAND command = ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(queue)[0];
            if (command.Type.Get() == EventQueue.CommandType.NxtLeds)
            {
                byte[] param = [0, 0, 0, 0];
                param[0] = command.Data.Basic.Data1;

                param[1] = queue[1].Data.Basic.Data1;
                queue.RemoveAt(1);

                param[2] = queue[1].Data.Basic.Data1;
                queue.RemoveAt(1);

                param[3] = queue[1].Data.Basic.Data1;
                queue.RemoveAt(1);

                NXT.CNXTWrite.Get()?.SetLed(param);
            }
            else
            {
                processed = false;
            }

            return processed;
        }
    }
}
