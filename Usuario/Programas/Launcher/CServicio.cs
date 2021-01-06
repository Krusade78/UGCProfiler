using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Launcher
{
    class CServicio : IDisposable
    {
        private Comunes.DataSetConfiguracion dsc = new Comunes.DataSetConfiguracion();
        private System.Threading.CancellationTokenSource cerrarPipe = new System.Threading.CancellationTokenSource();

        public CServicio()
        {
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cerrarPipe.Cancel();
                    while (cerrarPipe != null) { System.Threading.Thread.Sleep(100); }
                    dsc.Dispose(); dsc = null;
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
            SetTextoInicio();

            Task.Run(() =>
                {
                    while (!cerrarPipe.Token.IsCancellationRequested)
                    {
                        using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("LauncherPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None))
                        {
                            try { pipeServer.WaitForConnectionAsync(cerrarPipe.Token).Wait(cerrarPipe.Token); } catch { break; }
                            if (cerrarPipe.Token.IsCancellationRequested)
                                break;
                            using (System.IO.StreamReader r = new System.IO.StreamReader(pipeServer))
                            {
                                CargarPerfil(r.ReadToEnd());
                            }
                        }
                    }
                    cerrarPipe.Dispose();
                    cerrarPipe = null;
                });

            return true;
        }

        private void CargarCalibrado()
        {
            dsc.Clear();
            try
            {
                dsc.ReadXml("configuracion.dat");
            }
            catch
            {
                return;
            }

            Comunes.CTipos.STJITTER[] jitter = new Comunes.CTipos.STJITTER[4];
            Comunes.CTipos.STLIMITES[] limites = new Comunes.CTipos.STLIMITES[4];
            for (int i = 0; i < 4; i++)
            {
                limites[i].Cal = 0;
                limites[i].Cen = 1024;
                limites[i].Izq = 0;
                limites[i].Der = 2048;
                jitter[i].Antiv = 0;
                jitter[i].Margen = 0;
                jitter[i].Resistencia = 0;
            }
            if ((dsc.CALIBRADO_LIMITES.Count == 4) && (dsc.CALIBRADO_JITTER.Count == 4))
            {
                for (int i = 0; i < 4; i++)
                {
                    limites[i].Cen = dsc.CALIBRADO_LIMITES[i].Cen;
                    limites[i].Izq = dsc.CALIBRADO_LIMITES[i].Izq;
                    limites[i].Der = dsc.CALIBRADO_LIMITES[i].Der;
                    limites[i].Nulo = dsc.CALIBRADO_LIMITES[i].Nulo;
                    limites[i].Cal = dsc.CALIBRADO_LIMITES[i].Cal;
                    jitter[i].Margen = dsc.CALIBRADO_JITTER[i].Margen;
                    jitter[i].Resistencia = dsc.CALIBRADO_JITTER[i].Resistencia;
                    jitter[i].Antiv = dsc.CALIBRADO_JITTER[i].Antiv;
                }

                if (Comunes.CIoCtl.AbrirDriver())
                {
                    byte[] bufCal = new byte[Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)) * 4];
                    byte[] bufJit = new byte[Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)) * 4];
                    for (int i = 0; i < 4; i++)
                    {
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)));
                        Marshal.StructureToPtr(limites[i], ptr, true);
                        Marshal.Copy(ptr, bufCal, Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)) * i, Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)));
                        Marshal.FreeHGlobal(ptr);
                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)));
                        Marshal.StructureToPtr(jitter[i], ptr, true);
                        Marshal.Copy(ptr, bufJit, Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)) * i, Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)));
                        Marshal.FreeHGlobal(ptr);
                    }
                    if (Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_USR_CALIBRADO, bufCal, (uint)bufCal.Length, null, 0, out _, IntPtr.Zero))
                    {
                        Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_USR_ANTIVIBRACION, bufJit, (uint)bufJit.Length, null, 0, out _, IntPtr.Zero);
                    }

                    Comunes.CIoCtl.CerrarDriver();
                }
            }
        }

        private bool SetTextoInicio()
        {
            String[] filas = new String[] { "  Saitek X-52",
                                            "  Driver v8.0",
                                            "\0"};

            if (!Comunes.CIoCtl.AbrirDriver())
                return false;

            for (byte i = 0; i < 3; i++)
            {
                String fila = filas[i];
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(fila));
                byte[] buffer = new byte[17];
                for (byte c = 0; c < texto.Length; c++)
                    buffer[c + 1] = texto[c];

                buffer[0] = (byte)(i + 1);
                if (!Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out _, IntPtr.Zero))
                {
                    Comunes.CIoCtl.CerrarDriver();
                    return false;
                }
            }

            Comunes.CIoCtl.CerrarDriver();

            return true;
        }

        public void CargarPerfil(String archivo)
        {
            CargarCalibrado();

            byte ret = 0;
            lock (this)
            {
                ret = CPerfil.CargarMapa(archivo);
            }

            if (ret == 0)
            {
                String nombre = System.IO.Path.GetFileNameWithoutExtension(archivo);
                CMain.MessageBox("Perfil cargado correctamente.", nombre, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
