using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher
{
    class CBaseServicio : IDisposable
    {
        CancellationTokenSource hSalir = null;

        public CBaseServicio()
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
                    if (hSalir != null)
                    {
                        hSalir.Cancel();
                        while (hSalir != null)
                            Thread.Sleep(150);
                    }
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

        async public void Iniciar()
        {
            hSalir = new CancellationTokenSource();
            await Task.Run(() => Hilo(), hSalir.Token);
        }

        private void Hilo()
        {
            CServicio serv = new CServicio();
            Semaphore hHora = new Semaphore(1, 1, "eXHOTASHora");
            Semaphore hFecha = new Semaphore(1, 1, "eXHOTASFecha");
            Semaphore hConfig = new Semaphore(1, 1, "eXHOTASCargar");
            while (!hSalir.Token.IsCancellationRequested)
            {
                if (hHora.WaitOne(0))
                    serv.horaActiva = true;
                else
                    serv.horaActiva = false;

                if (hFecha.WaitOne(0))
                    serv.fechaActiva = true;
                else
                    serv.fechaActiva = false;

                if (hFecha.WaitOne(0))
                    serv.CargarConfiguracion();

                serv.Tick();
                Thread.Sleep(2000);
            }
            hHora.Close();
            hFecha.Close();
            hConfig.Close();
            serv.Dispose(); serv = null;
            hSalir.Token.ThrowIfCancellationRequested();
        }
    }
}
