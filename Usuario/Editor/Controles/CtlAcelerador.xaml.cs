using System;
using System.Windows;
using System.Windows.Controls;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlAcelerador.xaml
    /// </summary>
    internal partial class CtlAcelerador : UserControl
    {
        private readonly CtlPropiedades Vista;

        public CtlAcelerador(CtlEditar padre)
        {
            InitializeComponent();
            Vista = padre.ctlPropiedades;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Eje, (String)ButtonZ.Content);
        }

        #region "Seta 3"
        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(16, CEnums.Tipo.Seta, "Seta 3 Norte");
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(17, CEnums.Tipo.Seta, "Seta 3 Noreste");
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(18, CEnums.Tipo.Seta, "Seta 3 Este");
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(19, CEnums.Tipo.Seta, "Seta 3 Sureste");
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(20, CEnums.Tipo.Seta, "Seta 3 Sur");
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(21, CEnums.Tipo.Seta, "Seta 3 Suroeste");
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(22, CEnums.Tipo.Seta, "Seta 3 Oeste");
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(23, CEnums.Tipo.Seta, "Seta 3 Noroeste");
        }
        #endregion

        #region "Seta 4"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(24, CEnums.Tipo.Seta, "Seta 4 Norte");
        }

        private void Buttonp22_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(25, CEnums.Tipo.Seta, "Seta 4 Noreste");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(26, CEnums.Tipo.Seta, "Seta 4 Este");
        }

        private void Buttonp24_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(27, CEnums.Tipo.Seta, "Seta 4 Sureste");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(28, CEnums.Tipo.Seta, "Seta 4 Sur");
        }

        private void Buttonp26_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(29, CEnums.Tipo.Seta, "Seta 4 Suroeste");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(30, CEnums.Tipo.Seta, "Seta 4 Oeste");
        }

        private void Buttonp28_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(31, CEnums.Tipo.Seta, "Seta 4 Noroeste");
        }
        #endregion

        #region "botones"
        private void Buttond_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.Tipo.Boton, (String)Buttond.Content);
        }

        private void Buttone_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.Tipo.Boton, (String)Buttone.Content);
        }

        private void Buttoni_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.Tipo.Boton, (String)Buttoni.Content);
        }
        #endregion

        #region "rueda"
        private void ButtonWup_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(24, CEnums.Tipo.Boton, (String)ButtonWup.Content);
        }

        private void ButtonWb_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(23, CEnums.Tipo.Boton, (String)ButtonWb.Content);
        }

        private void ButtonWdown_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(25, CEnums.Tipo.Boton, (String)ButtonWdown.Content);
        }
        #endregion

        #region "botones mfd"
        private void ButtonMfd3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.Tipo.Boton, (String)ButtonMfd3.Content);
        }

        private void ButtonMfd2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.Tipo.Boton, (String)ButtonMfd2.Content);
        }

        private void ButtonMfd1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.Tipo.Boton, (String)ButtonMfd1.Content);
        }
        #endregion

        #region "ministick"
        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.EjeMini, (String)ButtonMinix.Content);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.EjeMini, (String)ButtonMiniy.Content);
        }

        private void ButtonMouse_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Boton, (String)ButtonMouse.Content);
        }
        #endregion

        #region "ejes"
        private void ButtonRy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.EjePeque, (String)ButtonRy.Content);
        }

        private void ButtonRx_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.EjePeque, (String)ButtonRx.Content);
        }

        private void Buttonsl_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.EjePeque, (String)Buttonsl.Content);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Eje, (String)ButtonZ.Content);
        }
        #endregion
    }
}
