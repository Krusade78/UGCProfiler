﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Profiler.Pages
{
    /// <summary>
    /// Lógica de interacción para CtlJoystick.xaml
    /// </summary>
    internal partial class VkbGladiatorNXT : Page, IHidToButton
    {
        private readonly Controls.CtlProperties props;
        private Microsoft.UI.Xaml.Controls.Primitives.ToggleButton lastUse = null;
        private readonly CHidToButton converter;

        public VkbGladiatorNXT(Controls.CtlProperties props)
        {
            InitializeComponent();
            this.props = props;
            converter = new([
                new CHidToButton.Map(0, ButtonX),
                new CHidToButton.Map(1, ButtonY),
                new CHidToButton.Map(2, ButtonR),
                new CHidToButton.Map(3, ButtonZ),
                new CHidToButton.Map(4, ButtonMinix),
                new CHidToButton.Map(5, ButtonMiniy),
                new CHidToButton.Map(8, ButtonEnc1Ar),
                new CHidToButton.Map(9, ButtonEnc1Ab),
                new CHidToButton.Map(10, ButtonEnc2Ar),
                new CHidToButton.Map(11, ButtonEnc2Ab),
                new CHidToButton.Map(12, ButtonBase3),
                new CHidToButton.Map(13, ButtonBase2),
                new CHidToButton.Map(14, ButtonBase1),
                new CHidToButton.Map(16, ButtonTrigger2),
                new CHidToButton.Map(17, ButtonTrigger1),
                new CHidToButton.Map(18, ButtonPinkie),
                new CHidToButton.Map(19, ButtonLanzar),
                new CHidToButton.Map(20, Buttonp10),
                new CHidToButton.Map(23, Button1),
                new CHidToButton.Map(28, Buttonp35),
                new CHidToButton.Map(29, Buttonp31),
                new CHidToButton.Map(30, Buttonp37),
                new CHidToButton.Map(31, Buttonp33),
                new CHidToButton.Map(32, Buttonp25),
                new CHidToButton.Map(33, Buttonp21),
                new CHidToButton.Map(34, Buttonp27),
                new CHidToButton.Map(35, Buttonp23),
                new CHidToButton.Map(36, Buttonp45),
                new CHidToButton.Map(37, Buttonp41),
                new CHidToButton.Map(38, Buttonp47),
                new CHidToButton.Map(39, Buttonp43),
                new CHidToButton.Map(40, Buttonp40),
                new CHidToButton.Map(43, ButtonS1),
                new CHidToButton.Map(44, [Buttonp11, Buttonp12, Buttonp13, Buttonp14, Buttonp15, Buttonp16, Buttonp17, Buttonp18]),
            ]);
        }

        public void UpdateStatus(Devices.DeviceInfo di, byte[] rawData)
        {
            converter.Update(di, rawData);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 0, (string)ButtonX.Content);
            ButtonX.IsChecked = true;
            lastUse = ButtonX;
        }

        private void Uncheck(object sender)
        {
            if (lastUse != null) { lastUse.IsChecked = false; }
            lastUse = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
        }

        #region "Seta 1"
        private void Buttonp10_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 20, "Seta 1 Centro");
            Uncheck(sender);
        }

        private void Buttonp11_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Norte", 0);
            Uncheck(sender);
        }

        private void Buttonp12_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Noreste", 1);
            Uncheck(sender);
        }

        private void Buttonp13_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Este", 2);
            Uncheck(sender);
        }

        private void Buttonp14_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Sureste", 3);
            Uncheck(sender);
        }

        private void Buttonp15_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Sur", 4);
            Uncheck(sender);
        }

        private void Buttonp16_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Suroeste", 5);
            Uncheck(sender);
        }

        private void Buttonp17_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Oeste", 6);
            Uncheck(sender);
        }

        private void Buttonp18_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 44, "Seta 1 Noroeste", 7);
            Uncheck(sender);
        }
        #endregion

        #region "Seta 2"
        private void Buttonp21_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 33, "Seta 2 Norte");
            Uncheck(sender);
        }

        private void Buttonp23_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 35, "Seta 2 Este");
            Uncheck(sender);
        }

        private void Buttonp25_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 32, "Seta 2 Sur");
            Uncheck(sender);
        }

        private void Buttonp27_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 34, "Seta 2 Oeste");
            Uncheck(sender);
        }

        #endregion

        #region "Seta 3"
        private void Buttonp31_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 29, "Seta 3 Norte");
            Uncheck(sender);
        }

        private void Buttonp33_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 31, "Seta 3 Este");
            Uncheck(sender);
        }

        private void Buttonp35_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 28, "Seta 3 Sur");
            Uncheck(sender);
        }

        private void Buttonp37_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 30, "Seta 3 Oeste");
            Uncheck(sender);
        }
        #endregion

        #region "Seta 4"
        private void Buttonp40_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 40, "Seta 4 Centro");
            Uncheck(sender);
        }

        private void Buttonp41_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 37, "Seta 4 Norte");
            Uncheck(sender);
        }

        private void Buttonp43_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 39, "Seta 4 Este");
            Uncheck(sender);
        }

        private void Buttonp45_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 36, "Seta 4 Sur");
            Uncheck(sender);
        }

        private void Buttonp47_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 38, "Seta 4 Oeste");
            Uncheck(sender);
        }
        #endregion

        #region "botones"
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 23, (string)Button1.Content);
            Uncheck(sender);
        }

        private void ButtonFastTrigger_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 43, (string)ButtonS1.Content);
            Uncheck(sender);
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 19, (string)ButtonLanzar.Content);
            Uncheck(sender);
        }

        private void ButtonTrigger1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 17, (string)ButtonTrigger1.Content);
            Uncheck(sender);
        }

        private void ButtonTrigger2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 16, (string)ButtonTrigger2.Content);
            Uncheck(sender);
        }

        private void ButtonPinkie_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 18, (string)ButtonPinkie.Content);
            Uncheck(sender);
        }
        #endregion

        #region "Base"
        private void ButtonBase1_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 14, (string)ButtonBase1.Content);
            Uncheck(sender);
        }

        private void ButtonBase2_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 13, (string)ButtonBase2.Content);
            Uncheck(sender);
        }

        private void ButtonBase3_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 12, (string)ButtonBase3.Content);
            Uncheck(sender);
        }
        #endregion

        #region "ejes"
        private void ButtonX_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 0, (string)ButtonX.Content);
            Uncheck(sender);
        }

        private void ButtonY_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 1, (string)ButtonY.Content);
            Uncheck(sender);
        }

        private void ButtonR_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 2, (string)ButtonR.Content);
            Uncheck(sender);
        }

        private void ButtonZ_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 3, (string)ButtonZ.Content);
            Uncheck(sender);
        }

        private void ButtonMinix_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 4, (string)ButtonMinix.Content);
            Uncheck(sender);
        }

        private void ButtonMiniy_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 5, (string)ButtonMiniy.Content);
            Uncheck(sender);
        }
        #endregion

        #region "encoders"
        private void ButtonE1Ar_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 8, (string)ButtonEnc1Ar.Content);
            Uncheck(sender);
        }

        private void ButtonE1Ab_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 9, (string)ButtonEnc1Ab.Content);
            Uncheck(sender);
        }

        private void ButtonE2Ar_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 10, (string)ButtonEnc2Ar.Content);
            Uncheck(sender);
        }

        private void ButtonE2Ab_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x231d0200, 11, (string)ButtonEnc2Ab.Content);
            Uncheck(sender);
        }
        #endregion
    }
}
