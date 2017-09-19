using System;
using System.Windows;
using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlJoystick.xaml
    /// </summary>
    internal partial class CtlJoystick : UserControl
    {
        private CtlPropiedades Vista;

        public CtlJoystick()
        {
            InitializeComponent();
            Vista = ((CtlEditar)this.Parent).ctlPropiedades;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(64, (String)ButtonX.Content);
        }

        #region "Seta 1"
        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(100, "Seta 1 Norte");
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(101, "Seta 1 Noreste");
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(102, "Seta 1 Este");
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(103, "Seta 1 Sureste");
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(104, "Seta 1 Sur");
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(105, "Seta 1 Suroeste");
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(106, "Seta 1 Oeste");
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(107, "Seta 1 Noroeste");
        }
        #endregion

        #region "Seta 2"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(108, "Seta 2 Norte");
        }

        private void Buttonp22_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(109, "Seta 2 Noreste");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(110, "Seta 2 Este");
        }

        private void Buttonp24_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(111, "Seta 2 Sureste");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(112, "Seta 2 Sur");
        }

        private void Buttonp26_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(113, "Seta 2 Suroeste");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(114, "Seta 2 Oeste");
        }

        private void Buttonp28_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(115, "Seta 2 Noroeste");
        }
        #endregion

        #region "botones"
        private void ButtonA_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(1, (String)ButtonA.Content);
        }

        private void ButtonB_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(2, (String)ButtonB.Content);
        }

        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(7, (String)ButtonC.Content);
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(3, (String)ButtonC.Content);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(22, (String)ButtonTrigger1.Content);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(0, (String)ButtonTrigger2.Content);
        }
        #endregion

        #region "modos"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(6, (String)ButtonPinkie.Content);
        }

        private void ButtonMode1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(8, (String)ButtonMode1.Content);
        }

        private void ButtonMode2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(9, (String)ButtonMode2.Content);
        }

        private void ButtonMode3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(10, (String)ButtonMode3.Content);
        }
        #endregion

        #region "toggles"
        private void ButtonTg1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(16, (String)ButtonTg1.Content);
        }

        private void ButtonTg2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(17, (String)ButtonTg2.Content);
        }

        private void ButtonTg3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(18, (String)ButtonTg3.Content);
        }

        private void ButtonTg4_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(19, (String)ButtonTg4.Content);
        }

        private void ButtonTg5_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(20, (String)ButtonTg5.Content);
        }

        private void ButtonTg6_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(21, (String)ButtonTg6.Content);
        }
        #endregion

        #region "ejes"
        private void ButtonX_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(64, (String)ButtonX.Content);
        }

        private void ButtonY_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(65, (String)ButtonY.Content);
        }

        private void ButtonR_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(66, (String)ButtonR.Content);
        }
        #endregion
    }
}
