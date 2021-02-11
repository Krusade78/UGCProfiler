using System;
using System.Windows;

namespace Launcher
{
    internal class CPerfil
    {
        public static byte CargarMapa(String archivo, System.IO.BinaryWriter pipe)
        {
            Comunes.DSPerfil perfil = new Comunes.DSPerfil();
            try
            {
                perfil.ReadXml(archivo);
            }
            catch (Exception ex)
            {
                CMain.MessageBox(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                perfil.Dispose();
                return 1;
            }

            #region "Comandos"
            {
                uint tamBuffer = 0;
                uint nAcciones = 0;
                foreach (Comunes.DSPerfil.ACCIONESRow r in perfil.ACCIONES.Rows)
                {
                    if (r.idAccion == 0)
                        continue;
                    nAcciones++;
                    tamBuffer += 1 + (uint)(2 * r.Comandos.Length);
                }
                byte[] bufferComandos = new byte[tamBuffer + 1];
                bufferComandos[0] = (byte)CServicio.TipoMsj.Comandos;

                int pos = 1;
                foreach (Comunes.DSPerfil.ACCIONESRow r in perfil.ACCIONES.Rows)
                {
                    if (r.idAccion == 0)
                        continue;
                    bufferComandos[pos] = (byte)r.Comandos.Length;
                    pos++;
                    for (byte i = 0; i < (byte)r.Comandos.Length; i++)
                    {
                        bufferComandos[pos] = (byte)(r.Comandos[i] & 0xff);
                        pos++;
                        bufferComandos[pos] = (byte)(r.Comandos[i] >> 8);
                        pos++;
                    }
                }

                if (pipe != null)
                {
                    pipe.Write(bufferComandos, 0, bufferComandos.Length);
                    pipe.Flush();
                }
            }
            #endregion

            #region "Mapeado"
            {
                const int TAM_TEXTO_MFD = 17;
                const int TAM_MAPAEJES = (1 + 1 + 1 + 1 + 10 + 15 + +1 + 1 + (16 * 2)) * (4 * 2 * 3 * 8);
                const int TAM_MAPABOTONES = (1 + 1 + (15 * 2)) * (4 * 2 * 3 * 16);
                const int TAM_MAPASETAS = (1 + 1 + (15 * 2)) * (4 * 2 * 3 * 32);
                byte[] bufferMapa = new byte[TAM_TEXTO_MFD + 1 + TAM_MAPAEJES + TAM_MAPABOTONES + TAM_MAPASETAS];
                int pos = 0;

                #region "Texto MFD"
                {
                    String nombre = System.IO.Path.GetFileNameWithoutExtension(archivo);
                    if (nombre.Length > 16)
                        nombre = nombre.Substring(0, 16);
                    else if (nombre.Length == 0)
                        nombre = "";
                    nombre = nombre.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                    byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(nombre));
                    for (byte i = 1; i < 17; i++)
                    {
                        if (texto.Length >= i)
                            bufferMapa[i] = texto[i - 1];
                        else
                            bufferMapa[i] = 0;
                        pos++;
                    }

                    bufferMapa[0] = 1;
                    pos++;
                }
                #endregion

                //tickraton
                bufferMapa[pos] = perfil.GENERAL[0].TickRaton;
                pos++;
                //botones
                for (byte j = 0; j < 4; j++)
                {
                    for (byte p = 0; p < 2; p++)
                    {
                        for (byte m = 0; m < 3; m++)
                        {
                            for (byte b = 0; b < 16; b++)
                            {
                                pos++;
                                bufferMapa[pos] = perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(j, p, m, b).TamIndices;
                                pos++;
                                for (byte i = 0; i < 15; i++)
                                {
                                    UInt16 indice = perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(j, p, m, b, i).idAccion;
                                    bufferMapa[pos] = (byte)(indice & 0xff);
                                    bufferMapa[pos + 1] = (byte)(indice >> 8);
                                    pos += 2;
                                }
                            }
                        }
                    }
                }
                //Setas
                for (byte j = 0; j < 4; j++)
                {
                    for (byte p = 0; p < 2; p++)
                    {
                        for (byte m = 0; m < 3; m++)
                        {
                            for (byte s = 0; s < 32; s++)
                            {
                                pos++;
                                bufferMapa[pos] = perfil.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(j, p, m, s).TamIndices;
                                pos++;
                                for (byte i = 0; i < 15; i++)
                                {
                                    UInt16 indice = perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(j, p, m, s, i).idAccion;
                                    bufferMapa[pos] = (byte)(indice & 0xff);
                                    bufferMapa[pos + 1] = (byte)(indice >> 8);
                                    pos += 2;
                                }
                            }
                        }
                    }
                }
                //ejes
                for (byte j = 0; j < 4; j++)
                {
                    for (byte p = 0; p < 2; p++)
                    {
                        for (byte m = 0; m < 3; m++)
                        {
                            for (byte e = 0; e < 8; e++)
                            {
                                Comunes.DSPerfil.MAPAEJESRow datos = perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(j, p, m, e);
                                bufferMapa[pos] = datos.Mouse;
                                pos++;
                                bufferMapa[pos] = datos.JoySalida;
                                pos++;
                                bufferMapa[pos] = datos.TipoEje;
                                pos++;
                                bufferMapa[pos] = datos.Eje;
                                pos++;
                                foreach (byte sens in datos.Sensibilidad)
                                {
                                    bufferMapa[pos] = sens;
                                    pos++;
                                }
                                foreach (byte banda in datos.Bandas)
                                {
                                    bufferMapa[pos] = banda;
                                    pos++;
                                }
                                for (byte i = 0; i < 16; i++)
                                {
                                    UInt16 indice = perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(j, p, m, e, i).idAccion;
                                    bufferMapa[pos] = (byte)(indice & 0xff);
                                    pos++;
                                    bufferMapa[pos] = (byte)(indice >> 8);
                                    pos++;
                                }
                                bufferMapa[pos] = datos.ResistenciaInc;
                                pos++;
                                bufferMapa[pos] = datos.ResistenciaDec;
                                pos++;

                            }
                        }
                    }
                }

                if (pipe != null)
                {
                    pipe.Write(bufferMapa, 0, bufferMapa.Length);
                    pipe.Flush();
                }
            }
            #endregion

            return 0;
        }
    }
}
