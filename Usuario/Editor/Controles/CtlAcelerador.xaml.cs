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
            Vista.Ver(2, CEnums.Tipo.Eje, (String)ButtonZ.Content);
        }

        #region "Seta 3"
        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Seta, "Seta 1 Norte");
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Seta, "Seta 1 Noreste");
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.Seta, "Seta 1 Este");
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Seta, "Seta 1 Sureste");
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.Tipo.Seta, "Seta 1 Sur");
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Seta, "Seta 1 Suroeste");
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.Tipo.Seta, "Seta 1 Oeste");
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.Tipo.Seta, "Seta 1 Noroeste");
        }
        #endregion

        #region "Seta 4"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.Tipo.Seta, "Seta 2 Norte");
        }

        private void Buttonp22_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.Tipo.Seta, "Seta 2 Noreste");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.Tipo.Seta, "Seta 2 Este");
        }

        private void Buttonp24_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.Tipo.Seta, "Seta 2 Sureste");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.Tipo.Seta, "Seta 2 Sur");
        }

        private void Buttonp26_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.Tipo.Seta, "Seta 2 Suroeste");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.Tipo.Seta, "Seta 2 Oeste");
        }

        private void Buttonp28_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.Tipo.Seta, "Seta 2 Noroeste");
        }
        #endregion

        #region "botones"
        private void Buttond_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Boton, (String)Buttond.Content);
        }

        private void Buttone_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Boton, (String)Buttone.Content);
        }

        private void Buttoni_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Boton, (String)Buttoni.Content);
        }
        #endregion

        #region "rueda"
        private void ButtonWup_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.Tipo.Boton, (String)ButtonWup.Content);
        }

        private void ButtonWb_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.Tipo.Boton, (String)ButtonWb.Content);
        }

        private void ButtonWdown_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.Tipo.Boton, (String)ButtonWdown.Content);
        }
        #endregion

        #region "botones mfd"
        private void ButtonMfd3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.Tipo.Boton, (String)ButtonMfd3.Content);
        }

        private void ButtonMfd2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Boton, (String)ButtonMfd2.Content);
        }

        private void ButtonMfd1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.Boton, (String)ButtonMfd1.Content);
        }
        #endregion

        #region "ministick"
        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Eje, (String)ButtonMinix.Content);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Eje, (String)ButtonMiniy.Content);
        }

        private void ButtonMouse_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.Tipo.Boton, (String)ButtonMouse.Content);
        }
        #endregion

        #region "ejes"
        private void ButtonRy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.Tipo.Eje, (String)ButtonRy.Content);
        }

        private void ButtonRx_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Eje, (String)ButtonRx.Content);
        }

        private void Buttonsl_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.Tipo.Eje, (String)Buttonsl.Content);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.Eje, (String)ButtonZ.Content);
        }
        #endregion
    }
}
