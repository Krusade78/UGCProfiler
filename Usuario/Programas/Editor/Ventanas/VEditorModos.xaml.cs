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
    /// Lógica de interacción para VEditorModos.xaml
    /// </summary>
    public partial class VEditorModos : Window
    {
        MainWindow padre;

        public VEditorModos()
        {
            InitializeComponent();
            padre = (MainWindow)this.Owner;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (padre.GetDatos().Perfil.GENERAL[0].PinkieActivado) CheckBox1.IsChecked = true;
            if (padre.GetDatos().Perfil.GENERAL[0].ModosActivado) CheckBox2.IsChecked = true;
        }

        private void ButtonGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox1.IsChecked == true)
            {
                if (TextBox1.Text.Trim() == "") TextBox1.Text = "Pinkie";
                if ((TextBox1.Text.Trim() != TextBox2.Text.Trim()) && (TextBox1.Text.Trim() != TextBox3.Text.Trim())
                        && (TextBox1.Text.Trim() != TextBox4.Text.Trim()))
                    padre.GetDatos().Perfil.GENERAL[0].NombrePinkie = TextBox1.Text.Trim();
                else
                {
                    TextBox1.SelectAll();
                    return;
                }
            }
            if (CheckBox2.IsChecked == true)
            {
                if (TextBox2.Text.Trim() == "") TextBox2.Text = "Modo 1";
                if ((TextBox1.Text.Trim() != TextBox2.Text.Trim()) && (TextBox2.Text.Trim() != TextBox3.Text.Trim())
                        && (TextBox2.Text.Trim() != TextBox4.Text.Trim()))
                    padre.GetDatos().Perfil.GENERAL[0].NombreModo1 = TextBox2.Text.Trim();
                else
                {
                    TextBox2.SelectAll();
                    return;
                }
                if (TextBox3.Text.Trim() == "") TextBox3.Text = "Modo 2";
                if ((TextBox3.Text.Trim() != TextBox2.Text.Trim()) && (TextBox1.Text.Trim() != TextBox3.Text.Trim())
                        && (TextBox3.Text.Trim() != TextBox4.Text.Trim()) )
                    padre.GetDatos().Perfil.GENERAL[0].NombreModo1 = TextBox3.Text.Trim();
                else
                {
                    TextBox3.SelectAll();
                    return;
                }
                if (TextBox4.Text.Trim() == "") TextBox4.Text = "Modo 3";
                if ((TextBox4.Text.Trim() != TextBox1.Text.Trim()) && (TextBox4.Text.Trim() != TextBox2.Text.Trim())
                        && (TextBox4.Text.Trim() != TextBox3.Text.Trim()))
                    padre.GetDatos().Perfil.GENERAL[0].NombreModo1 = TextBox4.Text.Trim();
                else
                {
                    TextBox4.SelectAll();
                    return;
                }
            }
            padre.GetDatos().Perfil.GENERAL[0].PinkieActivado = (CheckBox1.IsChecked == true);
            padre.GetDatos().Perfil.GENERAL[0].ModosActivado = (CheckBox2.IsChecked == true);
            this.Close();
        }

        private void ButtonCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
