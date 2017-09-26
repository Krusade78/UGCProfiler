using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorSensibilidad.xaml
    /// </summary>
    internal partial class VEditorBandas : Window
    {
        private MainWindow padre;
        private byte eje;
        private CEnums.Tipo tipo;
        private byte[] bandas = new byte[15];

        public VEditorBandas(byte eje, CEnums.Tipo tipo)
        {
            InitializeComponent();
            this.eje = eje;
            this.tipo = tipo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            padre = (MainWindow)App.Current.MainWindow;
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            //if (tipo == CEnums.Tipo.Eje)
            //{
            //    DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(eje, m, p);
            //    bandas = r.Bandas;
            //}
            //else
            //{
            //    DSPerfil.MAPAEJESPEQUERow r = padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(eje, m, p);
            //    bandas = r.Bandas;
            //}
            int n = 1;
            foreach (byte b in bandas)
            {
                if (b == 0)
                    break;
                else
                    n++;
            }
            numBandas.Value = n;
        }

        private void numBandas_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (eventos)
                CambiarBandas();
        }

        private void lbl1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (eventos & this.IsLoaded)
            {
                //double tam = b1.Height + b2.Height;
                //b1.Height = new GridLength(double.Parse(((TextBox)e.OriginalSource).Text), GridUnitType.Star);
                //b2.Height = new GridLength(tam - b1.Height, GridUnitType.Star);
            }
        }



        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(eje, m, p);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        bool eventos = true;
        private void gs1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (!CambiarSplitter())
                e.Handled = true;
        }

        private bool CambiarSplitter()
        {
            if (numBandas.Value == 1)
                return true;

            CtlNumUpDown[] ctls = new CtlNumUpDown[] { lbl1, lbl2, lbl3, lbl4, lbl5, lbl6, lbl7, lbl8, lbl9, lbl10, lbl11, lbl12, lbl13, lbl14, lbl15 };
            eventos = false;
            int tam = (int)grb.RowDefinitions[0].Height.Value;
            if (tam < 1)
                return false;
            for (int i = 0; i < ctls.Length; i++)
            {
                if ((int)grb.RowDefinitions[i * 2].Height.Value < 1)
                    return false;
                if (i > (numBandas.Value - 2))
                    break;
                ctls[i].Value = tam;
                bandas[i] = (byte)tam;
                tam += (int)grb.RowDefinitions[(i * 2) + 2].Height.Value;
            }
            eventos = true;
            return true;
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
                    if (bandas[i] >= 99)
                        numBandas.Value = i + 2;
                    else if (bandas[i] == 0)
                        bandas[i] = (i == 0) ? (byte)50 : (byte)(bandas[i - 1] + ((100 - bandas[i - 1]) / 2));
                }
            }

            grb.Height = 100;
            grb.RowDefinitions.Clear();
            grb.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            int tam = 0;
            for (int i = 0; i < 15; i++)
            {
                if (bandas[i] != 0)
                {
                    grb.RowDefinitions[grb.RowDefinitions.Count - 1].Height = new GridLength(bandas[i] - tam, GridUnitType.Star);
                    tam = bandas[i];
                    grb.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
                    grb.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100 - bandas[i], GridUnitType.Star) });
                }
                ctls[i].IsEnabled = (bandas[i] != 0) ? true : false;
                if (bandas[i] != 0) grb.Height += 1;
                gs[i].Visibility = (bandas[i] != 0) ? Visibility.Visible : Visibility.Collapsed;
                gr[i].Visibility = (bandas[i] != 0) ? Visibility.Visible : Visibility.Collapsed;
            }
            eventos = true;
        }
    }
}
