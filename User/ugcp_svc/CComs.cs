using System;

namespace ugcp_svc
{
    sealed class CComs : IDisposable
    {
        private readonly Profile.CProfile pProfile;
        private enum MsjType : byte { RawMode, CalibrationMode, Calibration, Antiv, Map, Commands };
        private System.IO.Pipes.NamedPipeClientStream? hPipe = null;
        private Action? CloseHwnd = null;
        private System.Threading.Thread? thread = null;
        private readonly System.Threading.CancellationTokenSource ct = new();

        public CComs(Profile.CProfile pProfile)
        {
            this.pProfile = pProfile;
        }

        void IDisposable.Dispose()
        {
            ct.Cancel();
            hPipe?.Close();
            hPipe?.Dispose();
            thread?.Join();
        }

        public void SetHwndClose(Action closeCallback) => CloseHwnd = closeCallback;

        public bool Init()
        {
            for(byte retry = 0; retry < 5; retry++)
            {
                hPipe = new(".", "LauncherPipeSvc", System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.WriteThrough);
                try
                {
                    hPipe.Connect(1000);
                    hPipe.ReadMode = System.IO.Pipes.PipeTransmissionMode.Message;
                    hPipe.Write([1,1]);
                    hPipe.Flush();
                    thread = new(() => ThreadRead(ct.Token))
                    {
                        IsBackground = true
                    };
                    thread.Start();
                    return true;
                }
                catch { }
                hPipe?.Dispose();
                hPipe = null;
                System.Threading.Tasks.Task.Delay(1000).Wait();
            }

            return false;
        }

        private async void ThreadRead(System.Threading.CancellationToken token)
        {
            int retries = 5;
            byte[] buffer = new byte[1024];

            while (!token.IsCancellationRequested && retries >= 0)
            {
                try
                {
                    using System.IO.MemoryStream ms = new();
                    do
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        int read = await hPipe.ReadAsync(buffer, token);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        if (read <= 0) { break; }
                        ms.Write(buffer, 0, read);

                    } while (!hPipe.IsMessageComplete);

                    if (ms.Length > 0)
                    {
                        ProcessMessage(System.Text.Encoding.UTF8.GetString(ms.ToArray()));
                        retries = 5;
                    }
                    else
                    {
                        retries--;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    retries--;
                }
            }

            CloseHwnd?.Invoke();
        }

        private bool ProcessMessage(ReadOnlySpan<char> msg)
        {
            switch ((MsjType)(byte)msg[0])
            {
                case MsjType.RawMode:
                    pProfile.RawMode = msg[1] == 1;
                    break;

                //case MsjType.CalibrationMode:
                //    pProfile.CalibrationMode = msg[1] == 1;
                //    break;

                case MsjType.Calibration:
                    pProfile.WriteCalibration(msg[1..]);
                    break;

                case MsjType.Antiv:
                    pProfile.WriteAntivibration(msg[1..]);
                    break;

                case MsjType.Map:
                    return pProfile.WriteProfile(msg[1..]);

                //case MsjType.Commands:
                //    return pProfile.HF_IoWriteCommands(msg[1..]);

                default:
                    break;
            }

            return true;
        }
    }
}
