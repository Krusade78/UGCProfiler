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
        private byte joySel = 0;
        private byte ejeSel = 0;
        private readonly Comunes.CTipos.STJITTER[,] jitter = new Comunes.CTipos.STJITTER[4,8];
        private readonly Comunes.CTipos.STLIMITES[,] limites = new Comunes.CTipos.STLIMITES[4,8];
        private static readonly short[,] mapaRangos = {
                { 0,0,0,255,0,0,63,63 },
                { 1023,1023,0,511,0,0,0,0 },
                { 0,0,127,127,127,127,7,7 },
                { 32767,32767,32767,32767,32767,32767,127,127 }
        };
        private short[,] hidReport = new short[4, 8];

        public HIDCal()
        {
            InitializeComponent();

            for (byte j = 0; j < 4; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    limites[j,i].Cal = 0;
                    limites[j,i].Cen = 0;
                    limites[j,i].Izq = -32767;
                    limites[j,i].Der = 32767;
                    jitter[j,i].Antiv = 0;
                    jitter[j,i].Margen = 0;
                    jitter[j,i].Resistencia = 0;
                }
            }

            using (Comunes.DataSetConfiguracion dsc = new Comunes.DataSetConfiguracion())
            {
                try
                {
                    dsc.ReadXml("configuracion.dat");
                    for (byte j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            limites[j,i].Cen = dsc.CALIBRADO_LIMITES[(j * 8) + i].Cen;
                            limites[j,i].Izq = dsc.CALIBRADO_LIMITES[(j * 8) + i].Izq;
                            limites[j,i].Der = dsc.CALIBRADO_LIMITES[(j * 8) + i].Der;
                            limites[j,i].Nulo = dsc.CALIBRADO_LIMITES[(j * 8) + i].Nulo;
                            limites[j,i].Cal = dsc.CALIBRADO_LIMITES[(j * 8) + i].Cal;
                            jitter[j,i].Margen = dsc.CALIBRADO_JITTER[(j * 8) + i].Margen;
                            jitter[j,i].Resistencia = dsc.CALIBRADO_JITTER[(j * 8) + i].Resistencia;
                            jitter[j,i].Antiv = dsc.CALIBRADO_JITTER[(j * 8) + i].Antiv;
                        }
                    }
                }
                catch (System.IO.FileNotFoundException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            CargarTextosEje(0, 3);
        }

        public void ActualizarEstado(byte[] hidData, byte joy, bool sinReport)
        {
            if (!sinReport)
            {
                for (byte i = 0; i < 8; i++)
                {
                    hidReport[joy - 1, i] = (short)(((int)hidData[(i * 2) + 1] << 8) | hidData[i * 2]);
                }
                if (joy == 2)
                {
                    for (byte i = 0; i < 8; i++)
                    {
                        hidReport[3, i] = (short)(((int)hidData[(i * 2) + 1] << 8) | hidData[i * 2]);
                    }
                }
            }

            int posr = hidReport[joySel, ejeSel];
            txtPosReal.Text = posr.ToString();
            posReal.Margin = new Thickness(posr + mapaRangos[joySel, ejeSel], 0, 0, 0);

            // Filtrado de ejes
            long pollEje = hidReport[joySel, ejeSel];

            if (jitter[joySel, ejeSel].Antiv == 1)
            {
                // Antivibraciones
                if ((pollEje < (jitter[joySel, ejeSel].PosElegida - jitter[joySel, ejeSel].Margen)) || (pollEje > (jitter[joySel, ejeSel].PosElegida + jitter[joySel, ejeSel].Margen)))
                {
                    jitter[joySel, ejeSel].PosRepetida = 0;
                    jitter[joySel, ejeSel].PosElegida = (short)pollEje;
                }
                else
                {
                    if (jitter[joySel, ejeSel].PosRepetida < jitter[joySel, ejeSel].Resistencia)
                    {
                        jitter[joySel, ejeSel].PosRepetida++;
                        pollEje = jitter[joySel, ejeSel].PosElegida;
                    }
                    else
                    {
                        jitter[joySel, ejeSel].PosRepetida = 0;
                        jitter[joySel, ejeSel].PosElegida = (short)pollEje;
                    }
                }
            }

            if (limites[joySel, ejeSel].Cal == 1)
            {
                // Calibrado
                short ancho1, ancho2;
                ancho1 = (short)(-limites[joySel, ejeSel].Izq +(limites[joySel, ejeSel].Cen - limites[joySel, ejeSel].Nulo));
                ancho2 = (short)(limites[joySel, ejeSel].Der - (limites[joySel, ejeSel].Cen + limites[joySel, ejeSel].Nulo));
                if (((pollEje >= (limites[joySel, ejeSel].Cen - limites[joySel, ejeSel].Nulo)) && (pollEje <= (limites[joySel, ejeSel].Cen + limites[joySel, ejeSel].Nulo))))
                {
                    //Zona nula
                    pollEje = 0;
                }
                else
                {
                    if (pollEje < limites[joySel, ejeSel].Izq)
                        pollEje = limites[joySel, ejeSel].Izq;
                    if (pollEje > limites[joySel, ejeSel].Der)
                        pollEje = limites[joySel, ejeSel].Der;

                    if (pollEje < limites[joySel, ejeSel].Cen)
                    {
                        pollEje -= (limites[joySel, ejeSel].Cen - limites[joySel, ejeSel].Nulo);
                        if (ancho1 != mapaRangos[joySel, ejeSel])
                        {
                            pollEje = (pollEje * mapaRangos[joySel, ejeSel]) / ancho1;
                        }
                    }
                    else
                    {
                        pollEje -= (limites[joySel, ejeSel].Cen + limites[joySel, ejeSel].Nulo);
                        if (ancho2 != mapaRangos[joySel, ejeSel])
                            pollEje = ((pollEje * mapaRangos[joySel, ejeSel]) / ancho2);
                    }
                }
            }

            txtPosCal.Text = pollEje.ToString();
            posCal.Margin = new Thickness(pollEje + mapaRangos[joySel, ejeSel], 0, 0, 0);
        }

        private void FbtAplicar_Click(object sender, RoutedEventArgs e)
        {
            limites[joySel, ejeSel].Cal = (chkCalActiva.IsChecked == true) ? (byte)1 : (byte)0;
            short.TryParse(txtI.Text, out short s);
            limites[joySel, ejeSel].Izq = s;
            short.TryParse(txtC.Text, out s);
            limites[joySel, ejeSel].Cen = s;
            short.TryParse(txtD.Text, out s);
            limites[joySel, ejeSel].Der = s;
            byte.TryParse(txtN.Text, out byte b);
            limites[joySel, ejeSel].Nulo = b;

            jitter[joySel, ejeSel].Antiv = (chkAntivActiva.IsChecked == true) ? (byte)1 : (byte)0;
            byte.TryParse(txtMargen.Text, out b);
            jitter[joySel, ejeSel].Margen = b;
            byte.TryParse(txtResistencia.Text, out b);
            jitter[joySel, ejeSel].Resistencia = b;

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
                for (byte j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        dsc.CALIBRADO_LIMITES.AddCALIBRADO_LIMITESRow(limites[j, i].Cal, limites[j, i].Nulo, limites[j, i].Izq, limites[j, i].Cen, limites[j, i].Der);
                        dsc.CALIBRADO_JITTER.AddCALIBRADO_JITTERRow(jitter[j, i].Antiv, jitter[j, i].Margen, jitter[j, i].Resistencia);
                    }
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

            using (System.IO.Pipes.NamedPipeClientStream pipeClient = new System.IO.Pipes.NamedPipeClientStream("LauncherPipe"))
            {
                try
                {
                    pipeClient.Connect(1000);
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(pipeClient))
                    {
                        sw.WriteLine("CCAL");
                        sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        #region "Toggles"
        private void FtbX_Checked(object sender, RoutedEventArgs e)
        {
            tbY.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            tbRy.IsChecked = false;
            tbRz.IsChecked = false;
            tbSl1.IsChecked = false;
            tbSl2.IsChecked = false;
            CargarTextosEje(joySel, 0);
        }

        private void FtbY_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            tbRy.IsChecked = false;
            tbRz.IsChecked = false;
            tbSl1.IsChecked = false;
            tbSl2.IsChecked = false;
            CargarTextosEje(joySel, 1);
        }

        private void FtbR_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                tbX.IsChecked = false;
                tbZ.IsChecked = false;
                tbY.IsChecked = false;
                tbRy.IsChecked = false;
                tbRz.IsChecked = false;
                tbSl1.IsChecked = false;
                tbSl2.IsChecked = false;
                CargarTextosEje(joySel, 3);
            }
        }

        private void FtbZ_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbY.IsChecked = false;
            tbR.IsChecked = false;
            tbRy.IsChecked = false;
            tbRz.IsChecked = false;
            tbSl1.IsChecked = false;
            tbSl2.IsChecked = false;
            CargarTextosEje(joySel, 2);
        }

        private void FtbRy_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbY.IsChecked = false;
            tbR.IsChecked = false;
            tbZ.IsChecked = false;
            tbRz.IsChecked = false;
            tbSl1.IsChecked = false;
            tbSl2.IsChecked = false;
            CargarTextosEje(joySel, 4);
        }
        private void FtbRz_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbY.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            tbRy.IsChecked = false;
            tbSl1.IsChecked = false;
            tbSl2.IsChecked = false;
            CargarTextosEje(joySel, 5);
        }
        private void FtbSl1_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbY.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            tbRy.IsChecked = false;
            tbRz.IsChecked = false;
            tbSl2.IsChecked = false;
            CargarTextosEje(joySel, 6);

        }
        private void FtbSl2_Checked(object sender, RoutedEventArgs e)
        {
            tbX.IsChecked = false;
            tbY.IsChecked = false;
            tbZ.IsChecked = false;
            tbR.IsChecked = false;
            tbRy.IsChecked = false;
            tbRz.IsChecked = false;
            tbSl1.IsChecked = false;
            CargarTextosEje(joySel, 7);
        }
        #endregion

        private void CargarTextosEje(byte joy, byte eje)
        {
            joySel = joy;
            ejeSel = eje;
            grBruto.Width = (mapaRangos[joy, eje] * 2) + 1;
            posReal.Width = ((grBruto.Width + 1) * 2) / 1024;
            grCal.Width = grBruto.Width;
            posCal.Width = posReal.Width;
            chkCalActiva.IsChecked = (limites[joySel, ejeSel].Cal == 1);
            txtI.Text = limites[joySel, ejeSel].Izq.ToString();
            txtC.Text = limites[joySel, ejeSel].Cen.ToString();
            txtN.Text = limites[joySel, ejeSel].Nulo.ToString();
            txtD.Text = limites[joySel, ejeSel].Der.ToString();
            chkAntivActiva.IsChecked = (jitter[joySel, ejeSel].Antiv == 1);
            txtMargen.Text = jitter[joySel, ejeSel].Margen.ToString();
            txtResistencia.Text = jitter[joySel, ejeSel].Resistencia.ToString();
            ActualizarEstado(null, joy, true);
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

        #region "Toggles Joy"
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                joySel = 0;
                tbJoy2.IsChecked = false;
                tbJoy3.IsChecked = false;
                tbJoy4.IsChecked = false;
                tbX.IsEnabled = false;
                tbY.IsEnabled = false;
                tbZ.IsEnabled = false;
                tbR.IsEnabled = true;
                tbRy.IsEnabled = false;
                tbRz.IsEnabled = false;
                tbSl1.IsEnabled = true;
                tbSl2.IsEnabled = true;
                tbR.IsChecked = true;
                FtbR_Checked(null, null);
            }
        }
        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            joySel = 1;
            tbJoy1.IsChecked = false;
            tbJoy3.IsChecked = false;
            tbJoy4.IsChecked = false;
            tbX.IsEnabled = true;
            tbY.IsEnabled = true;
            tbZ.IsEnabled = false;
            tbR.IsEnabled = true;
            tbRy.IsEnabled = false;
            tbRz.IsEnabled = false;
            tbSl1.IsEnabled = false;
            tbSl2.IsEnabled = false;
            tbX.IsChecked = true;
            FtbX_Checked(null, null);
        }
        private void ToggleButton_Checked_2(object sender, RoutedEventArgs e)
        {
            joySel = 2;
            tbJoy2.IsChecked = false;
            tbJoy1.IsChecked = false;
            tbJoy4.IsChecked = false;
            tbX.IsEnabled = false;
            tbY.IsEnabled = false;
            tbZ.IsEnabled = true;
            tbR.IsEnabled = true;
            tbRy.IsEnabled = true;
            tbRz.IsEnabled = true;
            tbSl1.IsEnabled = true;
            tbSl2.IsEnabled = true;
            tbZ.IsChecked = true;
            FtbZ_Checked(null, null);
        }
        private void ToggleButton_Checked_3(object sender, RoutedEventArgs e)
        {
            joySel = 3;
            tbJoy2.IsChecked = false;
            tbJoy3.IsChecked = false;
            tbJoy1.IsChecked = false;
            tbX.IsEnabled = true;
            tbY.IsEnabled = true;
            tbZ.IsEnabled = true;
            tbR.IsEnabled = true;
            tbRy.IsEnabled = true;
            tbRz.IsEnabled = true;
            tbSl1.IsEnabled = true;
            tbSl2.IsEnabled = true;
            tbX.IsChecked = true;
            FtbX_Checked(null, null);
        }
        #endregion
    }
}
