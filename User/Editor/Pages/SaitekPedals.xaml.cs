﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Profiler.Pages
{
    /// <summary>
    /// Lógica de interacción para CtlPedales.xaml
    /// </summary>
    internal sealed partial class SaitekPedals : Page, IHidToButton
    {
        private readonly Controls.Properties.CtlProperties props;
        private ToggleButton lastUse = null;
        private readonly CHidToButton converter;

        public SaitekPedals(Controls.Properties.CtlProperties props)
        {
            InitializeComponent();
            this.props = props;
            converter = new([
                new CHidToButton.Map(0, BtRight),
                new CHidToButton.Map(1, BtLeft),
                new CHidToButton.Map(2, BtDefault)]);
        }

        public void UpdateStatus(Devices.DeviceInfo di, byte[] rawData)
        {
            converter.Update(di, rawData);
        }

        private void Uncheck(object sender)
        {
            if ((lastUse != null) && (lastUse != (ToggleButton)sender)) { lastUse.IsChecked = false; }
            lastUse = (ToggleButton)sender;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            props.Show(0x06a30763, 2, (string)BtDefault.Content);
            Uncheck(BtDefault);
        }

        #region "axes"
        private void RightPedal_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x06a30763, 0, (string)((ToggleButton)sender).Content);
            Uncheck(sender);
        }

        private void LeftPedal_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x06a30763, 1, (string)((ToggleButton)sender).Content);
            Uncheck(sender);
        }

        private void AxisR_Click(object sender, RoutedEventArgs e)
        {
            props.Show(0x06a30763, 2, (string)((ToggleButton)sender).Content);
            Uncheck(sender);
        }
        #endregion
    }
}
