using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para HIDCal.xaml
    /// </summary>
    internal partial class HIDCal : UserControl
    {
        private uint joySel = 0;
        private byte ejeSel = 0;
        private readonly Dictionary<uint, Dictionary<byte, Comunes.CTipos.STJITTER>> jitter = new();
        private readonly Dictionary<uint, Dictionary<byte, Comunes.CTipos.STLIMITES>> limites = new();

        private readonly ushort[] hidReport = new ushort[8];

        private readonly Dictionary<uint, DatosJoy> dispositivos = new();

        public HIDCal()
        {
            InitializeComponent();

            try
            {
                Comunes.Calibrado.CCalibrado cal = System.Text.Json.JsonSerializer.Deserialize<Comunes.Calibrado.CCalibrado>(System.IO.File.ReadAllText("configuracion.dat"));
                foreach (Comunes.Calibrado.Limites r in cal.Limites)
                {
                    Comunes.CTipos.STLIMITES l = new()
                    {
                        Cen = r.Cen,
                        Izq = r.Izq,
                        Der = r.Der,
                        Nulo = r.Nulo,
                        Cal = r.Cal,
                        Rango = r.Rango,
                    };
                    if (!limites.ContainsKey(r.IdJoy))
                    {
                        limites.Add(r.IdJoy, new() { { r.IdEje, l } });
                    }
                    else
                    {
                        limites[r.IdJoy].Add(r.IdEje, l);
                    }
                }
                foreach (Comunes.Calibrado.Jitter r in cal.Jitters)
                {
                    Comunes.CTipos.STJITTER j = new()
                    {
                        Margen = r.Margen,
                        Resistencia = r.Resistencia,
                        Antiv = r.Antiv
                    };
                    if (!jitter.ContainsKey(r.IdJoy))
                    {
                        jitter.Add(r.IdJoy, new() { { r.IdEje, j } });
                    }
                    else
                    {
                        jitter[r.IdJoy].Add(r.IdEje, j);
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

        public void ActualizarEstado(string ninterface, byte[] hidData, uint joy)
        {
            if (!dispositivos.ContainsKey(joy))
            {
                DatosJoy nuevo = DatosJoy.GetInfo(ninterface, joy);
                dispositivos.Add(joy, nuevo);
                lDevices.Items.Add(nuevo);
                if (!limites.ContainsKey(joy))
                {
                    limites.Add(joy, new());
                    jitter.Add(joy, new());
                    foreach (DatosJoy.CUsage u in nuevo.Usages)
                    {
                        if (u.Eje < 254)
                        {
                            limites[joy].Add(u.Eje, new() { Cen = (ushort)(u.Rango /2), Rango = u.Rango });
                            jitter[joy].Add(u.Eje, new());
                        }
                    }
                }
                else
                {
                    foreach (DatosJoy.CUsage u in nuevo.Usages)
                    {
                        if (u.Eje < 254)
                        {
                            Comunes.CTipos.STLIMITES st = limites[joy][u.Eje];
                            st.Rango = u.Rango;
                            limites[joy][u.Eje] = st;
                        }
                    }
                }
            }
            if (joySel != joy)
            {
                return;
            }
            if (hidData !=  null)
            {
                uint botones = 0;
                short[] povs = new short[4];
                uint[] ejes = new uint[8];
                dispositivos[joySel].ToHiddata(hidData, ref ejes, ref botones, ref povs);
                for (byte i = 0; i < 8;  i++) { hidReport[i] = (ushort)ejes[i]; }
            }

            int posr = hidReport[ejeSel];
            txtPosReal.Text = posr.ToString();
            posReal.Margin = new Thickness(posr, 0, 0, 0);

            // Filtrado de ejes
            ushort pollEje = hidReport[ejeSel];

            if (jitter[joySel][ejeSel].Antiv == 1)
            {
                // Antivibraciones
                jitter[joySel].TryGetValue(ejeSel, out Comunes.CTipos.STJITTER v);
                //if ((pollEje == v.PosElegida) || (pollEje < (v.PosElegida - v.Margen)) || (pollEje > (v.PosElegida + v.Margen)))
                //{
                //    v.PosRepetida = 0;
                //    v.PosElegida = pollEje;
                //    v.PosPosible = pollEje;
                //    jitter[joySel][ejeSel] = v;
                //}
                //else
                {
                    if (pollEje == v.PosPosible)
                    {
                        v.PosRepetida++;
                        if (v.PosRepetida == v.Resistencia)
                        {
                            v.PosRepetida = 0;
                            v.PosElegida = pollEje;
                            jitter[joySel][ejeSel] = v;
                        }
                    }
                    else
                    {
                        v.PosRepetida = 0;
                        v.PosPosible = pollEje;
                        jitter[joySel][ejeSel] = v;
                    }
                    pollEje = v.PosElegida;
                }
            }

            if (limites[joySel][ejeSel].Cal == 1)
            {
                // Calibrado
                ushort ancho1, ancho2;
                ancho1 = (ushort)((limites[joySel][ejeSel].Cen - limites[joySel][ejeSel].Nulo) - limites[joySel][ejeSel].Izq);
                ancho2 = (ushort)(limites[joySel][ejeSel].Der - (limites[joySel][ejeSel].Cen + limites[joySel][ejeSel].Nulo));
                if (((pollEje >= (limites[joySel][ejeSel].Cen - limites[joySel][ejeSel].Nulo)) && (pollEje <= (limites[joySel][ejeSel].Cen + limites[joySel][ejeSel].Nulo))))
                {
                    //Zona nula
                    pollEje = limites[joySel][ejeSel].Cen;
                }
                else
                {
                    if (pollEje < limites[joySel][ejeSel].Izq)
                        pollEje = limites[joySel][ejeSel].Izq;
                    if (pollEje > limites[joySel][ejeSel].Der)
                        pollEje = limites[joySel][ejeSel].Der;

                    if (pollEje < limites[joySel][ejeSel].Cen)
                    {
                        if (ancho1 != limites[joySel][ejeSel].Cen)
                        {
                            if (pollEje >= ancho1) { pollEje = ancho1; }
                            pollEje -= limites[joySel][ejeSel].Izq;
                            pollEje = (ushort)((pollEje * limites[joySel][ejeSel].Cen) / ancho1);
                        }
                    }
                    else
                    {
                        if (ancho2 != (limites[joySel][ejeSel].Rango - limites[joySel][ejeSel].Cen))
                        {
                            if (pollEje >= limites[joySel][ejeSel].Der) { pollEje = limites[joySel][ejeSel].Der; }
                            pollEje -= (ushort)(limites[joySel][ejeSel].Cen + limites[joySel][ejeSel].Nulo);
                            pollEje = (ushort)(limites[joySel][ejeSel].Cen + ((pollEje * (limites[joySel][ejeSel].Rango - limites[joySel][ejeSel].Cen)) / ancho2));
                        }
                    }
                }
            }

            txtPosCal.Text = pollEje.ToString();
            posCal.Margin = new Thickness(pollEje, 0, 0, 0);
        }

        private void FbtSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (lDevices.SelectedIndex == -1)
            {
                tbJoy2.Visibility = Visibility.Collapsed;
                return;
            }

            DatosJoy sel = (DatosJoy)lDevices.SelectedItem;

            tbJoy2.Visibility = Visibility.Visible;
            tbJoy2.Content = sel.Nombre;

            System.Windows.Controls.Primitives.ToggleButton[] btEjes = { tbX, tbY, tbZ, tbR, tbRy, tbRz, tbSl1, tbSl2 };

            joySel = sel.Id;
            ejeSel = 0;
            for (byte i = 0; i < btEjes.Length; i++) { btEjes[i].IsEnabled = false; }
            foreach (DatosJoy.CUsage usg in sel.Usages)
            {
                if (usg.Eje < 254)
                {
                    btEjes[usg.Eje].IsChecked = true;
                    ejeSel = usg.Eje;
                    break;
                }
            }

            foreach (DatosJoy.CUsage usg in sel.Usages)
            {
                if (usg.Eje < 254)
                {
                    btEjes[usg.Eje].IsEnabled = true;
                }
            }
            ToggleButton_Checked_1(null, null);
        }

        private void FbtAplicar_Click(object sender, RoutedEventArgs e)
        {
            Comunes.CTipos.STLIMITES l = limites[joySel][ejeSel];
            l.Cal = (chkCalActiva.IsChecked == true) ? (byte)1 : (byte)0;
            _ = ushort.TryParse(txtI.Text, out ushort s);
            l.Izq = s;
            _ = ushort.TryParse(txtRawC.Text, out s);
            l.Cen = s;
            _ = ushort.TryParse(txtD.Text, out s);
            l.Der = s;
            _ = byte.TryParse(txtN.Text, out byte b);
            l.Nulo = b;
            limites[joySel][ejeSel] = l;

            Comunes.CTipos.STJITTER j = jitter[joySel][ejeSel];
            j.Antiv = (chkAntivActiva.IsChecked == true) ? (byte)1 : (byte)0;
            _ = byte.TryParse(txtMargen.Text, out b);
            j.Margen = b;
            _ = byte.TryParse(txtResistencia.Text, out b);
            j.Resistencia = b;
            jitter[joySel][ejeSel] = j;

            btAplicar.Foreground = System.Windows.Media.Brushes.GreenYellow;
        }

        private void FbtGuardar_Click(object sender, RoutedEventArgs e)
        {
            FbtAplicar_Click(null, null);
            {
                Comunes.Calibrado.CCalibrado dsc = new();

                foreach (var v in limites)
                {                    
                    foreach (var l in v.Value)
                    {
                        dsc.Limites.Add(new() { IdJoy = v.Key, IdEje = l.Key, Cal =l.Value.Cal, Nulo = l.Value.Nulo, Izq = l.Value.Izq, Cen = l.Value.Cen, Der = l.Value.Der, Rango = l.Value.Rango });
                    }
                }
                foreach (var v in jitter)
                {
                    foreach (var j in v.Value)
                    {
                        dsc.Jitters.Add(new() { IdJoy = v.Key, IdEje = j.Key, Antiv = j.Value.Antiv, Margen = j.Value.Margen, Resistencia = j.Value.Resistencia });
                    }
                }

                try
                {
                    System.IO.File.WriteAllText("configuracion.dat", System.Text.Json.JsonSerializer.Serialize(dsc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            using System.IO.Pipes.NamedPipeClientStream pipeClient = new("LauncherPipe");
            try
            {
                pipeClient.Connect(1000);
                using System.IO.StreamWriter sw = new(pipeClient);
                sw.WriteLine("CCAL");
                sw.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        #region "Toggles"
        private void FtbX_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(0);
        }

        private void FtbY_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(1);
        }

        private void FtbR_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(3);
        }

        private void FtbZ_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(2);
        }

        private void FtbRy_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(4);
        }
        private void FtbRz_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(5);
        }
        private void FtbSl1_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(6);

        }
        private void FtbSl2_Checked(object sender, RoutedEventArgs e)
        {
            CargarTextosEje(7);
        }
        #endregion

        private void CargarTextosEje(byte eje)
        {
            System.Windows.Controls.Primitives.ToggleButton[] btEjes = { tbX, tbY, tbZ, tbR, tbRy, tbRz, tbSl1, tbSl2 };
            for (byte i = 0; i < btEjes.Length; i++)
            {
                if (eje != i)
                {
                    btEjes[i].IsChecked = false;
                }
            }
            ejeSel = eje;
            grBruto.Width = limites[joySel][ejeSel].Rango + 1;
            posReal.Width = ((grBruto.Width + 1) * 2) / 1024;
            grCal.Width = grBruto.Width;
            posCal.Width = posReal.Width;
            chkCalActiva.IsChecked = (limites[joySel][ejeSel].Cal == 1);
            txtI.Text = limites[joySel][ejeSel].Izq.ToString();
            txtN.Text = limites[joySel][ejeSel].Nulo.ToString();
            txtD.Text = limites[joySel][ejeSel].Der.ToString();
            txtCalcRawC.Text = (limites[joySel][ejeSel].Rango / 2).ToString();
            txtRawC.Text = limites[joySel][ejeSel].Cen.ToString();
            txtRawD.Text = limites[joySel][ejeSel].Rango.ToString();
            chkAntivActiva.IsChecked = (jitter[joySel][ejeSel].Antiv == 1);
            txtMargen.Text = jitter[joySel][ejeSel].Margen.ToString();
            txtResistencia.Text = jitter[joySel][ejeSel].Resistencia.ToString();
            ActualizarEstado(null, null, joySel);
        }

        private void ButtonCen_Click(object sender, RoutedEventArgs e)
        {
            txtRawC.Text = txtCalcRawC.Text;
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
                gCalibrado.Visibility = Visibility.Collapsed;
                gDispositivos.Visibility = Visibility.Visible;
                tbJoy2.IsChecked = false;
            }
        }
        private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
        {
            gCalibrado.Visibility = Visibility.Visible;
            gDispositivos.Visibility = Visibility.Collapsed;
            tbJoy1.IsChecked = false;
        }
        #endregion
    }
}
