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

        public VEditorBandas(byte eje)
        {
            InitializeComponent();
            this.eje = eje;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            padre = (MainWindow)App.Current.MainWindow;
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(eje, m, p);
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
        private void b1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded)
                CambiarSplitter(0, (int)b1.ActualHeight);
        }
        private void b2_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded)
                CambiarSplitter(1, (int)b2.ActualHeight);
        }

        private void CambiarSplitter(int idx, int valor)
        {
            CtlNumUpDown[] ctls = new CtlNumUpDown[] { lbl1, lbl2, lbl3, lbl4 };
            eventos = false;
            int total = valor;
            for (int i = 0; i < idx; i++)
                total += ctls[i].Value;
            ctls[idx].Value = total;
            eventos = true;
        }

        private void lbl1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (eventos & this.IsLoaded)
            {
                b1.Height = lbl1.Value;
                //b2.Height;
            }
        }
    }
}
