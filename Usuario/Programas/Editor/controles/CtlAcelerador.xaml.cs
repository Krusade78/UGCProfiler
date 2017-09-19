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
        private CtlPropiedades Vista;

        public CtlAcelerador(CtlEditar padre)
        {
            InitializeComponent();
            Vista = padre.ctlPropiedades;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Vista.Ver(67, (String)ButtonZ.Content);
        }

        #region "Seta 3"
        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(116, "Seta 3 Norte");
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(117, "Seta 3 Noreste");
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(118, "Seta 3 Este");
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(119, "Seta 3 Sureste");
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(120, "Seta 3 Sur");
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(121, "Seta 3 Suroeste");
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(122, "Seta 3 Oeste");
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(123, "Seta 3 Noroeste");
        }
        #endregion

        #region "Seta 4"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(124, "Seta 4 Norte");
        }

        private void Buttonp22_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(125, "Seta 4 Noreste");
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(126, "Seta 4 Este");
        }

        private void Buttonp24_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(127, "Seta 4 Sureste");
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(128, "Seta 4 Sur");
        }

        private void Buttonp26_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(129, "Seta 4 Suroeste");
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(130, "Seta 4 Oeste");
        }

        private void Buttonp28_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(131, "Seta 4 Noroeste");
        }
        #endregion

        #region "botones"
        private void Buttond_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(4, (String)Buttond.Content);
        }

        private void Buttone_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(15, (String)Buttone.Content);
        }

        private void Buttoni_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(14, (String)Buttoni.Content);
        }
        #endregion

        #region "rueda"
        private void ButtonWup_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(24, (String)ButtonWup.Content);
        }

        private void ButtonWb_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(23, (String)ButtonWb.Content);
        }

        private void ButtonWdown_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(25, (String)ButtonWdown.Content);
        }
        #endregion

        #region "botones mfd"
        private void ButtonMfd3_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(13, (String)ButtonMfd3.Content);
        }

        private void ButtonMfd2_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(12, (String)ButtonMfd2.Content);
        }

        private void ButtonMfd1_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(11, (String)ButtonMfd1.Content);
        }
        #endregion

        #region "ministick"
        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(71, (String)ButtonMinix.Content);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(72, (String)ButtonMiniy.Content);
        }

        private void ButtonMouse_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(5, (String)ButtonMouse.Content);
        }
        #endregion

       #region "ejes"
        private void ButtonRy_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(69, (String)ButtonRy.Content);
        }

        private void ButtonRx_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(68, (String)ButtonRx.Content);
        }

        private void Buttonsl_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(70, (String)Buttonsl.Content);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            Vista.Ver(67, (String)ButtonZ.Content);
        }
        #endregion
    }
}
