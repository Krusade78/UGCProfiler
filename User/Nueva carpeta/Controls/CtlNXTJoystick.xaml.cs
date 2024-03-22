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

        public CtlNXTJoystick(CtlPropiedades vista)
        {
            InitializeComponent();
            Vista = vista;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Axis, (String)ButtonX.Content);
        }

        #region "Seta 1"
        private void Buttonp10_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.ElementType.Button, "Seta 1 Centro");
        }

        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Hat, "Seta 1 Norte");
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Hat, "Seta 1 Noreste");
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Hat, "Seta 1 Este");
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Hat, "Seta 1 Sureste");
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.ElementType.Hat, "Seta 1 Sur");
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.ElementType.Hat, "Seta 1 Suroeste");
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Hat, "Seta 1 Oeste");
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.ElementType.Hat, "Seta 1 Noroeste");
        }
        #endregion

        #region "Seta 2"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.ElementType.Hat, "Seta 2 Norte");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.ElementType.Hat, "Seta 2 Este");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.ElementType.Hat, "Seta 2 Sur");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.ElementType.Hat, "Seta 2 Oeste");
        }

        #endregion

        #region "Seta 3"
        private void Buttonp31_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(16, CEnums.ElementType.Hat, "Seta 3 Norte");
        }

        private void Buttonp33_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(18, CEnums.ElementType.Hat, "Seta 3 Este");
        }

        private void Buttonp35_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(20, CEnums.ElementType.Hat, "Seta 3 Sur");
        }

        private void Buttonp37_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(22, CEnums.ElementType.Hat, "Seta 3 Oeste");
        }
        #endregion

        #region "Seta 4"
        private void Buttonp40_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.ElementType.Button, "Seta 4 Centro");
        }

        private void Buttonp41_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(24, CEnums.ElementType.Hat, "Seta 4 Norte");
        }

        private void Buttonp43_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(26, CEnums.ElementType.Hat, "Seta 4 Este");
        }

        private void Buttonp45_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(28, CEnums.ElementType.Hat, "Seta 4 Sur");
        }

        private void Buttonp47_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(30, CEnums.ElementType.Hat, "Seta 4 Oeste");
        }
        #endregion

        #region "botones"
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.ElementType.Button, (String)Button1.Content);
        }

        private void ButtonS1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.ElementType.Button, (String)ButtonS1.Content);
        }

        private void ButtonLanzar_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.ElementType.Button, (String)ButtonLanzar.Content);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.ElementType.Button, (String)ButtonTrigger1.Content);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.ElementType.Button, (String)ButtonTrigger2.Content);
        }
        #endregion

        #region "modos"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.ElementType.Button, (String)ButtonPinkie.Content);
        }

        private void ButtonBase1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.ElementType.Button, (String)ButtonBase1.Content);
        }

        private void ButtonBase2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.ElementType.Button, (String)ButtonBase2.Content);
        }

        private void ButtonBase3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Button, (String)ButtonBase3.Content);
        }
        #endregion

        #region "ejes"
        private void ButtonX_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Axis, (String)ButtonX.Content);
        }

        private void ButtonY_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Axis, (String)ButtonY.Content);
        }

        private void ButtonR_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Axis, (String)ButtonR.Content);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Axis, (String)ButtonZ.Content);
        }

        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Axis, (String)ButtonMinix.Content);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.ElementType.Axis, (String)ButtonMiniy.Content);
        }
        #endregion

        #region "encoders"
        private void ButtonE1Ar_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Button, (String)ButtonEnc1Ar.Content);
        }

        private void ButtonE1Ab_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Button, (String)ButtonEnc1Ab.Content);
        }

        private void ButtonE2Ar_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Button, (String)ButtonEnc2Ar.Content);
        }

        private void ButtonE2Ab_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Button, (String)ButtonEnc2Ab.Content);
        }
        #endregion
    }
}
