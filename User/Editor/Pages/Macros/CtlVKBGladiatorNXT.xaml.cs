using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlVKBGladiatorNXT : UserControl, IChangeMode
    {
        public CtlVKBGladiatorNXT()
        {
            this.InitializeComponent();
        }

        #region "Leds NXT"
        private void ButtonLed_Click(object sender, RoutedEventArgs e)
        {
            //Leds((byte)((cbLed.SelectedIndex == 1) ? 11 : ((cbLed.SelectedIndex == 2) ? 10 : 0)) , (CEnums.LedOrder)cbOrden.SelectedIndex, (CEnums.ModoColor)cbModo.SelectedIndex, txtColor1.Text, txtColor2.Text);
        }

        private void FtxtColor1_PreviewMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            //Ventanas.VColor vc = new(txtColor1.Text)
            //{
            //    Owner = this
            //};
            //if (vc.ShowDialog() == true)
            //{
            //    txtColor1.Text = vc.ColorSt;
            //    System.Windows.Media.Color c = vc.ColorSB;
            //    if (cbLed.SelectedIndex == 0)
            //    {
            //        txtColor1.Text = "0;0;" + txtColor1.Text.Remove(0, 4);
            //        c.R = 0;
            //        c.G = 0;
            //    }
            //    else if (cbLed.SelectedIndex == 1)
            //    {
            //        txtColor1.Text = txtColor1.Text[..1] + ";0;0;";
            //        c.B = 0;
            //        c.G = 0;
            //    }
            //    rColor1.Fill = new System.Windows.Media.SolidColorBrush(c);
            //}
        }

        private void FtxtColor2_PreviewMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            //Ventanas.VColor vc = new (txtColor2.Text)
            //{
            //    Owner = this
            //};
            //if (vc.ShowDialog() == true)
            //{
            //    txtColor2.Text = vc.ColorSt;
            //    System.Windows.Media.Color c = vc.ColorSB;
            //    if (cbLed.SelectedIndex == 0)
            //    {
            //        txtColor1.Text = txtColor1.Text[..1] + ";0;0;";
            //        c.B = 0;
            //        c.G = 0;
            //    }
            //    rColor2.Fill = new System.Windows.Media.SolidColorBrush(c);
            //}
        }

        private void FcbLed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (!this.IsLoaded) return;
            //((ComboBoxItem)cbModo.Items[1]).IsEnabled = true;
            //((ComboBoxItem)cbModo.Items[2]).IsEnabled = true;
            //((ComboBoxItem)cbModo.Items[3]).IsEnabled = true;
            //((ComboBoxItem)cbModo.Items[4]).IsEnabled = true;
            //((ComboBoxItem)cbModo.Items[6]).IsEnabled = true;
            //if (cbLed.SelectedIndex == 0)
            //{
            //    txtColor1.Text = "0;0;7";
            //    txtColor2.Text = "7;0;0";
            //    txtColor2.IsEnabled = true;
            //    rColor1.Fill = System.Windows.Media.Brushes.Blue;
            //    rColor2.Fill = System.Windows.Media.Brushes.Red;
            //    ((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
            //    ((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
            //    ((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
            //}
            //else if (cbLed.SelectedIndex == 1)
            //{
            //    txtColor1.Text = "7;0;0";
            //    txtColor2.Text = "0;0;0";
            //    txtColor2.IsEnabled = false;
            //    rColor1.Fill = System.Windows.Media.Brushes.Red;
            //    rColor2.Fill = System.Windows.Media.Brushes.Black;
            //    ((ComboBoxItem)cbModo.Items[1]).IsEnabled = false;
            //    ((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
            //    ((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
            //    ((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
            //    ((ComboBoxItem)cbModo.Items[6]).IsEnabled = false;
            //}
            //else
            //{
            //    txtColor1.Text = "7;7;7";
            //    txtColor2.Text = "7;7;7";
            //    txtColor1.IsEnabled = true;
            //    txtColor2.IsEnabled = true;
            //    rColor1.Fill = System.Windows.Media.Brushes.White;
            //    rColor2.Fill = System.Windows.Media.Brushes.White;
            //}
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic.Visibility = Visibility.Visible;
        }

        public void GoToAdvanced()
        {
            PanelBasic.Visibility = Visibility.Collapsed;
        }

        //#region "Gladiator NXT"
        //private void Leds(byte nLed, CEnums.LedOrder orden, CEnums.ModoColor mColor, string scolor1, string scolor2)
        //{
        //	byte[] cmds = [0, 0, 0, 0];
        //	ushort color1 = (ushort)(byte.Parse(scolor1.Split(';')[0]) << 8);
        //	color1 |= (ushort)(byte.Parse(scolor1.Split(';')[1]) << 4);
        //	color1 |= byte.Parse(scolor1.Split(';')[2]);
        //	ushort color2 = (ushort)(byte.Parse(scolor2.Split(';')[0]) << 8);
        //	color2 |= (ushort)(byte.Parse(scolor2.Split(';')[1]) << 4);
        //	color2 |= byte.Parse(scolor2.Split(';')[2]);

        //	cmds[0] = nLed; //base 0, joy1 10, joy2 11

        //	cmds[1] |= (byte)(color1 >> 8);//r;
        //	cmds[1] |= (byte)((color1 & 0x70) >> 1); //g
        //	byte color = (byte)(color1 & 0x07); //b
        //	cmds[1] |= (byte)((color & 0x3) << 6);
        //	cmds[2] |= (byte)(color >> 2);

        //	cmds[2] |= (byte)(color2 >> 7); //r
        //	cmds[2] |= (byte)(color2 & 0x70); //g
        //	color = (byte)(color2 & 0x07); //b
        //	cmds[2] |= (byte)((color & 1) << 7);
        //	cmds[3] |= (byte)(color >> 1);

        //	cmds[3] = (byte)((byte)orden << 2);
        //	cmds[3] |= (byte)((byte)mColor << 5);

        //	ushort[] bloque = new ushort[4];
        //	CommandType tipo = CommandType.VkbGladiatorNxtLeds;
        //	bloque[0] = (ushort)((byte)tipo + (cmds[0] << 8));
        //	bloque[1] = (ushort)((byte)tipo + (cmds[1] << 8));
        //	bloque[2] = (ushort)((byte)tipo + (cmds[2] << 8));
        //	bloque[3] = (ushort)((byte)tipo + (cmds[3] << 8));
        //	Insertar(bloque, false);
        //}
        //#endregion
    }
}
