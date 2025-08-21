using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;


namespace Profiler.Pages
{
    internal  partial class SaitekX52 : FluentAvalonia.UI.Controls.Frame, IHidToButton
    {
        private readonly Controls.Properties.CtlProperties props;
        private ToggleButton lastUse = null;
        private readonly CHidToButton converter;

#if DEBUG
        public SaitekX52() { InitializeComponent(); }
#endif
        public SaitekX52(Controls.Properties.CtlProperties props)
        {
            InitializeComponent();
            this.props = props;
            converter = new([
                new CHidToButton.Map(0, ButtonX),
                new CHidToButton.Map(1, ButtonY),
                new CHidToButton.Map(2, ButtonR),
                new CHidToButton.Map(3, ButtonZ),
                new CHidToButton.Map(5, ButtonRy),
                new CHidToButton.Map(4, ButtonRx),
                new CHidToButton.Map(6, ButtonSl),
                new CHidToButton.Map(43, ButtonMinix),
                new CHidToButton.Map(42, ButtonMiniy),

                new CHidToButton.Map(7, ButtonTrigger1),
                new CHidToButton.Map(8, ButtonLaunch),
                new CHidToButton.Map(9, ButtonA),
                new CHidToButton.Map(10, ButtonB),
                new CHidToButton.Map(11, ButtonC),
                new CHidToButton.Map(12, ButtonPinkie),
                new CHidToButton.Map(13, ButtonD),
                new CHidToButton.Map(14, ButtonE),
                new CHidToButton.Map(15, ButtonTg1),
                new CHidToButton.Map(16, ButtonTg2),
                new CHidToButton.Map(17, ButtonTg3),
                new CHidToButton.Map(18, ButtonTg4),
                new CHidToButton.Map(19, ButtonTg5),
                new CHidToButton.Map(20, ButtonTg6),
                new CHidToButton.Map(21, ButtonTrigger2),
                new CHidToButton.Map(22, Buttonp21),
                new CHidToButton.Map(23, Buttonp23),
                new CHidToButton.Map(24, Buttonp25),
                new CHidToButton.Map(25, Buttonp27),
                new CHidToButton.Map(26, Buttonp31),
                new CHidToButton.Map(27, Buttonp33),
                new CHidToButton.Map(28, Buttonp35),
                new CHidToButton.Map(29, Buttonp37),
                new CHidToButton.Map(30, ButtonMode1),
                new CHidToButton.Map(31, ButtonMode2),
                new CHidToButton.Map(32, ButtonMode3),
                new CHidToButton.Map(33, ButtonMfd1),
                new CHidToButton.Map(34, ButtonMfd2),
                new CHidToButton.Map(35, ButtonMfd3),
                new CHidToButton.Map(36, ButtonI),
                new CHidToButton.Map(37, ButtonMouse),
                new CHidToButton.Map(38, ButtonWb),
                new CHidToButton.Map(39, ButtonWup),
                new CHidToButton.Map(40, ButtonWdown),

                new CHidToButton.Map(41, [Buttonp11, Buttonp12, Buttonp13, Buttonp14, Buttonp15, Buttonp16, Buttonp17, Buttonp18]),
            ]);
        }

        public void UpdateStatus(Devices.DeviceInfo di, byte[] rawData)
        {
            converter.Update(di, rawData);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (Avalonia.Controls.Design.IsDesignMode) { return; }
#endif
            props.Show(0x6a30255, 0, (string)ButtonX.Content);
            ButtonX.IsChecked = true;
            lastUse = ButtonX;
        }

        private void Uncheck(object sender)
        {
            if ((lastUse != null) && (lastUse != (ToggleButton)sender)) { lastUse.IsChecked = false; }
            lastUse = (ToggleButton)sender;
        }

        #region Joystick
        #region "Hat 1"
        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Avalonia.Application.Current.Resources["up"]}", 0);
            Uncheck(sender);
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Avalonia.Application.Current.Resources["up-right"]}", 1);
            Uncheck(sender);
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Avalonia.Application.Current.Resources["right"]}", 2);
            Uncheck(sender);
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Avalonia.Application.Current.Resources["down-right"]}", 3);
            Uncheck(sender);
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Application.Current.Resources["down"]}", 4);
            Uncheck(sender);
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Application.Current.Resources["down-left"]}", 5);
            Uncheck(sender);
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Application.Current.Resources["left"]}", 6);
            Uncheck(sender);
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 41, $"{(string)Application.Current.Resources["hat"]} 1 {(string)Application.Current.Resources["up-left"]}", 7);
            Uncheck(sender);
        }
        #endregion

        #region "Hat 2"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 22, $"{(string)Application.Current.Resources["hat"]} 2 {(string)Application.Current.Resources["up"]}");
            Uncheck(sender);
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 23, $"{(string)Application.Current.Resources["hat"]} 2 {(string)Application.Current.Resources["right"]}");
            Uncheck(sender);
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 24, $"{(string)Application.Current.Resources["hat"]} 2 {(string)Application.Current.Resources["down"]}");
            Uncheck(sender);
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 25, $"{(string)Application.Current.Resources["hat"]} 2 {(string)Application.Current.Resources["left"]}");
            Uncheck(sender);
        }
        #endregion

        #region "Buttons"
        private void ButtonA_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 9, (string)ButtonA.Content);
            Uncheck(sender);
        }

        private void ButtonB_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 10, (string)ButtonB.Content);
            Uncheck(sender);
        }

        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 11, (string)ButtonC.Content);
            Uncheck(sender);
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 8, (string)ButtonLaunch.Content);
            Uncheck(sender);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 7, (string)ButtonTrigger1.Content);
            Uncheck(sender);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 21, (string)ButtonTrigger2.Content);
            Uncheck(sender);
        }
        #endregion

        #region "Modes"
        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 12, (string)ButtonPinkie.Content);
            Uncheck(sender);
        }

        private void ButtonMode1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 30, (string)ButtonMode1.Content);
            Uncheck(sender);
        }

        private void ButtonMode2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 31, (string)ButtonMode2.Content);
            Uncheck(sender);
        }

        private void ButtonMode3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 32, (string)ButtonMode3.Content);
            Uncheck(sender);
        }
        #endregion

        #region "toggles"
        private void ButtonTg1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 15, (string)ButtonTg1.Content);
            Uncheck(sender);
        }

        private void ButtonTg2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 16, (string)ButtonTg2.Content);
            Uncheck(sender);
        }

        private void ButtonTg3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 17, (string)ButtonTg3.Content);
            Uncheck(sender);
        }

        private void ButtonTg4_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 18, (string)ButtonTg4.Content);
            Uncheck(sender);
        }

        private void ButtonTg5_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 19, (string)ButtonTg5.Content);
            Uncheck(sender);
        }

        private void ButtonTg6_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 20, (string)ButtonTg6.Content);
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
        #region "Hat 3"
        private void Buttonp31_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 26, $"{(string)Application.Current.Resources["hat"]} 3 {(string)Application.Current.Resources["up"]}");
            Uncheck(sender);
        }

        private void Buttonp33_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 27, $"{(string)Application.Current.Resources["hat"]} 3 {(string)Application.Current.Resources["right"]}");
            Uncheck(sender);
        }

        private void Buttonp35_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 28, $"{(string)Application.Current.Resources["hat"]} 3 {(string)Application.Current.Resources["down"]}");
            Uncheck(sender);
        }

        private void Buttonp37_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 29, $"{(string)Application.Current.Resources["hat"]} 3 {(string)Application.Current.Resources["left"]}");
            Uncheck(sender);
        }
        #endregion

        #region "Buttons"
        private void Buttond_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 13, (string)ButtonD.Content);
            Uncheck(sender);
        }

        private void Buttone_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 14, (string)ButtonE.Content);
            Uncheck(sender);
        }

        private void Buttoni_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 36, (string)ButtonI.Content);
            Uncheck(sender);
        }
        #endregion

        #region "Wheel"
        private void ButtonWup_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 39, (string)ButtonWup.Content);
            Uncheck(sender);
        }

        private void ButtonWb_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 38, (string)ButtonWb.Content);
            Uncheck(sender);
        }

        private void ButtonWdown_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 40, (string)ButtonWdown.Content);
            Uncheck(sender);
        }
        #endregion

        #region "MFD buttons"
        private void ButtonMfd3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 35, (string)ButtonMfd3.Content);
            Uncheck(sender);
        }

        private void ButtonMfd2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 34, (string)ButtonMfd2.Content);
            Uncheck(sender);
        }

        private void ButtonMfd1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 33, (string)ButtonMfd1.Content);
            Uncheck(sender);
        }
        #endregion

        #region "ministick"
        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 43, (string)ButtonMinix.Content);
            Uncheck(sender);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 42, (string)ButtonMiniy.Content);
            Uncheck(sender);
        }

        private void ButtonMouse_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 37, (string)ButtonMouse.Content);
            Uncheck(sender);
        }
        #endregion

        #region "axes"
        private void ButtonRy_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 5, (string)ButtonRy.Content);
            Uncheck(sender);
        }

        private void ButtonRx_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 4, (string)ButtonRx.Content);
            Uncheck(sender);
        }

        private void Buttonsl_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x6a30255, 6, (string)ButtonSl.Content);
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
