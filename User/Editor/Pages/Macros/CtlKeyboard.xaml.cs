using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlKeyboard : UserControl , IChangeMode
    {
        public CtlKeyboard()
        {
            this.InitializeComponent();
            Panel2.Translation += new System.Numerics.Vector3(0, 0, 16);
            LoadTemplate();
        }

        #region "teclas"
        private void ButtonPresionar_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237)
            //    return;
            //else
            //{
            //    Insertar([(ushort)((byte)CommandType.Key + (ushort)(ComboBox1.SelectedIndex << 8))], false);
            //}
        }

        private void ButtonSoltar_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237)
            //    return;
            //else
            //{
            //    Insertar([(ushort)(((byte)CommandType.Key | (byte)CommandType.Release) + (ushort)(ComboBox1.SelectedIndex << 8))], false);
            //}
        }

        private void ButtonNormal_Click(object sender, RoutedEventArgs e)
        {
            //TeclasPulsar(false);
        }

        private void ButtonMantener_Click(object sender, RoutedEventArgs e)
        {
            //TeclasPulsar(true);
        }

        private bool teclasActivado = false;
        private void TextBoxTecla_LostFocus(object sender, RoutedEventArgs e)
        {
            //TextBoxTecla.Background = System.Windows.Media.Brushes.Black;
            //TextBoxTecla.Foreground = System.Windows.Media.Brushes.GreenYellow;
            //TextBoxTecla.FontWeight = FontWeights.Normal;
            //teclasActivado = false;
        }

        private void TextBoxTecla_GotFocus(object sender, RoutedEventArgs e)
        {
            //teclas.Clear();
            //TextBoxTecla.Text = "";
            //TextBoxTecla.Background = System.Windows.Media.Brushes.LimeGreen;
            //TextBoxTecla.Foreground = System.Windows.Media.Brushes.Black;
            //TextBoxTecla.FontWeight = FontWeights.Bold;
            //teclasActivado = true;
        }

        private void TextBoxTecla_PreviewKeyDown(object sender, RoutedEventArgs e)
        {
            //if (teclasActivado)
            //{
            //    LeerTeclado();
            //    e.Handled = true;
            //}
        }

        private void TextBoxTecla_PreviewKeyUp(object sender, RoutedEventArgs e)
        {
            //if (teclasActivado)
            //{
            //    LeerTeclado();
            //    e.Handled = true;
            //}
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
                    string linea = f.ReadLine();
                    ComboBox1.Items.Add(linea);
                    GroupedCommand.Keys.Add(linea);
                }
            }
            else if (System.IO.File.Exists($".\\Language\\keytemplate.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.txt"))
            {
                using System.IO.StreamReader f = new($".\\Language\\keytemplate.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.txt");
                while (f.Peek() >= 0)
                {
                    string linea = f.ReadLine();
                    ComboBox1.Items.Add(linea);
                    GroupedCommand.Keys.Add(linea);
                }
            }
            else
            {
                using System.IO.StreamReader f = new(typeof(App).Assembly.GetManifestResourceStream("Profiler.Language.keytemplate-en.txt"));
                while (f.Peek() >= 0)
                {
                    string linea = f.ReadLine();
                    ComboBox1.Items.Add(linea);
                    GroupedCommand.Keys.Add(linea);
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

        //#region "teclas"
        //private void TeclasPulsar(bool mantener)
        //{
        //	if (teclas.Count == 0)
        //		return;
        //	if ((GetCuenta() + ((mantener) ? 1 : 0) + (teclas.Count * 2)) > 237)
        //		return;

        //	if (RadioButtonBasico.IsChecked == true)
        //	{
        //		dsMacros.MACROS.Clear();
        //	}

        //	List<ushort> bloque = [];
        //	for (byte i = 0; i < teclas.Count; i++)
        //	{
        //		int k = teclas[i];
        //		if (k > -1) bloque.Add((ushort)((byte)CommandType.Key + (k << 8)));
        //	}
        //	if (mantener)
        //	{
        //		bloque.Add((ushort)CommandType.Hold);
        //	}
        //	for (int j = teclas.Count - 1; j >= 0; j--)
        //	{
        //		int k = teclas[j];
        //		if (k > -1)
        //		{
        //			bloque.Add((ushort)((byte)(CommandType.Release | CommandType.Key) + (k << 8)));
        //		}
        //	}
        //	Insertar([.. bloque], true);
        //}

        //private void LeerTeclado()
        //{
        //	string s = "";
        //	bool[] buff = new bool[256];
        //	for (Key k = Key.Cancel; k <= Key.DeadCharProcessed; k++)
        //	{
        //		if (Keyboard.IsKeyDown(k))
        //			buff[KeyInterop.VirtualKeyFromKey(k)] = true;
        //	}
        //	teclas.Clear();
        //	if (buff[0x10])
        //	{
        //		buff[0x10] = false;
        //		if (!buff[0xa0] && !buff[0xa1])
        //		{
        //			buff[0xa0] = true;
        //		}
        //	}
        //	if (buff[0x11])
        //	{
        //		buff[0x11] = false;
        //		if (!buff[0xa2] && !buff[0xa3])
        //		{
        //			buff[0xa2] = true;
        //		}
        //	}
        //	if (buff[0x12])
        //	{
        //		buff[0x12] = false;
        //		if (!buff[0xa4] && !buff[0xa5])
        //		{
        //			buff[0xa4] = true;
        //		}
        //	}
        //	if (buff[0xa0])
        //	{
        //		s += ((s == "") ? "" : " + ") + "May.I";
        //		teclas.Add(0xa0);
        //	}
        //	if (buff[0xa1])
        //	{
        //		s += ((s == "") ? "" : " + ") + "May.D";
        //		teclas.Add(0xa1);
        //	}
        //	if (buff[0xa2])
        //	{
        //		s += ((s == "") ? "" : " + ") + "ControlI";
        //		teclas.Add(0xa2);
        //	}
        //	if (buff[0xa3])
        //	{
        //		s += ((s == "") ? "" : " + ") + "ControlD";
        //		teclas.Add(0xa3);
        //	}
        //	if (buff[0xa4])
        //	{
        //		s += ((s == "") ? "" : " + ") + "AltI";
        //		teclas.Add(0xa4);
        //	}
        //	if (buff[0xa5])
        //	{
        //		s += ((s == "") ? "" : " + ") + "AltD";
        //		teclas.Add(0xa5);
        //	}
        //	if (buff[0x5b])
        //	{
        //		s += ((s == "") ? "" : " + ") + "WinI";
        //		teclas.Add(0x5b);
        //	}
        //	if (buff[0x5c])
        //	{
        //		s += ((s == "") ? "" : " + ") + "WinD";
        //		teclas.Add(0x5c);
        //	}
        //	for (ushort i = 1; i < 255; i++)
        //	{
        //		if (i is not 0x5B and not 0x5C and (< 0xA0 or > 0xA5))
        //		{
        //			if (buff[i])
        //			{
        //				s += ((s == "") ? "" : " + ") + KeyInterop.KeyFromVirtualKey(i).ToString();
        //				teclas.Add((byte)i);
        //				ButtonNormal.Focus();
        //			}
        //		}
        //	}
        //	TextBoxTecla.Text = s;
        //}

        //#endregion
    }
}
