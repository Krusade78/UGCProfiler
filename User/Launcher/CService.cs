using System;
using System.Windows;
using System.IO.Pipes;
using System.Threading.Tasks;


namespace Launcher
{
    internal class CService(object main) : IDisposable
    {
        private readonly System.Threading.CancellationTokenSource closePipe = new();
        private Task? tPipe = null;
        private readonly System.Threading.CancellationTokenSource closePipeSvc = new();
        private Task? tPipeSvc = null;
        private NamedPipeServerStream? pipeServerSvc = null;
        private readonly object main = main;

        public event EventHandler? ExitEvt;
        public enum MsgType : byte { RawMode, CalibrationMode, Calibration, Antivibration, Map, Macros };

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
                if (disposing)
                {
                    closePipe?.Cancel();
                    tPipe?.Wait();
                    closePipe?.Dispose();
                    closePipeSvc?.Cancel();
                    tPipeSvc?.Wait();
                    closePipeSvc?.Dispose();
                }
            }
        }
        public void Dispose()
        {
             Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion

        public bool Init()
        {
            tPipe = Task.Run(async() =>
                {
                    byte[] buff = new byte[270 * sizeof(char)];
                    using NamedPipeServerStream pipeServer = new("LauncherPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough);
                    try { pipeServer.WaitForConnectionAsync(closePipe.Token).Wait(closePipe.Token); } catch { closePipeSvc.Cancel(); return; }
                    while (!closePipe.Token.IsCancellationRequested)
                    {
                        try
                        {
                            int size = await pipeServer.ReadAsync(buff.AsMemory(), closePipe.Token);
                            MessageIn(System.Text.Encoding.UTF8.GetString(buff, 0, size));
                        }
                        catch { break; }
                    }
                });
            tPipeSvc = Task.Run(async() =>
            {
                using NamedPipeServerStream tPipeServerSvc = new("LauncherPipeSvc", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough);
                while (!closePipeSvc.Token.IsCancellationRequested)
                {
                    try { tPipeServerSvc.WaitForConnectionAsync(closePipeSvc.Token).Wait(closePipeSvc.Token); } catch { closePipeSvc.Cancel(); }
                    if (!closePipeSvc.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (!closePipeSvc.Token.IsCancellationRequested)
                            {
                                byte[] r = new byte[2];
                                int size = await tPipeServerSvc.ReadAsync(r.AsMemory(0, 2), closePipeSvc.Token);
                                if ((size == 2) && (r[0] == 1) && (r[1] == 1))
                                {
                                    pipeServerSvc = tPipeServerSvc;
                                    lock (this)
                                    {
                                        LoadCalibration();
                                    }
                                    closePipeSvc.Token.WaitHandle.WaitOne();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            closePipeSvc.Cancel();
                        }
                        pipeServerSvc = null;
                    }
                }
            }).ContinueWith((ret) => ExitEvt?.Invoke(null, new()));

            return true;
        }

        private void LoadCalibration()
        {
            if (pipeServerSvc != null)
            {
                CCalibration.Load(pipeServerSvc);
            }
        }

        private void MessageIn(string msj)
        {
            if (msj.StartsWith("CCAL"))
            {
                LoadCalibration();
            }
            //else if (msj.StartsWith("CAL:"))
            //{
            //    if (pipeServerSvc != null)
            //    {
            //        byte[] buff = [(byte)MsgType.CalibrationMode, (byte)(msj.Contains("True") ? 1 : 0)];
            //        pipeServerSvc.Write(buff, 0, 2);
            //        pipeServerSvc.Flush();
            //    }
            //}
            else if (msj.StartsWith("RAW:"))
            {
                if (pipeServerSvc != null)
                {
                    byte[] buff = [(byte)MsgType.RawMode, (byte)(msj.Contains("True") ? 1 : 0)];
                    pipeServerSvc.Write(buff, 0, 2);
                    pipeServerSvc.Flush();
                }
            }
            else if (msj.StartsWith("DEF:"))
            {
                LoadProfile(null);
            }
            else
            {
                LoadProfile(msj);
            }
        }

        public void LoadProfile(string? file)
        {
            bool ret = false;
            lock (this)
            {
                if (pipeServerSvc != null)
                {
                    ret = new CProfile(main).Load(file, pipeServerSvc);
                }
            }

            if (ret && (file != null))
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                ((CMain)main).MessageBox(CTranslate.Get("profile loaded ok"), name, MessageBoxImage.Information);
            }
        }
    }
}
