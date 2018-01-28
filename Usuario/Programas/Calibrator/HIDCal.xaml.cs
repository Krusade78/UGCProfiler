using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para HIDCal.xaml
    /// </summary>
    internal partial class HIDCal : UserControl
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct CALIBRADO
        {
            internal ushort i;
            internal ushort c;
            internal ushort d;
            internal byte n;
            internal byte Margen;
            internal byte Resistencia;
            internal byte cal;
            internal byte antiv;
        };

        private int ejeSel = 0;
        private struct STLIMITES
        {
            internal byte cal;
            internal ushort i;
            internal ushort c;
            internal ushort d;
            internal byte n;
        };
        private struct STJITTER
        {
            internal byte antiv;
            internal long PosElegida;
            internal byte PosRepetida;
            internal byte Margen;
            internal byte Resistencia;
        };

        STJITTER[] jitter = new STJITTER[4];
        STLIMITES[] limites = new STLIMITES[4];

        public HIDCal()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
            {
                limites[i].cal = 0;
                limites[i].c = 1024;
                limites[i].i = 0;
                limites[i].d = 2048;
                jitter[i].antiv = 0;
                jitter[i].Margen = 0;
                jitter[i].Resistencia = 0;
            }

            System.IO.FileStream archivo = null;
            try
            {
                archivo = new System.IO.FileStream("calibrado.dat", System.IO.FileMode.Open);
                for (int i = 0; i < 4; i++)
                {
                    byte[] buf = new byte[Marshal.SizeOf(typeof(CALIBRADO))];
                    archivo.Read(buf, 0, buf.Length);


                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CALIBRADO)));
                    Marshal.Copy(buf, 0, ptr, buf.Length);
                    CALIBRADO bufCal = (CALIBRADO)Marshal.PtrToStructure(ptr, typeof(CALIBRADO));
                    Marshal.FreeHGlobal(ptr);

                    limites[i].c = bufCal.c;
                    limites[i].i = bufCal.i;
                    limites[i].d = bufCal.d;
                    limites[i].n = bufCal.n;
                    limites[i].cal = bufCal.cal;
                    jitter[i].Margen = bufCal.Margen;
                    jitter[i].Resistencia = bufCal.Resistencia;
                    jitter[i].antiv = bufCal.antiv;
                }
            }
            catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            finally
            {
                if (archivo != null) { try { archivo.Close(); archivo = null; } catch { } }
            }

            CargarTextosEje(0);
        }

        public void ActualizarEstado(byte[] hidData)
        {
            int posr = (hidData[(ejeSel * 2) + 1] << 8) | hidData[(ejeSel * 2)];
            txtPosReal.Text = posr.ToString();
            posReal.Margin = new Thickness(posr - 5, 0, 0, 0);

            // Filtrado de ejes
            long pollEje = (ushort)posr;

            if (jitter[ejeSel].antiv == 1)
            {
                // Antivibraciones
                if ((pollEje < (jitter[ejeSel].PosElegida - jitter[ejeSel].Margen)) || (pollEje > (jitter[ejeSel].PosElegida + jitter[ejeSel].Margen)))
                {
                    jitter[ejeSel].PosRepetida = 0;
                    jitter[ejeSel].PosElegida = pollEje;
                }
                else
                {
                    if (jitter[ejeSel].PosRepetida < jitter[ejeSel].Resistencia)
                    {
                        jitter[ejeSel].PosRepetida++;
                        pollEje = jitter[ejeSel].PosElegida;
                    }
                    else
                    {
                        jitter[ejeSel].PosRepetida = 0;
                        jitter[ejeSel].PosElegida = pollEje;
                    }
                }
            }

            if (limites[ejeSel].cal == 1)
            {
                // Calibrado
                ushort ancho1, ancho2;
                ancho1 = (ushort)((limites[ejeSel].c - limites[ejeSel].n) - limites[ejeSel].i);
                ancho2 = (ushort)(limites[ejeSel].d - (limites[ejeSel].c + limites[ejeSel].n));
                if (((pollEje >= (limites[ejeSel].c - limites[ejeSel].n)) && (pollEje <= (limites[ejeSel].c + limites[ejeSel].n))))
                {
                    //Zona nula
                    pollEje = 1024;
                }
                else
                {
                    if (pollEje < limites[ejeSel].i)
                        pollEje = limites[ejeSel].i;
                    if (pollEje > limites[ejeSel].d)
                        pollEje = limites[ejeSel].d;

                    if (pollEje < limites[ejeSel].c)
                    {
                        pollEje = ((limites[ejeSel].c - limites[ejeSel].n) - pollEje);
                        if (ancho1 > ancho2)
                        {
                            pollEje = (pollEje * ancho2) / ancho1;
                        }
                        pollEje = 0 - pollEje;
                    }
                    else
                    {
                        pollEje -= (limites[ejeSel].c + limites[ejeSel].n);
                        if (ancho2 > ancho1)
                            pollEje = ((pollEje * ancho1) / ancho2);
                    }

                    if (ancho2 > ancho1)
                        pollEje = ((pollEje + ancho1) * (2048)) / (2 * ancho1);
                    else
                        pollEje = ((pollEje + ancho2) * (2048)) / (2 * ancho2);
                }
            }

            txtPosCal.Text = pollEje.ToString();
            posCal.Margin = new Thickness(pollEje - 5, 0, 0, 0);
        }

        private void btAplicar_Click(object sender, RoutedEventArgs e)
        {
            limites[ejeSel].cal = (chkCalActiva.IsChecked == true) ? (byte)1 : (byte)0;
            ushort s = 0;
            ushort.TryParse(txtI.Text, out s);
            limites[ejeSel].i = 0;
            ushort.TryParse(txtC.Text, out s);
            limites[ejeSel].c = s;
            ushort.TryParse(txtD.Text, out s);
            limites[ejeSel].d = s;
            byte b = 0;
            byte.TryParse(txtN.Text, out b);
            limites[ejeSel].n = b;

            jitter[ejeSel].antiv = (chkAntivActiva.IsChecked == true) ? (byte)1 : (byte)0;
            byte.TryParse(txtMargen.Text, out b);
            jitter[ejeSel].Margen = b;
            byte.TryParse(txtResistencia.Text, out b);
            jitter[ejeSel].Resistencia = b;

            btAplicar.Foreground = System.Windows.Media.Brushes.GreenYellow;
        }

        private void btGuardar_Click(object sender, RoutedEventArgs e)
        {
            CALIBRADO[] bufCal = new CALIBRADO[4];

            for (int i = 0; i < 4; i++)
            {
                bufCal[i].c = limites[i].c;
                bufCal[i].i = limites[i].i;
                bufCal[i].d = limites[i].d;
                bufCal[i].n = limites[i].n;
                bufCal[i].cal = limites[i].cal;
                bufCal[i].Margen = jitter[i].Margen;
                bufCal[i].Resistencia = jitter[i].Resistencia;
                bufCal[i].antiv = jitter[i].antiv;
            }

            byte[] buf = new byte[Marshal.SizeOf(typeof(CALIBRADO)) * 4];

            System.IO.FileStream archivo = null;
            try
            {
                archivo = new System.IO.FileStream("calibrado.dat", System.IO.FileMode.Create);
                for (int i = 0; i < 4; i++)
                {
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CALIBRADO)));
                    Marshal.StructureToPtr(bufCal[i], ptr, true);
                    Marshal.Copy(ptr, buf, Marshal.SizeOf(typeof(CALIBRADO)) * i, Marshal.SizeOf(typeof(CALIBRADO)));
                    Marshal.FreeHGlobal(ptr);
                }
                archivo.Write(buf, 0, buf.Length);
                archivo.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            finally
            {
                if (archivo != null) { try { archivo.Close(); archivo = null; } catch { } }
            }

            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CSystem32.CreateFile(
                    "\\\\.\\XUSBInterface",
                    0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                    0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                    IntPtr.Zero,
                    3,//OPEN_EXISTING,
                    0,
                    IntPtr.Zero);
            if (driver.IsInvalid)
            {
                MessageBox.Show("No se puede abrir el driver", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UInt32 ret = 0;
            UInt32 IOCTL_USR_CALIBRADO = ((0x22) << 16) | ((2) << 14) | ((0x0809) << 2) | (0);
            if (!CSystem32.DeviceIoControl(driver, IOCTL_USR_CALIBRADO, buf, (uint)buf.Length, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("No se puede enviar la orden al driver", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            driver.Close();
        }

        private void tbX_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                tbY.IsChecked = false;
                tbZ.IsChecked = false;
                tbR.IsChecked = false;
                CargarTextosEje(0);
            }
        }

        private void tbY_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            CargarTextosEje(1);
        }

        private void tbR_Checked(object sender, RoutedEventArgs e)
        {
            tbY.IsChecked = false;
            tbZ.IsChecked = false;
            tbX.IsChecked = false;
            CargarTextosEje(2);
        }

        private void tbZ_Checked(object sender, RoutedEventArgs e)
        {
            tbY.IsChecked = false;
            tbX.IsChecked = false;
            tbR.IsChecked = false;
            CargarTextosEje(3);
        }

        private void CargarTextosEje(int eje)
        {
            ejeSel = eje;
            chkCalActiva.IsChecked = (limites[ejeSel].cal == 1) ? true : false;
            txtI.Text = limites[ejeSel].i.ToString();
            txtC.Text = limites[ejeSel].c.ToString();
            txtN.Text = limites[ejeSel].n.ToString();
            txtD.Text = limites[ejeSel].d.ToString();
            chkAntivActiva.IsChecked = (jitter[ejeSel].antiv == 1) ? true : false;
            txtMargen.Text = jitter[ejeSel].Margen.ToString();
            txtResistencia.Text = jitter[ejeSel].Resistencia.ToString();
        }

        private void chk_Cambiado(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
                btAplicar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x84, 0x2F));
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
                btAplicar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x84, 0x2F));
        }
    }
}
