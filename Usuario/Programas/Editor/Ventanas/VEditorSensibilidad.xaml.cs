using System;
using System.Windows;
using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorSensibilidad.xaml
    /// </summary>
    internal partial class VEditorSensibilidad : Window
    {
        private MainWindow padre;
        private byte eje;

        public VEditorSensibilidad(byte eje)
        {
            InitializeComponent();
            this.eje = eje;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            padre = (MainWindow)this.Owner;
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(eje, m, p);
            TrackBar1.Value = r.Sensibilidad[0];
            TrackBar2.Value = r.Sensibilidad[1];
            TrackBar3.Value = r.Sensibilidad[2];
            TrackBar4.Value = r.Sensibilidad[3];
            TrackBar5.Value = r.Sensibilidad[4];
            TrackBar6.Value = r.Sensibilidad[5];
            TrackBar7.Value = r.Sensibilidad[6];
            TrackBar8.Value = r.Sensibilidad[7];
            TrackBar9.Value = r.Sensibilidad[8];
            TrackBar10.Value = r.Sensibilidad[9];
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            DSPerfil.MAPAEJESRow r = padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(eje, m, p);
            r.Sensibilidad[0] = (byte)TrackBar1.Value;
            r.Sensibilidad[1] = (byte)TrackBar2.Value;
            r.Sensibilidad[2] = (byte)TrackBar3.Value;
            r.Sensibilidad[3] = (byte)TrackBar4.Value;
            r.Sensibilidad[4] = (byte)TrackBar5.Value;
            r.Sensibilidad[5] = (byte)TrackBar6.Value;
            r.Sensibilidad[6] = (byte)TrackBar7.Value;
            r.Sensibilidad[7] = (byte)TrackBar8.Value;
            r.Sensibilidad[8] = (byte)TrackBar9.Value;
            r.Sensibilidad[9] = (byte)TrackBar10.Value;

            this.DialogResult = true;
            this.Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TrackBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((Slider)sender).Value = Math.Round(e.NewValue);
        }
    }
}
