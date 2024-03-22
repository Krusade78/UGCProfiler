using System;
using System.Windows;
using static Comunes.CTipos;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal partial class VCopiaDe : Window
    {
        private byte eje;

        public VCopiaDe(byte eje)
        {
            InitializeComponent();
            this.eje = eje;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Guardar();
            this.DialogResult = true;
            this.Close();
        }

        private void Guardar()
        {
            MainWindow padre = (MainWindow)App.Current.MainWindow;
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);

            for (byte i = 0; i < 10; i++)
            {
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, eje).Sensibilidad[i] = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, (byte)NumericUpDownP.Valor, (byte)(NumericUpDownM.Valor - 1), eje).Sensibilidad[i];
            }
            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, eje).Slider= padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, (byte)NumericUpDownP.Valor, (byte)(NumericUpDownM.Valor - 1), eje).Slider;
        }
    }
}
