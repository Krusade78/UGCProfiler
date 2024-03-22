using System;
using System.Windows;
using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlJoystick.xaml
    /// </summary>
    internal partial class CtlX52Joystick : UserControl
    {
        private readonly CtlPropiedades Vista;

        public CtlX52Joystick(CtlPropiedades vista)
        {
            InitializeComponent();
            Vista = vista;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Axis, (String)ButtonX.Content);
        }

        #region Joystick
        #region "Seta 1"
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

        private void Buttonp22_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.ElementType.Hat, "Seta 2 Noreste");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.ElementType.Hat, "Seta 2 Este");
        }

        private void Buttonp24_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.ElementType.Hat, "Seta 2 Sureste");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.ElementType.Hat, "Seta 2 Sur");
        }

        private void Buttonp26_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.ElementType.Hat, "Seta 2 Suroeste");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.ElementType.Hat, "Seta 2 Oeste");
        }

        private void Buttonp28_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.ElementType.Hat, "Seta 2 Noroeste");
        }
        #endregion

        #region "botones"
        private void ButtonA_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.ElementType.Button, (String)ButtonA.Content);
        }

        private void ButtonB_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.ElementType.Button, (String)ButtonB.Content);
        }

        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.ElementType.Button, (String)ButtonC.Content);
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Button, (String)ButtonLaunch.Content);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Button, (String)ButtonTrigger1.Content);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Button, (String)ButtonTrigger2.Content);
        }
        #endregion

        #region "modos"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Button, (String)ButtonPinkie.Content);
        }

        private void ButtonMode1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.ElementType.Button, (String)ButtonMode1.Content);
        }

        private void ButtonMode2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Button, (String)ButtonMode2.Content);
        }

        private void ButtonMode3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.ElementType.Button, (String)ButtonMode3.Content);
        }
        #endregion

        #region "toggles"
        private void ButtonTg1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.ElementType.Button, (String)ButtonTg1.Content);
        }

        private void ButtonTg2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.ElementType.Button, (String)ButtonTg2.Content);
        }

        private void ButtonTg3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.ElementType.Button, (String)ButtonTg3.Content);
        }

        private void ButtonTg4_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.ElementType.Button, (String)ButtonTg4.Content);
        }

        private void ButtonTg5_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.ElementType.Button, (String)ButtonTg5.Content);
        }

        private void ButtonTg6_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.ElementType.Button, (String)ButtonTg6.Content);
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
        #endregion
        #endregion

        #region Throttle
        #region "Seta 3"
        private void Buttonp31_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Hat, "Seta 3 Norte");
        }

        private void Buttonp32_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Hat, "Seta 3 Noreste");
        }

        private void Buttonp33_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Hat, "Seta 3 Este");
        }

        private void Buttonp34_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Hat, "Seta 3 Sureste");
        }

        private void Buttonp35_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.ElementType.Hat, "Seta 3 Sur");
        }

        private void Buttonp36_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.ElementType.Hat, "Seta 3 Suroeste");
        }

        private void Buttonp37_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Hat, "Seta 3 Oeste");
        }

        private void Buttonp38_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.ElementType.Hat, "Seta 3 Noroeste");
        }
        #endregion

        #region "Seta 4"
        private void Buttonp41_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.ElementType.Hat, "Seta 4 Norte");
        }

        private void Buttonp42_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.ElementType.Hat, "Seta 4 Noreste");
        }

        private void Buttonp43_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.ElementType.Hat, "Seta 4 Este");
        }

        private void Buttonp44_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.ElementType.Hat, "Seta 4 Sureste");
        }

        private void Buttonp45_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.ElementType.Hat, "Seta 4 Sur");
        }

        private void Buttonp46_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.ElementType.Hat, "Seta 4 Suroeste");
        }

        private void Buttonp47_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.ElementType.Hat, "Seta 4 Oeste");
        }

        private void Buttonp48_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.ElementType.Hat, "Seta 4 Noroeste");
        }
        #endregion

        #region "botones"
        private void Buttond_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Button, (String)Buttond.Content);
        }

        private void Buttone_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Button, (String)Buttone.Content);
        }

        private void Buttoni_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.ElementType.Button, (String)Buttoni.Content);
        }
        #endregion

        #region "rueda"
        private void ButtonWup_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.ElementType.Button, (String)ButtonWup.Content);
        }

        private void ButtonWb_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.ElementType.Button, (String)ButtonWb.Content);
        }

        private void ButtonWdown_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.ElementType.Button, (String)ButtonWdown.Content);
        }
        #endregion

        #region "botones mfd"
        private void ButtonMfd3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.ElementType.Button, (String)ButtonMfd3.Content);
        }

        private void ButtonMfd2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Button, (String)ButtonMfd2.Content);
        }

        private void ButtonMfd1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Button, (String)ButtonMfd1.Content);
        }
        #endregion

        #region "ministick"
        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.ElementType.Axis, (String)ButtonMinix.Content);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.ElementType.Axis, (String)ButtonMiniy.Content);
        }

        private void ButtonMouse_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Button, (String)ButtonMouse.Content);
        }
        #endregion

        #region "ejes"
        private void ButtonRy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.ElementType.Axis, (String)ButtonRy.Content);
        }

        private void ButtonRx_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.ElementType.Axis, (String)ButtonRx.Content);
        }

        private void Buttonsl_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.ElementType.Axis, (String)Buttonsl.Content);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.ElementType.Axis, (String)ButtonZ.Content);
        }
        #endregion
        #endregion
    }
}
