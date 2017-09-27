using System;
using System.Windows;

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

            Perfil.GENERAL.AddGENERALRow(Perfil.GENERAL.NewGENERALRow());
            DSPerfil.ACCIONESRow accionVacia = Perfil.ACCIONES.AddACCIONESRow(0, "</------- Ninguna -------/>", new ushort[0]);
            for (byte p = 0; p < 2; p++)
            {
                for (byte m = 0; m < 3; m++)
                {
                    for (byte e = 0; e < 4; e++)
                    {
                        Perfil.MAPAEJES.AddMAPAEJESRow(p, m, e, 0, 0, new byte[10], new byte[15], 0, 0);
                        Perfil.MAPAEJESPEQUE.AddMAPAEJESPEQUERow(p, m, e, 0, 0, new byte[15], 0, 0);
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

        public bool Cargar(String archivo)
        {
            Perfil.Clear();
            try
            {
                Perfil.ReadXml("archivo");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public bool Guardar(String archivo)
        {
            //Reordenar ids acciones
            ushort idmax = 0;
            foreach (DSPerfil.ACCIONESRow ar in Perfil.ACCIONES.Rows)
                if (ar.idAccion > idmax)
                    idmax = ar.idAccion;

            ushort id = 0;
            while (id < Perfil.ACCIONES.Rows.Count)
            {
                if (Perfil.ACCIONES.FindByidAccion(id) == null)
                {
                    for (ushort i = idmax; i > id; i--)
                    {
                        DSPerfil.ACCIONESRow recolocar = Perfil.ACCIONES.FindByidAccion(idmax);
                        if (recolocar != null)
                        {
                            recolocar.idAccion = id;
                            idmax = (ushort)(i - 1);
                            break;
                        }
                    }
                }
                id++;
            }
            try
            {
                Perfil.WriteXml(archivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}
