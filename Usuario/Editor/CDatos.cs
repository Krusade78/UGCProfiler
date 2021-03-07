using System;
using System.Windows;
using Comunes;

namespace Editor
{
    class CDatos : IDisposable
    {
        public DSPerfil Perfil { get; } = new DSPerfil();
        public bool Modificado { get; set; } = false;

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

            Perfil.GENERAL.AddGENERALRow(1);
            DSPerfil.ACCIONESRow accionVacia = Perfil.ACCIONES.AddACCIONESRow(0, "</------- Ninguna -------/>", new ushort[0]);
            for (byte j = 0; j < 4; j++)
            {
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 8; e++)
                        {
                            Perfil.MAPAEJES.AddMAPAEJESRow(j, p, m, e, 1, (j == 3) ? (byte)1 : j, 0, e, new byte[10] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }, 0, new byte[15], 0, 0);
                            for (byte i = 0; i < 16; i++)
                            {
                                Perfil.INDICESEJES.AddINDICESEJESRow(j, p, m, e, i, accionVacia);
                            }
                        }
                        for (byte b = 0; b < 16; b++)
                        {
                            Perfil.MAPABOTONES.AddMAPABOTONESRow(j, p, m, b, 0);
                            for (byte i = 0; i < 15; i++)
                                Perfil.INDICESBOTONES.AddINDICESBOTONESRow(j, p, m, b, i, accionVacia);
                        }
                        for (byte s = 0; s < 32; s++)
                        {
                            Perfil.MAPASETAS.AddMAPASETASRow(j, p, m, s, 0);
                            for (byte i = 0; i < 15; i++)
                                Perfil.INDICESSETAS.AddINDICESSETASRow(j, p, m, s, i, accionVacia);
                        }
                    }
                }
            }
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(0, 0, 0, 3).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(0, 0, 0, 6).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(0, 0, 0, 7).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(1, 0, 0, 0).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(1, 0, 0, 1).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(1, 0, 0, 3).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(2, 0, 0, 2).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(2, 0, 0, 3).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(2, 0, 0, 4).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(2, 0, 0, 5).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(2, 0, 0, 6).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(2, 0, 0, 7).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 0).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 1).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 2).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 3).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 4).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 5).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 6).TipoEje = 1;
            Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(3, 0, 0, 7).TipoEje = 1;

            Modificado = true;
        }

        public bool Cargar(String archivo)
        {
            Perfil.Clear();
            try
            {
                Perfil.ReadXml(archivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Modificado = false;
            return true;
        }

        public bool Guardar(String archivo)
        {
            //Reordenar ids acciones
            ushort id = 0;
            foreach (DSPerfil.ACCIONESRow ar in Perfil.ACCIONES.Rows)
            {
                if (ar.idAccion != id)
                {
                    DSPerfil.ACCIONESRow reemplazada = Perfil.ACCIONES.FindByidAccion(id);
                    if (reemplazada != null)
                    {
                        ushort antigua = ar.idAccion;
                        reemplazada.idAccion = ushort.MaxValue;
                        ar.idAccion = id;
                        reemplazada.idAccion = antigua;
                    }
                    else
                    {
                        ar.idAccion = id;
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

            Modificado = false;
            return true;
        }
    }
}
