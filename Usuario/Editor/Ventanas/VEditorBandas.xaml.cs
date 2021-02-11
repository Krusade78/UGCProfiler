using System;
using System.Windows;
using System.Windows.Controls;
using Comunes;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorSensibilidad.xaml
    /// </summary>
    internal partial class VEditorBandas : Window
    {
        private readonly MainWindow padre;
        private readonly byte eje;
        private byte[] bandas;
        private bool eventos = true;

        public VEditorBandas(byte eje)
        {
            InitializeComponent();
            this.eje = eje;
            padre = (MainWindow)App.Current.MainWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);

            DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, eje);
            bandas = (byte[])r.Bandas.Clone();

            eventos = false;
            foreach (byte b in bandas)
            {
                if (b == 0)
                    break;
                else
                    numBandas.Value++;
            }
            eventos = true;
            CambiarBandas();
        }

        private void FnumBandas_TextChanged(object sender, EventArgs e)
        {
            if (eventos)
                CambiarBandas();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);

            DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, eje);
            r.Bandas = (byte[])bandas.Clone();

            this.DialogResult = true;
            this.Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Flbl_TextChanged(object sender, EventArgs e)
        {
            if (eventos & this.IsLoaded)
            {
                double d1 = (double)((CtlNumUpDown)sender).Value;
                int idc = lbls.Children.IndexOf((UIElement)sender);
                double d2 = Math.Round(grb.RowDefinitions[idc * 2].Height.Value) - d1;
                d2 += (idc != 14) ? ((CtlNumUpDown)lbls.Children[idc + 1]).Value : int.Parse(lbl16.Text);
                grb.RowDefinitions[idc * 2].Height = new GridLength(d1, GridUnitType.Star);
                grb.RowDefinitions[(idc * 2) + 2].Height = new GridLength(d2, GridUnitType.Star);
                CambiarSplitter((idc * 2) + 1);
            }
        }

        private void Fgs1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (!CambiarSplitter(Grid.GetRow((GridSplitter)sender)))
                e.Handled = true;
        }

        private bool CambiarSplitter(int idc)
        {
            if (numBandas.Value == 1)
                return true;

            bool ok = true;
            CtlNumUpDown[] ctls = new CtlNumUpDown[] { lbl1, lbl2, lbl3, lbl4, lbl5, lbl6, lbl7, lbl8, lbl9, lbl10, lbl11, lbl12, lbl13, lbl14, lbl15 };
            eventos = false;

            if ((int)grb.RowDefinitions[idc - 1].Height.Value < 1)
            {
                grb.RowDefinitions[idc - 1].Height = new GridLength(1, GridUnitType.Star);
                grb.RowDefinitions[idc + 1].Height = new GridLength(grb.RowDefinitions[idc + 1].Height.Value - 1, GridUnitType.Star);
                ok = false;
            }
            if ((int)grb.RowDefinitions[idc + 1].Height.Value < 1)
            {
                grb.RowDefinitions[idc + 1].Height = new GridLength(1, GridUnitType.Star);
                grb.RowDefinitions[idc - 1].Height = new GridLength(grb.RowDefinitions[idc - 1].Height.Value - 1, GridUnitType.Star);
                ok = false;
            }
            ctls[(idc - 1) / 2].Value = (int)Math.Round(grb.RowDefinitions[idc - 1].Height.Value);
            if (idc != 1)
                ctls[((idc - 1) / 2) - 1].Maximum = ctls[(idc - 1) / 2].Value - 1;
            bandas[(idc - 1) / 2] = (byte)(((idc == 1) ? 0 : bandas[((idc - 1) / 2) - 1]) + ctls[(idc - 1) / 2].Value);
            if (idc != 29)
                ctls[(idc + 1) / 2].Value = (int)Math.Round(grb.RowDefinitions[idc + 1].Height.Value);
            else
                lbl16.Text = ((int)Math.Round(grb.RowDefinitions[idc + 1].Height.Value)).ToString();

            eventos = true;
            return ok;
        }

        private void CambiarBandas()
        {
            CtlNumUpDown[] ctls = new CtlNumUpDown[] { lbl1, lbl2, lbl3, lbl4, lbl5, lbl6, lbl7, lbl8, lbl9, lbl10, lbl11, lbl12, lbl13, lbl14, lbl15 };
            Grid[] gr = new Grid[] { b2, b3, b4, b5, b6, b7, b8, b9, b10, b11, b12, b13, b14, b15, b16 };
            GridSplitter[] gs = new GridSplitter[] { gs1, gs2, gs3, gs4, gs5, gs6, gs7, gs8, gs9, gs10, gs11, gs12, gs13, gs14, gs15 };

            eventos = false;

            for (int i = 0; i < 15; i++)
            {
                if (i >= (numBandas.Value - 1))
                    bandas[i] = 0;
                else
                {
                    if (bandas[i] == 99)
                        numBandas.Value = i + 2;
                    else if (bandas[i] == 0)
                        bandas[i] = (i == 0) ? (byte)50 : (byte)(bandas[i - 1] + ((100 - bandas[i - 1]) / 2));
                }
            }

            grb.Height = 100;
            grb.RowDefinitions.Clear();
            grb.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            lbl1.Value = 100;
            int tam = 0;
            for (int i = 0; i < 15; i++)
            {
                if (bandas[i] != 0)
                {
                    grb.RowDefinitions[grb.RowDefinitions.Count - 1].Height = new GridLength(bandas[i] - tam, GridUnitType.Star);
                    grb.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
                    grb.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100 - bandas[i], GridUnitType.Star) });

                    ctls[i].Value = bandas[i] - tam;
                    if (i > 0)
                        ctls[i - 1].Maximum = ctls[i - 1].Value + ctls[i].Value - 1;
                    if (grb.RowDefinitions.Count != 31)
                        ctls[i + 1].Value = 100 - bandas[i];
                    else
                        lbl16.Text = (100 - bandas[i]).ToString();

                    tam = bandas[i];
                }
                ctls[i].IsEnabled = (bandas[i] != 0);
                if (bandas[i] != 0) grb.Height += 1;
                gs[i].Visibility = (bandas[i] != 0) ? Visibility.Visible : Visibility.Collapsed;
                gr[i].Visibility = (bandas[i] != 0) ? Visibility.Visible : Visibility.Collapsed;
            }
            eventos = true;
        }
    }
}
