using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Profiler.Pages
{
    /// <summary>
    /// Lógica de interacción para CtlJoystick.xaml
    /// </summary>
    internal sealed partial class SaitekX52 : Page, IHidToButton
    {
        private readonly Controls.CtlProperties props;
        private Microsoft.UI.Xaml.Controls.Primitives.ToggleButton lastUse = null;
        private readonly CHidToButton converter;

        public SaitekX52(Controls.CtlProperties props)
        {
            InitializeComponent();
            this.props = props;
            converter = new([
                new CHidToButton.Map(0, ButtonX),
                new CHidToButton.Map(1, ButtonY),
                new CHidToButton.Map(2, ButtonR),
                new CHidToButton.Map(3, ButtonZ),
                new CHidToButton.Map(4, ButtonRy),
                new CHidToButton.Map(5, ButtonRx),
                new CHidToButton.Map(6, Buttonsl),
                new CHidToButton.Map(7, ButtonMinix),
                new CHidToButton.Map(8, ButtonMiniy),
                //new CHidToButton.Map(8, ButtonEnc1Ar),
                //new CHidToButton.Map(9, ButtonEnc1Ab),
                //new CHidToButton.Map(10, ButtonEnc2Ar),
                //new CHidToButton.Map(11, ButtonEnc2Ab),
                //new CHidToButton.Map(12, ButtonBase3),
                //new CHidToButton.Map(13, ButtonBase2),
                //new CHidToButton.Map(14, ButtonBase1),
                //new CHidToButton.Map(16, ButtonTrigger2),
                //new CHidToButton.Map(17, ButtonTrigger1),
                //new CHidToButton.Map(18, ButtonPinkie),
                //new CHidToButton.Map(19, ButtonLanzar),
                //new CHidToButton.Map(20, Buttonp10),
                //new CHidToButton.Map(23, Button1),
                //new CHidToButton.Map(28, Buttonp35),
                //new CHidToButton.Map(29, Buttonp31),
                //new CHidToButton.Map(30, Buttonp37),
                //new CHidToButton.Map(31, Buttonp33),
                //new CHidToButton.Map(32, Buttonp25),
                //new CHidToButton.Map(33, Buttonp21),
                //new CHidToButton.Map(34, Buttonp27),
                //new CHidToButton.Map(35, Buttonp23),
                new CHidToButton.Map(36, [Buttonp11, Buttonp13, Buttonp15, Buttonp17]),
            ]);
        }

        public void UpdateStatus(Devices.DeviceInfo di, byte[] rawData)
        {
            converter.Update(di, rawData);
        }

        /*** TODO CORRECT Idx & Hats ***/

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 0, (string)ButtonX.Content);
            ButtonX.IsChecked = true;
            lastUse = ButtonX;
        }

        private void Uncheck(object sender)
        {
            if (lastUse != null) { lastUse.IsChecked = false; }
            lastUse = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
        }

        #region Joystick
        #region "Seta 1"
        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 36, "Seta 1 Norte", 0);
            Uncheck(sender);
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 36, "Seta 1 Este", 2);
            Uncheck(sender);
        }


        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 36, "Seta 1 Sur", 4);
            Uncheck(sender);
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 36, "Seta 1 Oeste", 6);
            Uncheck(sender);
        }
        #endregion

        #region "Seta 2"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 8, "Seta 2 Norte");
            Uncheck(sender);
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 10, "Seta 2 Este");
            Uncheck(sender);
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 12, "Seta 2 Sur");
            Uncheck(sender);
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 14, "Seta 2 Oeste");
            Uncheck(sender);
        }
        #endregion

        #region "botones"
        private void ButtonA_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 8, (string)ButtonA.Content);
            Uncheck(sender);
        }

        private void ButtonB_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 9, (string)ButtonB.Content);
            Uncheck(sender);
        }

        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 4, (string)ButtonC.Content);
            Uncheck(sender);
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 2, (string)ButtonLaunch.Content);
            Uncheck(sender);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 1, (string)ButtonTrigger1.Content);
            Uncheck(sender);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 0, (string)ButtonTrigger2.Content);
            Uncheck(sender);
        }
        #endregion

        #region "modos"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 3, (string)ButtonPinkie.Content);
            Uncheck(sender);
        }

        private void ButtonMode1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 5, (string)ButtonMode1.Content);
            Uncheck(sender);
        }

        private void ButtonMode2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 6, (string)ButtonMode2.Content);
            Uncheck(sender);
        }

        private void ButtonMode3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 7, (string)ButtonMode3.Content);
            Uncheck(sender);
        }
        #endregion

        #region "toggles"
        private void ButtonTg1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 10, (string)ButtonTg1.Content);
            Uncheck(sender);
        }

        private void ButtonTg2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 11, (string)ButtonTg2.Content);
            Uncheck(sender);
        }

        private void ButtonTg3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 12, (string)ButtonTg3.Content);
            Uncheck(sender);
        }

        private void ButtonTg4_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 13, (string)ButtonTg4.Content);
            Uncheck(sender);
        }

        private void ButtonTg5_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 14, (string)ButtonTg5.Content);
            Uncheck(sender);
        }

        private void ButtonTg6_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 15, (string)ButtonTg6.Content);
            Uncheck(sender);
        }
        #endregion

        #region "ejes"
        private void ButtonX_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 0, (string)ButtonX.Content);
            Uncheck(sender);
        }

        private void ButtonY_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 1, (string)ButtonY.Content);
            Uncheck(sender);
        }

        private void ButtonR_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 2, (string)ButtonR.Content);
            Uncheck(sender);
        }
        #endregion
        #endregion

        #region Throttle
        #region "Seta 3"
        private void Buttonp31_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 0, "Seta 3 Norte");
            Uncheck(sender);
        }

        private void Buttonp33_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 2, "Seta 3 Este");
            Uncheck(sender);
        }

        private void Buttonp35_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 4, "Seta 3 Sur");
            Uncheck(sender);
        }

        private void Buttonp37_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 6, "Seta 3 Oeste");
            Uncheck(sender);
        }
        #endregion

        #region "Seta 4"
        private void Buttonp41_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 8, "Seta 4 Norte");
            Uncheck(sender);
        }

        private void Buttonp43_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 10, "Seta 4 Este");
            Uncheck(sender);
        }

        private void Buttonp45_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 12, "Seta 4 Sur");
            Uncheck(sender);
        }

        private void Buttonp47_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 14, "Seta 4 Oeste");
            Uncheck(sender);
        }

        private void Buttonp48_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 15, "Seta 4 Noroeste");
            Uncheck(sender);
        }
        #endregion

        #region "botones"
        private void Buttond_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 0, (string)Buttond.Content);
            Uncheck(sender);
        }

        private void Buttone_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 1, (string)Buttone.Content);
            Uncheck(sender);
        }

        private void Buttoni_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 5, (string)Buttoni.Content);
            Uncheck(sender);
        }
        #endregion

        #region "rueda"
        private void ButtonWup_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 8, (string)ButtonWup.Content);
            Uncheck(sender);
        }

        private void ButtonWb_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 7, (string)ButtonWb.Content);
            Uncheck(sender);
        }

        private void ButtonWdown_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 9, (string)ButtonWdown.Content);
            Uncheck(sender);
        }
        #endregion

        #region "botones mfd"
        private void ButtonMfd3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 4, (string)ButtonMfd3.Content);
            Uncheck(sender);
        }

        private void ButtonMfd2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 3, (string)ButtonMfd2.Content);
            Uncheck(sender);
        }

        private void ButtonMfd1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 2, (string)ButtonMfd1.Content);
            Uncheck(sender);
        }
        #endregion

        #region "ministick"
        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 0, (string)ButtonMinix.Content);
            Uncheck(sender);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 1, (string)ButtonMiniy.Content);
            Uncheck(sender);
        }

        private void ButtonMouse_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 6, (string)ButtonMouse.Content);
            Uncheck(sender);
        }
        #endregion

        #region "ejes"
        private void ButtonRy_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 4, (string)ButtonRy.Content);
            Uncheck(sender);
        }

        private void ButtonRx_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 5, (string)ButtonRx.Content);
            Uncheck(sender);
        }

        private void Buttonsl_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 6, (string)Buttonsl.Content);
            Uncheck(sender);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 3, (string)ButtonZ.Content);
            Uncheck(sender);
        }
        #endregion
        #endregion
    }
}
