using System;
using System.Windows;
using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlJoystick.xaml
    /// </summary>
    internal partial class CtlNXTJoystick : UserControl
    {
        private readonly CtlPropiedades Vista;

        public CtlNXTJoystick(CtlEditar padre)
        {
            InitializeComponent();
            Vista = padre.ctlPropiedades;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Eje, (String)ButtonX.Content);
        }

        #region "Seta 1"
        private void Buttonp10_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.Tipo.Boton, "Seta 1 Centro");
        }

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

        #region "Seta 2"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.Tipo.Seta, "Seta 2 Norte");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.Tipo.Seta, "Seta 2 Este");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.Tipo.Seta, "Seta 2 Sur");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.Tipo.Seta, "Seta 2 Oeste");
        }

        #endregion

        #region "Seta 3"
        private void Buttonp31_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(16, CEnums.Tipo.Seta, "Seta 3 Norte");
        }

        private void Buttonp33_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(18, CEnums.Tipo.Seta, "Seta 3 Este");
        }

        private void Buttonp35_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(20, CEnums.Tipo.Seta, "Seta 3 Sur");
        }

        private void Buttonp37_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(22, CEnums.Tipo.Seta, "Seta 3 Oeste");
        }
        #endregion

        #region "Seta 4"
        private void Buttonp40_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.Tipo.Boton, "Seta 4 Centro");
        }

        private void Buttonp41_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(24, CEnums.Tipo.Seta, "Seta 4 Norte");
        }

        private void Buttonp43_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(26, CEnums.Tipo.Seta, "Seta 4 Este");
        }

        private void Buttonp45_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(28, CEnums.Tipo.Seta, "Seta 4 Sur");
        }

        private void Buttonp47_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(30, CEnums.Tipo.Seta, "Seta 4 Oeste");
        }
        #endregion

        #region "botones"
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.Tipo.Boton, (String)Button1.Content);
        }

        private void ButtonS1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.Tipo.Boton, (String)ButtonS1.Content);
        }

        private void ButtonLanzar_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.Tipo.Boton, (String)ButtonLanzar.Content);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.Tipo.Boton, (String)ButtonTrigger1.Content);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.Tipo.Boton, (String)ButtonTrigger2.Content);
        }
        #endregion

        #region "modos"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.Tipo.Boton, (String)ButtonPinkie.Content);
        }

        private void ButtonBase1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.Tipo.Boton, (String)ButtonBase1.Content);
        }

        private void ButtonBase2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Boton, (String)ButtonBase2.Content);
        }

        private void ButtonBase3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.Tipo.Boton, (String)ButtonBase3.Content);
        }
        #endregion

        #region "ejes"
        private void ButtonX_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Eje, (String)ButtonX.Content);
        }

        private void ButtonY_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Eje, (String)ButtonY.Content);
        }

        private void ButtonR_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Eje, (String)ButtonR.Content);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.Eje, (String)ButtonZ.Content);
        }

        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.Tipo.Eje, (String)ButtonMinix.Content);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.Tipo.Eje, (String)ButtonMiniy.Content);
        }
        #endregion

        #region "encoders"
        private void ButtonE1Ar_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Boton, (String)ButtonEnc1Ar.Content);
        }

        private void ButtonE1Ab_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Boton, (String)ButtonEnc1Ab.Content);
        }

        private void ButtonE2Ar_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.Boton, (String)ButtonEnc2Ar.Content);
        }

        private void ButtonE2Ab_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Boton, (String)ButtonEnc2Ab.Content);
        }
        #endregion
    }
}
