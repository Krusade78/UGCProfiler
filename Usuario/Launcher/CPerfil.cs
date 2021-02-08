using System;
using System.Windows;

namespace Launcher
{
    internal class CPerfil
    {
        public static byte CargarMapa(String archivo)
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

            if (!Comunes.CIoCtl.AbrirDriver())
                return 3;

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
                byte[] bufferComandos = new byte[tamBuffer];

                int pos = 0;
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

                bool ok;
                if (tamBuffer == 0)
                {
                    ok = Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_USR_COMANDOS, null, 0, null, 0, out _, IntPtr.Zero);
                }
                else
                {
                    ok = Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_USR_COMANDOS, bufferComandos, (uint)bufferComandos.Length, null, 0, out _, IntPtr.Zero);
                }
                if (!ok)
                {
                    Comunes.CIoCtl.CerrarDriver();
                    CMain.MessageBox("No se puede enviar la orden al driver", "[CPerfil][0.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return 4;
                }
            }
            #endregion

            #region "Mapeado"
            {
                const int TAM_MAPAEJES = (1 + 1 + 1+ 10 + 15 + +1 + 1 + (16 * 2)) * (2 * 3 * 4);
                const int TAM_MAPAEJESPEQUE = (1 + 1 + 1 + 15 + 1 + 1 + (16 * 2)) * (2 * 3 * 5);
                const int TAM_MAPAEJESMINI = (1 + 1 + 1 + 1) * (2 * 3 * 2);
                const int TAM_MAPABOTONES = (1 + 1 + (15 * 2)) * (2 * 3 * 26);
                const int TAM_MAPASETAS = (1 + 1 + (15 * 2)) * (2 * 3 * 32);
                byte[] bufferMapa = new byte[TAM_MAPAEJES + TAM_MAPAEJESPEQUE + TAM_MAPAEJESMINI + 1 + TAM_MAPABOTONES + TAM_MAPASETAS];

                int pos = 0;
                //tickraton
                bufferMapa[pos] = perfil.GENERAL[0].TickRaton;
                pos++;
                //botones
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte b = 0; b < 26; b++)
                        {
                            pos++;
                            bufferMapa[pos] = perfil.MAPABOTONES.FindByidPinkieidModoidBoton(p, m, b).TamIndices;
                            pos++;
                            for (byte i = 0; i < 15; i++)
                            {
                                UInt16 indice = perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(b, m, p, i).Accion;
                                bufferMapa[pos] = (byte)(indice & 0xff);
                                bufferMapa[pos + 1] = (byte)(indice >> 8);
                                pos += 2;
                            }
                        }
                    }
                }
                //Setas
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte s = 0; s < 32; s++)
                        {
                            pos++;
                            bufferMapa[pos] = perfil.MAPASETAS.FindByidPinkieidModoIdSeta(p, m, s).TamIndices;
                            pos++;
                            for (byte i = 0; i < 15; i++)
                            {
                                UInt16 indice = perfil.INDICESSETAS.FindByididSetaidModoidPinkie(i, s, m, p).Accion;
                                bufferMapa[pos] = (byte)(indice & 0xff);
                                bufferMapa[pos + 1] = (byte)(indice >> 8);
                                pos += 2;
                            }
                        }
                    }
                }
                //ejes
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 4; e++)
                        {
                            Comunes.DSPerfil.MAPAEJESRow datos = perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p);
                            bufferMapa[pos] = datos.Mouse;
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
                                UInt16 indice = perfil.INDICESEJES.FindByididEjeidModoidPinkie(i, e, m, p).Accion;
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
                //ejespeque
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 5; e++)
                        {
                            Comunes.DSPerfil.MAPAEJESPEQUERow datos = perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p);
                            bufferMapa[pos] = datos.Mouse;
                            pos++;
                            bufferMapa[pos] = datos.TipoEje;
                            pos++;
                            bufferMapa[pos] = datos.Eje;
                            pos++;
                            foreach (byte banda in datos.Bandas)
                            {
                                bufferMapa[pos] = banda;
                                pos++;
                            }
                            for (byte i = 0; i < 16; i++)
                            {
                                UInt16 indice = perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(i, e, m, p).Accion;
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
                //ejespequemini
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 2; e++)
                        {
                            Comunes.DSPerfil.MAPAEJESMINIRow datos = perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(e, m, p);
                            bufferMapa[pos] = datos.Mouse;
                            pos++;
                            bufferMapa[pos] = datos.TipoEje;
                            pos++;
                            bufferMapa[pos] = datos.Eje;
                            pos++;
                            pos++;
                        }
                    }
                }

                if (!Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_USR_MAPA, bufferMapa, (uint)bufferMapa.Length, null, 0, out _, IntPtr.Zero))
                {
                    Comunes.CIoCtl.CerrarDriver();
                    CMain.MessageBox("No se puede enviar la orden al driver", "[CPerfil][0.3]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return 4;
                }
            }
            #endregion

            #region "Texto MFD"
            {
                String nombre = System.IO.Path.GetFileNameWithoutExtension(archivo);
                if (nombre.Length > 16)
                    nombre = nombre.Substring(0, 16);
                else if (nombre.Length == 0)
                    nombre = "";
                nombre = nombre.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(nombre));
                byte[] buffer = new byte[17];
                for (byte i = 1; i < 17; i++)
                {
                    if (texto.Length >= i)
                        buffer[i] = texto[i - 1];
                    else
                        buffer[i] = 0;
                }

                buffer[0] = 1;

                Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out _, IntPtr.Zero);
                //siguientes lineas en blanco
                buffer[1] = 0;
                buffer[0] = 2;
                Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_TEXTO, buffer, 2, null, 0, out _, IntPtr.Zero);
                buffer[0] = 3;
                Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_TEXTO, buffer, 2, null, 0, out _, IntPtr.Zero);
            }

            Comunes.CIoCtl.CerrarDriver();

            return 0;
        }
        #endregion
    }
}
