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

        public CtlX52Joystick(CtlEditar padre)
        {
            InitializeComponent();
            Vista = padre.ctlPropiedades;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Eje, (String)ButtonX.Content);
        }

        #region "Seta 1"
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
        private void ButtonA_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, CEnums.Tipo.Boton, (String)ButtonA.Content);
        }

        private void ButtonB_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, CEnums.Tipo.Boton, (String)ButtonB.Content);
        }

        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, CEnums.Tipo.Boton, (String)ButtonC.Content);
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, CEnums.Tipo.Boton, (String)ButtonLaunch.Content);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, CEnums.Tipo.Boton, (String)ButtonTrigger1.Content);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, CEnums.Tipo.Boton, (String)ButtonTrigger2.Content);
        }
        #endregion

        #region "modos"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, CEnums.Tipo.Boton, (String)ButtonPinkie.Content);
        }

        private void ButtonMode1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, CEnums.Tipo.Boton, (String)ButtonMode1.Content);
        }

        private void ButtonMode2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, CEnums.Tipo.Boton, (String)ButtonMode2.Content);
        }

        private void ButtonMode3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, CEnums.Tipo.Boton, (String)ButtonMode3.Content);
        }
        #endregion

        #region "toggles"
        private void ButtonTg1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, CEnums.Tipo.Boton, (String)ButtonTg1.Content);
        }

        private void ButtonTg2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, CEnums.Tipo.Boton, (String)ButtonTg2.Content);
        }

        private void ButtonTg3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, CEnums.Tipo.Boton, (String)ButtonTg3.Content);
        }

        private void ButtonTg4_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, CEnums.Tipo.Boton, (String)ButtonTg4.Content);
        }

        private void ButtonTg5_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, CEnums.Tipo.Boton, (String)ButtonTg5.Content);
        }

        private void ButtonTg6_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, CEnums.Tipo.Boton, (String)ButtonTg6.Content);
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
        #endregion
    }
}
