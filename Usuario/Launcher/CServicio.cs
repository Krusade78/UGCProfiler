using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Launcher
{
    internal class CServicio : IDisposable
    {
        private Comunes.Calibrado.CCalibrado dsc = new();
        private System.Threading.CancellationTokenSource cerrarPipe = new();
        private System.Threading.CancellationTokenSource cerrarPipeSvc = new();
        private System.IO.BinaryWriter salidaPipeSvc = null;
        private readonly object main = null;

        public event EventHandler<ResolveEventArgs> EvtSalir;
        public enum TipoMsj : byte { ModoRaw, ModoCalibrado, Calibrado, Antiv, Mapa, Comandos };

        public CServicio(object main)
        {
            this.main = main;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cerrarPipe?.Cancel();
                    while (cerrarPipe != null) { System.Threading.Thread.Sleep(100); }
                    dsc = null;
                    cerrarPipeSvc?.Cancel();
                    while (cerrarPipeSvc != null) { System.Threading.Thread.Sleep(100); }
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
             Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion
        public bool Iniciar()
        {
            Task.Run(() =>
                {
                    while (!cerrarPipe.Token.IsCancellationRequested)
                    {
                        using (NamedPipeServerStream pipeServer = new ("LauncherPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough))
                        {
                            try { pipeServer.WaitForConnectionAsync(cerrarPipe.Token).Wait(cerrarPipe.Token); } catch { break; }
                            if (cerrarPipe.Token.IsCancellationRequested)
                                break;
                            using (System.IO.StreamReader r = new System.IO.StreamReader(pipeServer))
                            {
                                MensajeIn(r.ReadToEnd());
                            }
                        }
                    }
                    cerrarPipe.Dispose();
                    cerrarPipe = null;
                });
            Task.Run(() =>
            {
                using (NamedPipeServerStream pipeServerSvc = new("LauncherPipeSvc", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough))
                {
                    //using (System.Diagnostics.Process p = new System.Diagnostics.Process())
                    //{
                    //    p.StartInfo.FileName = "xhotas_svc.exe";
                    //    p.StartInfo.UseShellExecute = true;
                    //    try
                    //    {
                    //        p.Start();
                    //    }
                    //    catch { }
                    //}
                    while (!cerrarPipeSvc.Token.IsCancellationRequested)
                    {
                        try { pipeServerSvc.WaitForConnectionAsync(cerrarPipeSvc.Token).Wait(cerrarPipeSvc.Token); } catch { cerrarPipeSvc.Cancel(); }
                        if (!cerrarPipeSvc.Token.IsCancellationRequested)
                        {
                            try
                            {
                                using (System.IO.StreamReader r = new System.IO.StreamReader(pipeServerSvc))
                                {
                                    using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(pipeServerSvc))
                                    {
                                        while (!cerrarPipeSvc.Token.IsCancellationRequested)
                                        {
                                            if (r.ReadLine() == "OK")
                                            {
                                                salidaPipeSvc = w;
                                                CargarCalibrado();
                                                CargarPerfil("perfilbase.dat");
                                                cerrarPipeSvc.Token.WaitHandle.WaitOne();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                cerrarPipeSvc.Cancel();
                            }
                            if (salidaPipeSvc != null)
                            {
                                salidaPipeSvc.Close();
                                salidaPipeSvc = null;
                            }
                        }
                    }
                }
                cerrarPipeSvc.Dispose();
                cerrarPipeSvc = null;
            }).ContinueWith((ret) => EvtSalir.Invoke(null, null));

            return true;
        }

        private void CargarCalibrado()
        {
            try
            {
                dsc = System.Text.Json.JsonSerializer.Deserialize<Comunes.Calibrado.CCalibrado>(System.IO.File.ReadAllText("configuracion.dat"));
            }
            catch (Exception ex)
            {
                ((CMain)main).MessageBox($"Error al leer calibrado:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Comunes.CTipos.STJITTER[,] jitter = new Comunes.CTipos.STJITTER[4,8];
            Comunes.CTipos.STLIMITES[,] limites = new Comunes.CTipos.STLIMITES[4,8];
            uint[] ids = { 0x06A30763, 0x06a30255, 0x06a30256, 0x231d0200 };
            for (byte j = 0; j < 4; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Comunes.Calibrado.Limites l = dsc.Limites.Find(x => (x.IdJoy == ids[j]) && (x.IdEje == i));
                    if (l != null)
                    {
                        limites[j, i].Cen = l.Cen;
                        limites[j, i].Izq = l.Izq;
                        limites[j, i].Der = l.Der;
                        limites[j, i].Nulo = l.Nulo;
                        limites[j, i].Cal = l.Cal;
                        limites[j, i].Rango = l.Rango;
                    }
                    Comunes.Calibrado.Jitter ji = dsc.Jitters.Find(x => (x.IdJoy == ids[j]) && (x.IdEje == i));
                    if (ji != null)
                    { 
                        jitter[j, i].Margen = ji.Margen;
                        jitter[j, i].Resistencia = ji.Resistencia;
                        jitter[j, i].Antiv = ji.Antiv;
                    }
                }
            }

            if (salidaPipeSvc != null)
            {
                byte[] bufCal = new byte[1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)) * 8 * 4)];
                byte[] bufJit = new byte[1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)) * 8 * 4)];
                bufCal[0] = (byte)TipoMsj.Calibrado;
                bufJit[0] = (byte)TipoMsj.Antiv;
                for (byte j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)));
                        Marshal.StructureToPtr(limites[j, i], ptr, true);
                        Marshal.Copy(ptr, bufCal, 1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)) * ((j * 8) + i)), Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)));
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)));
                        Marshal.StructureToPtr(jitter[j, i], ptr, true);
                        Marshal.Copy(ptr, bufJit, 1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)) * ((j * 8) + i)), Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)));
                        Marshal.FreeHGlobal(ptr);
                    }
                }
                salidaPipeSvc.Write(bufCal, 0, bufCal.Length);
                salidaPipeSvc.Flush();
                salidaPipeSvc.Write(bufJit, 0, bufJit.Length);
                salidaPipeSvc.Flush();
            }
        }

        private void MensajeIn(string msj)
        {
            if (msj.StartsWith("CCAL"))
            {
                CargarCalibrado();
            }
            else if (msj.StartsWith("CAL:"))
            {
                if (salidaPipeSvc != null)
                {
                    byte[] buff = { (byte)TipoMsj.ModoCalibrado, (msj.IndexOf("True") != -1) ? (byte)1 : (byte)0 };
                    salidaPipeSvc.Write(buff, 0, 2);
                    salidaPipeSvc.Flush();
                }
            }
            else if (msj.StartsWith("RAW:"))
            {
                if (salidaPipeSvc != null)
                {
                    byte[] buff = { (byte)TipoMsj.ModoRaw, (msj.IndexOf("True") != -1) ? (byte)1 : (byte)0 };
                    salidaPipeSvc.Write(buff, 0, 2);
                    salidaPipeSvc.Flush();
                }
            }
            else
            {
                CargarPerfil(msj);
            }
        }

        public void CargarPerfil(string archivo)
        {
            byte ret = 0;
            lock (this)
            {
                ret = CPerfil.CargarMapa(main, archivo, salidaPipeSvc);
            }

            if ((ret == 0) && (archivo != "perfilbase.dat"))
            {
                string nombre = System.IO.Path.GetFileNameWithoutExtension(archivo);
                ((CMain)main).MessageBox("Perfil cargado correctamente.", nombre, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
