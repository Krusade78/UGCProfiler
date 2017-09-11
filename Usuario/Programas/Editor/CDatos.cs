using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    class CDatos : IDisposable
    {
        public DSPerfil Perfil { get; } = new DSPerfil();

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Perfil.Dispose();
                }

                disposedValue = true;
            }
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
             GC.SuppressFinalize(this);
        }
        #endregion

        public void Nuevo()
        {
            Perfil.Clear();

            Perfil.GENERAL.AddGENERALRow(0, false, false, false);
            DSPerfil.ACCIONESRow accionVacia = Perfil.ACCIONES.AddACCIONESRow(0, new ushort[0]);
            for (byte p = 0; p < 2; p++)
            {
                for (byte m = 0; m < 3; m++)
                {
                    for (byte e = 0; e < 4; e++)
                    {
                        Perfil.MAPAEJES.AddMAPAEJESRow(p, m, e, 0, 0, new byte[10], new byte[15]);
                        Perfil.MAPAEJESPEQUE.AddMAPAEJESPEQUERow(p, m, e, 0, 0, new byte[15]);
                        for (byte i = 0; i < 16; i++)
                        {
                            Perfil.INDICESEJES.AddINDICESEJESRow((UInt32)((p << 16) | (m << 8) | e), i, accionVacia);
                            Perfil.INDICESEJESPEQUE.AddINDICESEJESPEQUERow((UInt32)((p << 16) | (m << 8) | e), i, accionVacia);
                        }
                    }
                    for (byte e = 0; e < 2; e++)
                    {
                        Perfil.MAPAEJESMINI.AddMAPAEJESMINIRow(p, m, e, 0, 0);
                    }
                    for (byte b = 0; b < 26; b++)
                    {
                        Perfil.MAPABOTONES.AddMAPABOTONESRow(p, m, b, 0);
                        for (byte i = 0; i < 15; i++)
                            Perfil.INDICESBOTONES.AddINDICESBOTONESRow((UInt32)((p << 16) | (m << 8) | b), i, accionVacia);
                    }
                    for (byte s = 0; s < 32; s++)
                    {
                        Perfil.MAPASETAS.AddMAPASETASRow(p, m, s, 0);
                        for (byte i = 0; i < 15; i++)
                            Perfil.INDICESSETAS.AddINDICESSETASRow((UInt32)((p << 16) | (m << 8) | s), i, accionVacia);
                    }
                }
            }
        }
    }
}
