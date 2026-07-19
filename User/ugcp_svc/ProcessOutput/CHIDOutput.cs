using System;

namespace ugcp_svc.ProcessOutput
{
    internal class CHIDOutput : IDisposable
    {
        private readonly Profile.CProfile profile;
        private readonly CVirtualHID vhid;
        private readonly CProcessOutput output;
        private System.Threading.Thread? thread = null;
        private readonly System.Threading.CancellationTokenSource ct = new();

        public CHIDOutput(Profile.CProfile refProfile, CVirtualHID refVhid)
        {
            profile = refProfile;
            vhid = refVhid;
            output = new(profile, vhid);
        }

        void IDisposable.Dispose()
        {
            ct.Cancel();
            thread?.Join();
            ((IDisposable?)output)?.Dispose();
        }

        public bool Init()
        {
            try
            {
                thread = new(() => ThreadRead(ct.Token))
                {
                    Priority = System.Threading.ThreadPriority.Highest
                };
                thread.Start();
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void ThreadRead(System.Threading.CancellationToken token)
        {
            //System.Threading.WaitHandle[] events = [ct.Token.WaitHandle, evQueue.GetEvQueue().WaitHandle, output.GetEvQueue().WaitHandle];

            //while (true)
            //{
            //    int idx = System.Threading.WaitHandle.WaitAny(events);
            //    if (idx != 0)
            //    {
            //        EventQueue.CEventPacket? paq = null;
            //        if (idx == 1)
            //        {
            //            paq = evQueue.Read();
            //        }
            //        output.Process(paq);
            //        return;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}
        }
    }
}
