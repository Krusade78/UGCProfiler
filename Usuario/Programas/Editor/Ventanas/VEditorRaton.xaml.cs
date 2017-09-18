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
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal partial class VEditorRaton : Window
    {
        public VEditorRaton()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown1.Value = ((MainWindow)this.Owner).GetDatos().Perfil.GENERAL[0].TickRaton;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Owner).GetDatos().Perfil.GENERAL[0].TickRaton = (byte)NumericUpDown1.Value;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
