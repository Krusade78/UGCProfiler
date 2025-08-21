using Avalonia.Interactivity;
using Avalonia.Controls;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public partial class CtlVKBGladiatorNXT : UserControl, IChangeMode
    {
        bool changing = false;

        public CtlVKBGladiatorNXT()
        {
            this.InitializeComponent();
            changing = true;
            cpColor2.Color = Avalonia.Media.Color.FromArgb(255, 255, 0, 0);
            cpColor1.Color = Avalonia.Media.Color.FromArgb(255, 0, 0, 255);
            txtColor1.Text = "0;0;7";
            changing = false;
        }

        #region "Leds NXT"
        private void ButtonLed_Click(object sender, RoutedEventArgs e)
        {
            Leds((byte)((cbLed.SelectedIndex == 1) ? 11 : ((cbLed.SelectedIndex == 2) ? 10 : 0)), (CEnums.LedOrder)cbOrden.SelectedIndex, (CEnums.ColorMode)cbModo.SelectedIndex, txtColor1.Text, txtColor2.Text);
        }

        private void Color1Picker_ColorChanged(object sender, ColorChangedEventArgs args)
        {
            if (!changing)
            {
                txtColor1.Text = (args.NewColor.R * 7 / 255) + ";" + (args.NewColor.G * 7 / 255) + ";" + (args.NewColor.B * 7 / 255);
                Avalonia.Media.Color c = Avalonia.Media.Color.FromArgb(255, ColorFromLed(args.NewColor.R), ColorFromLed(args.NewColor.G), ColorFromLed(args.NewColor.B));
                if (cbLed.SelectedIndex == 0)
                {
                    txtColor1.Text = "0;0;" + txtColor1.Text.Remove(0, 4);
                    c = Avalonia.Media.Color.FromArgb(255, 0, 0, c.B);
                }
                else if (cbLed.SelectedIndex == 1)
                {
                    txtColor1.Text = txtColor1.Text[..1] + ";0;0;";
                    c = Avalonia.Media.Color.FromArgb(255, c.R, 0, 0);
                }
                rColor1.Fill = new Avalonia.Media.SolidColorBrush(c);
                changing = true;
                cpColor1.Color = c;
                changing = false;
            }
        }

        private void Color2Picker_ColorChanged(object sender, ColorChangedEventArgs args)
        {
            if (!changing)
            {
                txtColor2.Text = (args.NewColor.R * 7 / 255) + ";" + (args.NewColor.G * 7 / 255) + ";" + (args.NewColor.B * 7 / 255); ;
                Avalonia.Media.Color c = Avalonia.Media.Color.FromArgb(255, ColorFromLed(args.NewColor.R), ColorFromLed(args.NewColor.G), ColorFromLed(args.NewColor.B));
                if (cbLed.SelectedIndex == 0)
                {
                    txtColor2.Text = txtColor2.Text[..1] + ";0;0;";
                    c = Avalonia.Media.Color.FromArgb(255, c.R, 0, 0);
                }
                rColor2.Fill = new Avalonia.Media.SolidColorBrush(c);
                changing = true;
                cpColor2.Color = c;
                changing = false;
            }
        }

        private static byte ColorFromLed(byte color) => (byte)(color * 7 / 255 * 255 / 7);


        private void FcbLed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            ((ComboBoxItem)cbModo.Items[1]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[2]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[3]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[4]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[5]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[6]).IsEnabled = true;
            if (cbLed.SelectedIndex == 0)
            {
                txtColor1.Text = "0;0;7";
                txtColor2.Text = "7;0;0";
                changing = true;
                cpColor2.Color = Avalonia.Media.Color.FromArgb(255, 255, 0, 0);
                cpColor1.Color = Avalonia.Media.Color.FromArgb(255, 0, 0, 255);
                changing = false;
                sbColor1.IsEnabled = true;
                sbColor2.IsEnabled = true;
                rColor1.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Blue);
                rColor2.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red);
                ((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
            }
            else if (cbLed.SelectedIndex == 1)
            {
                txtColor1.Text = "7;0;0";
                txtColor2.Text = "0;0;0";
                changing = true;
                cpColor1.Color = Avalonia.Media.Color.FromArgb(255, 255, 0, 0);
                cpColor2.Color = Avalonia.Media.Color.FromArgb(255, 0, 0, 0);
                changing = false;
                sbColor1.IsEnabled = true;
                sbColor2.IsEnabled = false;
                rColor1.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red);
                rColor2.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
                ((ComboBoxItem)cbModo.Items[1]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[6]).IsEnabled = false;
            }
            else
            {
                txtColor1.Text = "7;7;7";
                txtColor2.Text = "0;0;0";
                changing = true;
                cpColor1.Color = Avalonia.Media.Color.FromArgb(255, 255, 255, 255);
                cpColor2.Color = Avalonia.Media.Color.FromArgb(255, 0, 0, 0);
                changing = false;
                sbColor1.IsEnabled = true;
                sbColor2.IsEnabled = true;
                rColor1.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White);
                rColor2.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black);
                //((ComboBoxItem)cbModo.Items[1]).IsEnabled = false;
                //((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
                //((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
                //((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
                //((ComboBoxItem)cbModo.Items[6]).IsEnabled = false;
            }
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic.IsVisible = true;
        }

        public void GoToAdvanced()
        {
            PanelBasic.IsVisible = false;
        }

        #region "Gladiator NXT"
        private void Leds(byte nLed, CEnums.LedOrder orden, CEnums.ColorMode mColor, string scolor1, string scolor2)
        {
            if (((EditedMacro)DataContext).LimitReached(4)) return;

            byte[] cmds = [0, 0, 0, 0];
            ushort color1 = (ushort)(byte.Parse(scolor1.Split(';')[0]) << 8);
            color1 |= (ushort)(byte.Parse(scolor1.Split(';')[1]) << 4);
            color1 |= byte.Parse(scolor1.Split(';')[2]);
            ushort color2 = (ushort)(byte.Parse(scolor2.Split(';')[0]) << 8);
            color2 |= (ushort)(byte.Parse(scolor2.Split(';')[1]) << 4);
            color2 |= byte.Parse(scolor2.Split(';')[2]);

            cmds[0] = nLed; //base 0, joy1 10, joy2 11

            cmds[1] |= (byte)(color1 >> 8);//r;
            cmds[1] |= (byte)((color1 & 0x70) >> 1); //g
            byte color = (byte)(color1 & 0x07); //b
            cmds[1] |= (byte)((color & 0x3) << 6);
            cmds[2] |= (byte)(color >> 2);

            cmds[2] |= (byte)(color2 >> 7); //r
            cmds[2] |= (byte)(color2 & 0x70); //g
            color = (byte)(color2 & 0x07); //b
            cmds[2] |= (byte)((color & 1) << 7);
            cmds[3] |= (byte)(color >> 1);

            cmds[3] = (byte)((byte)orden << 2);
            cmds[3] |= (byte)((byte)mColor << 5);

            uint[] block = new uint[4];
            CommandType tipo = CommandType.VkbGladiatorNxtLeds;
            block[0] = (uint)((byte)tipo + (cmds[0] << 8));
            block[1] = (uint)((byte)tipo + (cmds[1] << 8));
            block[2] = (uint)((byte)tipo + (cmds[2] << 8));
            block[3] = (uint)((byte)tipo + (cmds[3] << 8));
            ((EditedMacro)DataContext).Insert(block, true);
        }
        #endregion
    }
}
