using System;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Input;

namespace Calibrator
{
	/// <summary>
	/// Lógica de interacción para Info.xaml
	/// </summary>
	internal partial class Info : UserControl
	{
		private readonly System.Collections.Generic.List<Key> shifted = new System.Collections.Generic.List<Key>();
		private readonly System.Collections.Generic.List<Key> teclas = new System.Collections.Generic.List<Key>();
		private readonly System.Collections.Generic.List<string> raton = new System.Collections.Generic.List<string>();
		private readonly System.Windows.Threading.DispatcherTimer borradoRueda = new System.Windows.Threading.DispatcherTimer();
		private short[,] hidReportEjes = new short[3, 8];
		private byte[,] hidReportResto = new byte[3, 8];
		private byte joySel = 0;

		public Info()
		{
			InitializeComponent();
			borradoRueda.Interval = new TimeSpan(5000000);
		}

		private void FcheckBox_Checked(object sender, RoutedEventArgs e)
		{
			((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(true);
		}
		private void FcheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(false);
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			borradoRueda.Tick += BorradoRueda_Tick;
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			borradoRueda.Tick -= BorradoRueda_Tick;
		}

		public void ActualizarEstado(byte[] hidData, byte joy, bool sinReport)
		{
			if (!sinReport)
			{
				for (byte i = 0; i < 8; i++)
				{
					hidReportEjes[joy - 1, i] = (short)(((int)hidData[(i * 2) + 1] << 8) | hidData[i * 2]);
					hidReportResto[joy - 1, i] = hidData[16 + i];
				}
			}
			Actualizar();
		}

		private void Actualizar()
		{ 
			int x = hidReportEjes[joySel, 0];
			int y = hidReportEjes[joySel, 1];
			Labelxy.Text = x + " # " + y;
			ejeXY.Margin = new Thickness(x+ 32767, y + 32767, 0, 0);

			int z = hidReportEjes[joySel, 2];
			Labelz.Text = z.ToString();
			ejeZ.Height = z + 32767;

			int r = hidReportEjes[joySel, 3];
			Labelr.Text = r.ToString();
			ejeR.Width = r + 32767;

			int ry = hidReportEjes[joySel, 4];
			Labelrx.Text = ry.ToString();
			double angulo = (Math.PI * (((double)360 / 65535) * (ry + 32767))) / 180;
			String ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
			String ax = (ry == 32767) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
			ejeRX.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

			int rz = hidReportEjes[joySel, 5];
			Labelry.Text = rz.ToString();
			angulo = (Math.PI * (((double)360 / 65535) * (rz + 32767))) / 180;
			ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
			ax = (rz == 32767) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
			ejeRY.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

			int sl1 = hidReportEjes[joySel, 6];
			Labelsl2.Text = sl1.ToString();
			ejeSl2.Width = sl1 + 32767;

			int sl2 = hidReportEjes[joySel, 7];
			Labelsl3.Text = sl2.ToString();
			ejeSl3.Width = sl2 + 32767;

			int my = sl2;
			int mx = sl1;
			Labelmxy.Text = "mX: " + mx + "\n" + "mY: " + my;
			ejeMXY.Margin = new Thickness(mx + 32767, my + 32767, 0, 0);

			System.Windows.Shapes.Ellipse[] bts = new System.Windows.Shapes.Ellipse[] { bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8,
																					bt9, bt10, bt11, bt12, bt13, bt14, bt15, bt16,
																					bt17, bt18, bt19, bt20, bt21, bt22, bt23, bt24,
																					bt25, bt26, bt27, bt28, bt29, bt30, bt31, bt32};
			for (int i = 0; i < 32; i++)
			{
				bts[i].Visibility = ((hidReportResto[joySel, 4 + (i / 8)] & (1 << (i % 8))) != 0) ? Visibility.Visible : Visibility.Hidden;
			}

				pathP1.Visibility = (hidReportResto[joySel, 0] == 0) ? Visibility.Hidden : Visibility.Visible;
				pathP1.RenderTransform = new System.Windows.Media.RotateTransform((hidReportResto[joySel, 0] > 5) ? (hidReportResto[joySel, 0] - 9) * 45 : (hidReportResto[joySel, 0] - 1) * 45);
				pathP2.Visibility = (hidReportResto[joySel, 1] == 0) ? Visibility.Hidden : Visibility.Visible;
				pathP2.RenderTransform = new System.Windows.Media.RotateTransform((hidReportResto[joySel, 1] > 5) ? (hidReportResto[joySel, 1] - 9) * 45 : (hidReportResto[joySel, 1] - 1) * 45);
				pathP3.Visibility = (hidReportResto[joySel, 2] == 0) ? Visibility.Hidden : Visibility.Visible;
				pathP3.RenderTransform = new System.Windows.Media.RotateTransform((hidReportResto[joySel, 2] > 5) ? (hidReportResto[joySel, 2] - 9) * 45 : (hidReportResto[joySel, 2] - 1) * 45);
				pathP4.Visibility = (hidReportResto[joySel, 3] == 0) ? Visibility.Hidden : Visibility.Visible;
				pathP4.RenderTransform = new System.Windows.Media.RotateTransform((hidReportResto[joySel, 3] > 5) ? (hidReportResto[joySel, 3] - 9) * 45 : (hidReportResto[joySel, 3] - 1) * 45);
		}

		public void ActualizarTeclado(CRawInput.RAWINPUTKEYBOARD data)
		{
			Key key = KeyInterop.KeyFromVirtualKey(data.VKey);
			if ((data.Flags & CRawInput.RawKeyboardFlags.KeyBreak) == 0)
			{
				if (key.ToString().Contains("Ctrl") || key.ToString().Contains("Alt") || key.ToString().Contains("Win"))
				{
					if (shifted.IndexOf(key) == -1)  shifted.Add(key);
				}
				else
				{
					if (teclas.IndexOf(key) == -1) teclas.Add(key);
				}
			}
			else
			{
				if (key.ToString().Contains("Ctrl") || key.ToString().Contains("Alt") || key.ToString().Contains("Win"))
				{
					if (shifted.IndexOf(key) != -1) shifted.Remove(key);
				}
				else
				{
					if (teclas.IndexOf(key) != -1) teclas.Remove(key);
				}
			}

			txtTeclado.Text = "";
			foreach (Key k in shifted)
			{
				txtTeclado.Text += " + " + k.ToString(); ;
			}
			foreach (Key k in teclas)
			{
				txtTeclado.Text += " + " + k.ToString();
			}
			if (txtTeclado.Text.StartsWith(" + "))
			{
				txtTeclado.Text = txtTeclado.Text.Remove(0, 3);
			}
		}

		public void ActualizarRaton(CRawInput.RAWINPUTMOUSE data)
		{
			string[] botones = new string[] { "Bt1", "-Bt1", "Bt2", "-Bt2", "Bt3", "-Bt3", "Bt4", "-Bt4", "Bt5", "-Bt5", "RuedaV", "RuedaH" };
			uint[] mapa = new uint[] { 0x01, 0x02, 0x4, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800 };
			string boton = "";
			for (int i = 0; i < mapa.Length; i++)
			{
				if ((mapa[i] & data.usButtonFlags) != 0)
				{
					if (botones[i].StartsWith("-"))
					{
						if (raton.IndexOf(botones[i].Remove(0, 1)) != -1) raton.Remove(botones[i].Remove(0, 1));
					}
					else
					{
						string txt = botones[i] + ((data.usButtonData > 0) ? "+" : "");
						txt += (data.usButtonData < 0) ? "-" : "";
						if (raton.IndexOf(txt) == -1)
						{
							raton.Add(txt);
						}
						if ((data.usButtonData > 0) || (data.usButtonData < 0))
						{
							borradoRueda.Start();
						}
						
					}
					boton += botones[i];
				}
			}

			txtRaton.Text = "";
			foreach (string sr in raton)
			{
				txtRaton.Text += " " + sr;
			}
			if (txtTeclado.Text.StartsWith(" "))
			{
				txtTeclado.Text = txtTeclado.Text.Remove(0, 1);
			}
			txtRaton.Text = $"X: {data.lLastX} Y: {data.lLastY}" + txtRaton.Text;
		}

		private void BorradoRueda_Tick(object sender, EventArgs e)
		{
			int i = raton.Count - 1;
			while(i >= 0)
			{
				if (raton[i].StartsWith("RuedaV") || raton[i].StartsWith("RuedaH"))
				{
					txtRaton.Text = txtRaton.Text.Replace(raton[i], "");
					raton.RemoveAt(i);
					
				}
				i--;
			}
			borradoRueda.Stop();
		}

		private void ToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			if (this.IsLoaded)
			{
				tbJ2.IsChecked = false;
				tbJ3.IsChecked = false;
				joySel = 0;
				Actualizar();
			}
		}

		private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
		{
			tbJ1.IsChecked = false;
			tbJ3.IsChecked = false;
			joySel = 1;
			Actualizar();
		}

		private void ToggleButton_Checked_2(object sender, RoutedEventArgs e)
		{
			tbJ2.IsChecked = false;
			tbJ1.IsChecked = false;
			joySel = 2;
			Actualizar();
		}
	}
}
