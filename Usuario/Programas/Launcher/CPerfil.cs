﻿using System;
using System.Windows;

namespace Launcher
{
    internal class CPerfil
    {
        public static byte CargarMapa(String archivo, ref bool horaModif, ref bool fechaModif)
        {
            DSPerfil perfil = new DSPerfil();
            try
            {
                perfil.ReadXml(archivo);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                perfil.Dispose();
                return 1;
            }

            Microsoft.Win32.SafeHandles.SafeFileHandle driver = driver = CSystem32.CreateFile(
                        "\\\\.\\XUSBInterface",
                        0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                        0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                        IntPtr.Zero,
                        3,//OPEN_EXISTING,
                        0,
                        IntPtr.Zero);
            if (driver.IsInvalid)
            {
                MessageBox.Show("No se puede abrir el driver", "[CPerfil][0.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return 3;
            }

            #region "Comandos"
            {
                uint tamBuffer = 2;
                foreach (DSPerfil.ACCIONESRow r in perfil.ACCIONES.Rows)
                {
                    tamBuffer += (uint)(2 * r.Comandos.Length);
                }
                byte[] bufferComandos = new byte[tamBuffer];

                bufferComandos[0] = (byte)(perfil.ACCIONES.Rows.Count & 0xff);
                bufferComandos[1] = (byte)(perfil.ACCIONES.Rows.Count >> 8);
                int pos = 2;
                foreach (DSPerfil.ACCIONESRow r in perfil.ACCIONES.Rows)
                {
                    bufferComandos[pos] = (byte)r.Comandos.Length;
                    pos++;
                    for (byte i = 0; i < (byte)r.Comandos.Length; i++)
                    {
                        bufferComandos[pos] = (byte)(r.Comandos[i] & 0xff);
                        pos++;
                        bufferComandos[pos] = (byte)(r.Comandos[i] >> 8);
                        if ((bufferComandos[pos] == (byte)CSystem32.TipoComando.TipoComando_MfdHora) || (bufferComandos[pos] == (byte)CSystem32.TipoComando.TipoComando_MfdHora24))
                            horaModif = true;
                        else if (bufferComandos[pos] == (byte)CSystem32.TipoComando.TipoComando_MfdFecha)
                            fechaModif = true;
                        pos++;
                    }
                }

                UInt32 ret = 0;
                UInt32 IOCTL_USR_COMANDOS = ((0x22) << 16) | ((2) << 14) | ((0x080b) << 2) | (0);
                if (!CSystem32.DeviceIoControl(driver, IOCTL_USR_COMANDOS, bufferComandos, (uint)bufferComandos.Length, null, 0, out ret, IntPtr.Zero))
                {
                    driver.Close();
                    MessageBox.Show("No se puede enviar la orden al driver", "[CPerfil][0.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return 4;
                }
            }

            #endregion

            #region "Mapeado"
            {
                const int TAM_MAPAEJES = (1 + 1 + 10 + 15 + (16 * 2)) * (2 * 3 * 4);
                const int TAM_MAPAEJESPEQUE = (1 + 1 + 15 + (16 * 2)) * (2 * 3 * 4);
                const int TAM_MAPAEJESMINI = (1 + 1) * (2 * 3 * 2);
                const int TAM_MAPABOTONES = (1 + (15 * 2)) * (2 * 3 * 26);
                const int TAM_MAPASETAS = (1 + (15 * 2)) * (2 * 3 * 32);
                byte[] bufferMapa = new byte[TAM_MAPAEJES + TAM_MAPAEJESPEQUE + TAM_MAPAEJESMINI + 1 + TAM_MAPABOTONES + TAM_MAPASETAS];

                int pos = 0;
                //ejes
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 4; e++)
                        {
                            DSPerfil.MAPAEJESRow datos = perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p);
                            bufferMapa[pos] = datos.Mouse;
                            pos++;
                            bufferMapa[pos] = datos.nEje;
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
                                UInt16 indice = perfil.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), i).Indice;
                                bufferMapa[pos] = (byte)(indice & 0xff);
                                pos++;
                                bufferMapa[pos] = (byte)(indice >> 8);
                                pos++;
                            }
                        }
                    }
                }
                //ejespeque
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        for (byte e = 0; e < 4; e++)
                        {
                            DSPerfil.MAPAEJESPEQUERow datos = perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p);
                            bufferMapa[pos] = datos.Mouse;
                            pos++;
                            bufferMapa[pos] = datos.nEje;
                            pos++;
                            foreach (byte banda in datos.Bandas)
                            {
                                bufferMapa[pos] = banda;
                                pos++;
                            }
                            for (byte i = 0; i < 16; i++)
                            {
                                UInt16 indice = perfil.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), i).Indice;
                                bufferMapa[pos] = (byte)(indice & 0xff);
                                pos++;
                                bufferMapa[pos] = (byte)(indice >> 8);
                                pos++;
                            }
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
                            DSPerfil.MAPAEJESMINIRow datos = perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(e, m, p);
                            bufferMapa[pos] = datos.Mouse;
                            pos++;
                            bufferMapa[pos] = datos.nEje;
                            pos++;
                        }
                    }
                }
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
                            DSPerfil.MAPABOTONESRow datos = perfil.MAPABOTONES.FindByidEjeidModoidPinkie(b, m, p);
                            bufferMapa[pos] = datos.Estado;
                            pos++;
                            for (byte i = 0; i < 15; i++)
                            {
                                UInt16 indice = perfil.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | b), i).Indice;
                                bufferMapa[pos] = (byte)(indice & 0xff);
                                pos++;
                                bufferMapa[pos] = (byte)(indice >> 8);
                                pos++;
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
                            DSPerfil.MAPASETASRow datos = perfil.MAPASETAS.FindByidEjeidModoidPinkie(s, m, p);
                            bufferMapa[pos] = datos.Estado;
                            pos++;
                            for (byte i = 0; i < 15; i++)
                            {
                                UInt16 indice = perfil.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | s), i).Indice;
                                bufferMapa[pos] = (byte)(indice & 0xff);
                                pos++;
                                bufferMapa[pos] = (byte)(indice >> 8);
                                pos++;
                            }
                        }
                    }
                }

                UInt32 ret = 0;
                UInt32 IOCTL_USR_MAPA = ((0x22) << 16) | ((2) << 14) | ((0x080a) << 2) | (0);
                if (!CSystem32.DeviceIoControl(driver, IOCTL_USR_MAPA, bufferMapa, (uint)bufferMapa.Length, null, 0, out ret, IntPtr.Zero))
                {
                    driver.Close();
                    MessageBox.Show("No se puede enviar la orden al driver", "[CPerfil][0.3]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return 4;
                }
            }
            #endregion

            #region "Texto MFD"
            {
                String nombre = System.IO.Path.GetFileName(archivo);
                if (nombre.Length > 16)
                    nombre = nombre.Substring(0, 16);
                else if (nombre.Length == 0)
                    nombre = "";

                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(archivo));
                byte[] buffer = new byte[18];
                for (byte i = 1; i < 18; i++)
                {
                    if (texto.Length >= i)
                        buffer[i] = texto[i - 1];
                    else
                        buffer[i] = 0;
                }

                buffer[0] = 1;

                UInt32 ret = 0;
                UInt32 IOCTL_TEXTO = ((0x22) << 16) | ((2) << 14) | ((0x0804) << 2) | (0);
                CSystem32.DeviceIoControl(driver, IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero);
                //siguientes lineas en blanco
                buffer[1] = 0;
                buffer[0] = 2;
                CSystem32.DeviceIoControl(driver, IOCTL_TEXTO, buffer, 2, null, 0, out ret, IntPtr.Zero);
                buffer[0] = 3;
                CSystem32.DeviceIoControl(driver, IOCTL_TEXTO, buffer, 2, null, 0, out ret, IntPtr.Zero);
            }

            driver.Close();

            return 0;
        }
        #endregion
    }
}