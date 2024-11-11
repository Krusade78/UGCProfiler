using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlKeyboard : UserControl , IChangeMode
    {
        private readonly System.Collections.Generic.List<byte> keys = [];
        private bool[] keyboardStatus = new bool[255];
        private bool txtKeysFocused = false;

        public CtlKeyboard()
        {
            this.InitializeComponent();
            Panel2.Translation += new System.Numerics.Vector3(0, 0, 16);
            LoadTemplate();
        }

        #region "keys"
        private void ButtonPress_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237)
                return;
            else
            {
                ((EditedMacro)DataContext).Insert([(ushort)((byte)CommandType.Key + (ushort)(ComboBox1.SelectedIndex << 8))], false);
            }
        }

        private void ButtonRelease_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237)
                return;
            else
            {
                ((EditedMacro)DataContext).Insert([(ushort)(((byte)CommandType.Key | (byte)CommandType.Release) + (ushort)(ComboBox1.SelectedIndex << 8))], false);
            }
        }

        private void ButtonNormal_Click(object sender, RoutedEventArgs e)
        {
            AddKeys(false);
        }

        private void ButtonHold_Click(object sender, RoutedEventArgs e)
        {
            AddKeys(true);
        }

        private void TextBoxKey_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxKey.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
            TextBoxKey.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.GreenYellow);
            TextBoxKey.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
            txtKeysFocused = false;
        }

        private void TextBoxKey_GotFocus(object sender, RoutedEventArgs e)
        {
            keys.Clear();
            TextBoxKey.Text = "";
            TextBoxKey.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LimeGreen);
            TextBoxKey.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
            TextBoxKey.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
            txtKeysFocused = true;
            keyboardStatus = new bool[255];
        }

        private void TextBoxKey_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (txtKeysFocused)
            {
                ReadKeyboard(e.Key, e.KeyStatus);
                e.Handled = true;
            }
        }

        private void TextBoxKey_PreviewKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (txtKeysFocused)
            {
                ReadKeyboard(e.Key, e.KeyStatus);
                e.Handled = true;
            }
        }
        #endregion

        #region "Templates"
        private void LoadTemplate()
        {
            if (System.IO.File.Exists($".\\Language\\keytemplate.{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.txt"))
            {
                using System.IO.StreamReader f = new($".\\Language\\keytemplate.{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.txt");
                while (f.Peek() >= 0)
                {
                    string line = f.ReadLine();
                    ComboBox1.Items.Add(line);
                    GroupedCommand.Keys.Add(line);
                }
            }
            else if (System.IO.File.Exists($".\\Language\\keytemplate.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.txt"))
            {
                using System.IO.StreamReader f = new($".\\Language\\keytemplate.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.txt");
                while (f.Peek() >= 0)
                {
                    string line = f.ReadLine();
                    ComboBox1.Items.Add(line);
                    GroupedCommand.Keys.Add(line);
                }
            }
            else
            {
                using System.IO.StreamReader f = new(typeof(App).Assembly.GetManifestResourceStream("Profiler.Language.keytemplate-en.txt"));
                while (f.Peek() >= 0)
                {
                    string line = f.ReadLine();
                    ComboBox1.Items.Add(line);
                    GroupedCommand.Keys.Add(line);
                }
            }

            if (ComboBox1.Items.Count > 0)
            {
                ComboBox1.SelectedIndex = 0;
            }
            else
            {
                ButtonPresionar.IsEnabled = false;
                ButtonSoltar.IsEnabled = false;
            }
        }
        #endregion

        public void GoToBasic()
        {
            KeyPanel.Visibility = Visibility.Visible;  
        }

        public void GoToAdvanced()
        {
            KeyPanel.Visibility = Visibility.Collapsed;
        }

        #region
        private void AddKeys(bool hold)
        {
            if (keys.Count == 0)
                return;
            if ((((EditedMacro)DataContext).GetCount() + (hold ? 1 : 0) + (keys.Count * 2)) > 237)
                return;

            if (KeyPanel.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
            }

            System.Collections.Generic.List<uint> block = [];
            for (byte i = 0; i < keys.Count; i++)
            {
                byte k = keys[i];
                block.Add((uint)((byte)CommandType.Key + (k << 8)));
            }
            if (hold)
            {
                block.Add((uint)CommandType.Hold);
            }
            for (int j = keys.Count - 1; j >= 0; j--)
            {
                byte k = keys[j];
                block.Add((uint)((byte)(CommandType.Release | CommandType.Key) + (k << 8)));
            }
            ((EditedMacro)DataContext).Insert([.. block], true);
        }

        private void ReadKeyboard(Windows.System.VirtualKey vk, Windows.UI.Core.CorePhysicalKeyStatus status)
        {
            string s = "";
            //bool[] buff = new bool[255];
            //for (Windows.System.VirtualKey k = 0; k <= (Windows.System.VirtualKey)255; k++)
            //{
            //    if (Microsoft.UI.Input.InputKeyboardSource.(k) == Windows.UI.Core.CoreVirtualKeyStates.Down)// Keyboard.IsKeyDown(k))
            //        buff[(byte)k] = true;//buff[KeyInterop.VirtualKeyFromKey(k)] = true;
            //}
            keyboardStatus[(byte)vk] = !status.IsKeyReleased;
            keys.Clear();
            if (keyboardStatus[0x10])
            {
                //keyboardStatus[0x10] = false;
                //if (!keyboardStatus[0xa0] && !keyboardStatus[0xa1])
                //{
                //    keyboardStatus[0xa0] = true;
                //}
                s += ((s == "") ? "" : " + ") + "May.";
                keys.Add(0x10);
            }
            if (keyboardStatus[0x11])
            {
                //keyboardStatus[0x11] = false;
                //if (!keyboardStatus[0xa2] && !keyboardStatus[0xa3])
                //{
                //    keyboardStatus[0xa2] = true;
                //}
                s += ((s == "") ? "" : " + ") + "Control";
                keys.Add(0x11);
            }
            if (keyboardStatus[0x12])
            {
                //keyboardStatus[0x12] = false;
                //if (!keyboardStatus[0xa4] && !keyboardStatus[0xa5])
                //{
                //    keyboardStatus[0xa4] = true;
                //}
                s += ((s == "") ? "" : " + ") + "Menu";
                keys.Add(0x12);
            }
            if (keyboardStatus[0xa0])
            {
                s += ((s == "") ? "" : " + ") + "May.I";
                keys.Add(0xa0);
            }
            if (keyboardStatus[0xa1])
            {
                s += ((s == "") ? "" : " + ") + "May.D";
                keys.Add(0xa1);
            }
            if (keyboardStatus[0xa2])
            {
                s += ((s == "") ? "" : " + ") + "ControlI";
                keys.Add(0xa2);
            }
            if (keyboardStatus[0xa3])
            {
                s += ((s == "") ? "" : " + ") + "ControlD";
                keys.Add(0xa3);
            }
            if (keyboardStatus[0xa4])
            {
                s += ((s == "") ? "" : " + ") + "AltI";
                keys.Add(0xa4);
            }
            if (keyboardStatus[0xa5])
            {
                s += ((s == "") ? "" : " + ") + "AltD";
                keys.Add(0xa5);
            }
            if (keyboardStatus[0x5b])
            {
                s += ((s == "") ? "" : " + ") + "WinI";
                keys.Add(0x5b);
            }
            if (keyboardStatus[0x5c])
            {
                s += ((s == "") ? "" : " + ") + "WinD";
                keys.Add(0x5c);
            }
            for (byte i = 1; i < 255; i++)
            {
                if (i is not 0x5B and not 0x5C and (< 0xA0 or > 0xA5) and (< 0x10 or > 0x12))
                {
                    if (keyboardStatus[i])
                    {
                        s += ((s == "") ? "" : " + ") + ((Windows.System.VirtualKey)i).ToString();//; KeyInterop.KeyFromVirtualKey(i).ToString();
                        keys.Add(i);
                        ButtonNormal.Focus(FocusState.Programmatic);
                    }
                }
            }
            TextBoxKey.Text = s;
        }

        #endregion
    }
}
