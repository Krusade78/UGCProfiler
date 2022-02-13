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

        public bool Cargar(string archivo)
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

        public bool Guardar(string archivo)
        {
            ushort[,] mapaRangosMax = {
                    { 0,0,0,512,0,0,128,128 },
                    { 2048,2048,0,1024,0,0,0,0 },
                    { 0,0,256,256,256,256,16,16 },
                    { 4096,4096,2048,4096,0,0,1024,1024 }
                };

            //Reordenar ids acciones
            ushort id = 0;
            foreach (DSPerfil.ACCIONESRow ar in Perfil.ACCIONES.Rows)
            {
                foreach (ushort c in ar.Comandos)
                {
                    byte tipo = (byte)(c & 0x7f);
                    byte dato = (byte)(c >> 8);
                    bool soltar = ((c & 0xff) & (byte)CTipos.TipoComando.TipoComando_Soltar) == (byte)CTipos.TipoComando.TipoComando_Soltar;
                    if (tipo == (byte)CTipos.TipoComando.TipoComando_ModoPreciso)
                    {
                        if (!soltar)
                        {
                            ushort nuevoRango = (ushort)(mapaRangosMax[(dato & 31) / 8, (dato & 31) % 8] * ((dato >> 5) + 1));
                            if (mapaRangosMax[(dato & 31) / 8, (dato & 31) % 8] < nuevoRango)
                            {
                                mapaRangosMax[(dato & 31) / 8, (dato & 31) % 8] = nuevoRango;
                            }
                        }
                    }
                }

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

            Perfil.RANGOSENTRADA.Clear();
            for (byte j = 0; j < 4; j++)
            {
                for (byte e = 0; e < 8; e++)
                {
                    Perfil.RANGOSENTRADA.AddRANGOSENTRADARow(j, e, mapaRangosMax[j, e]);
                }
            }

            Perfil.RANGOSSALIDA.Clear();
            for (byte e = 0; e < 8; e++)
            {
                Perfil.RANGOSSALIDA.AddRANGOSSALIDARow(0, e, 0);
                Perfil.RANGOSSALIDA.AddRANGOSSALIDARow(1, e, 0);
                Perfil.RANGOSSALIDA.AddRANGOSSALIDARow(2, e, 0);
            }
            for (byte j = 0; j < 4; j++)
            {
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 8; e++)
                        {
                            byte eje = Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(j, p, m, e).Eje;
                            byte js = Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(j, p, m, e).JoySalida;
                            byte tipo = Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(j, p, m, e).TipoEje;
                            if ((tipo & 0b1) == 1)
                            {
                                if (Perfil.RANGOSSALIDA.FindByidJoyidEje(js, eje).Maximo < Perfil.RANGOSENTRADA.FindByidJoyidEje(j, e).Maximo)
                                {
                                    Perfil.RANGOSSALIDA.FindByidJoyidEje(js, eje).Maximo = Perfil.RANGOSENTRADA.FindByidJoyidEje(j, e).Maximo;
                                }
                            }
                        }
                    }
                }
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
