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
        private int ejeSel = 0;
        private readonly Comunes.CTipos.STJITTER[] jitter = new Comunes.CTipos.STJITTER[4];
        private readonly Comunes.CTipos.STLIMITES[] limites = new Comunes.CTipos.STLIMITES[4];

        public HIDCal()
        {
            InitializeComponent();

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

            using (Comunes.DataSetConfiguracion dsc = new Comunes.DataSetConfiguracion())
            {
                try
                {
                    dsc.ReadXml("configuracion.dat");
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
                }
                catch (System.IO.FileNotFoundException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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

            if (jitter[ejeSel].Antiv == 1)
            {
                // Antivibraciones
                if ((pollEje < (jitter[ejeSel].PosElegida - jitter[ejeSel].Margen)) || (pollEje > (jitter[ejeSel].PosElegida + jitter[ejeSel].Margen)))
                {
                    jitter[ejeSel].PosRepetida = 0;
                    jitter[ejeSel].PosElegida = (ushort)(ulong)pollEje;
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
                        jitter[ejeSel].PosElegida = (ushort)(ulong)pollEje;
                    }
                }
            }

            if (limites[ejeSel].Cal == 1)
            {
                // Calibrado
                ushort ancho1, ancho2;
                ancho1 = (ushort)((limites[ejeSel].Cen - limites[ejeSel].Nulo) - limites[ejeSel].Izq);
                ancho2 = (ushort)(limites[ejeSel].Der - (limites[ejeSel].Cen + limites[ejeSel].Nulo));
                if (((pollEje >= (limites[ejeSel].Cen - limites[ejeSel].Nulo)) && (pollEje <= (limites[ejeSel].Cen + limites[ejeSel].Nulo))))
                {
                    //Zona nula
                    pollEje = 1024;
                }
                else
                {
                    if (pollEje < limites[ejeSel].Izq)
                        pollEje = limites[ejeSel].Izq;
                    if (pollEje > limites[ejeSel].Der)
                        pollEje = limites[ejeSel].Der;

                    if (pollEje < limites[ejeSel].Cen)
                    {
                        pollEje = ((limites[ejeSel].Cen - limites[ejeSel].Nulo) - pollEje);
                        if (ancho1 > ancho2)
                        {
                            pollEje = (pollEje * ancho2) / ancho1;
                        }
                        pollEje = 0 - pollEje;
                    }
                    else
                    {
                        pollEje -= (limites[ejeSel].Cen + limites[ejeSel].Nulo);
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

        private void FbtAplicar_Click(object sender, RoutedEventArgs e)
        {
            limites[ejeSel].Cal = (chkCalActiva.IsChecked == true) ? (byte)1 : (byte)0;
            ushort.TryParse(txtI.Text, out ushort s);
            limites[ejeSel].Izq = s;
            ushort.TryParse(txtC.Text, out s);
            limites[ejeSel].Cen = s;
            ushort.TryParse(txtD.Text, out s);
            limites[ejeSel].Der = s;
            byte.TryParse(txtN.Text, out byte b);
            limites[ejeSel].Nulo = b;

            jitter[ejeSel].Antiv = (chkAntivActiva.IsChecked == true) ? (byte)1 : (byte)0;
            byte.TryParse(txtMargen.Text, out b);
            jitter[ejeSel].Margen = b;
            byte.TryParse(txtResistencia.Text, out b);
            jitter[ejeSel].Resistencia = b;

            btAplicar.Foreground = System.Windows.Media.Brushes.GreenYellow;
        }

        private void FbtGuardar_Click(object sender, RoutedEventArgs e)
        {
            using (Comunes.DataSetConfiguracion dsc = new Comunes.DataSetConfiguracion())
            {
                try
                {
                    dsc.ReadXml("configuracion.dat");
                }
                catch (System.IO.FileNotFoundException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dsc.CALIBRADO_JITTER.Clear();
                dsc.CALIBRADO_LIMITES.Clear();
                for (int i = 0; i < 4; i++)
                {
                    dsc.CALIBRADO_LIMITES.AddCALIBRADO_LIMITESRow(limites[i].Cal, limites[i].Nulo, limites[i].Izq, limites[i].Cen, limites[i].Der);
                    dsc.CALIBRADO_JITTER.AddCALIBRADO_JITTERRow(jitter[i].Antiv, jitter[i].PosRepetida, jitter[i].Margen, jitter[i].Resistencia);
                }

                try
                {
                    dsc.WriteXml("configuracion.dat");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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

        private void FtbX_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                tbY.IsChecked = false;
                tbZ.IsChecked = false;
                tbR.IsChecked = false;
                CargarTextosEje(0);
            }
        }

        private void FtbY_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            CargarTextosEje(1);
        }

        private void FtbR_Checked(object sender, RoutedEventArgs e)
        {
            tbY.IsChecked = false;
            tbZ.IsChecked = false;
            tbX.IsChecked = false;
            CargarTextosEje(2);
        }

        private void FtbZ_Checked(object sender, RoutedEventArgs e)
        {
            tbY.IsChecked = false;
            tbX.IsChecked = false;
            tbR.IsChecked = false;
            CargarTextosEje(3);
        }

        private void CargarTextosEje(int eje)
        {
            ejeSel = eje;
            chkCalActiva.IsChecked = (limites[ejeSel].Cal == 1);
            txtI.Text = limites[ejeSel].Izq.ToString();
            txtC.Text = limites[ejeSel].Cen.ToString();
            txtN.Text = limites[ejeSel].Nulo.ToString();
            txtD.Text = limites[ejeSel].Der.ToString();
            chkAntivActiva.IsChecked = (jitter[ejeSel].Antiv == 1);
            txtMargen.Text = jitter[ejeSel].Margen.ToString();
            txtResistencia.Text = jitter[ejeSel].Resistencia.ToString();
        }

        private void Fchk_Cambiado(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
                btAplicar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x84, 0x2F));
        }

        private void Ftxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
                btAplicar.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x84, 0x2F));
        }
    }
}
