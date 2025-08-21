//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Globalization;
//using System.Windows.Input;
//using Comunes.Calibrado;

//namespace Calibrator
//{
//	/// <summary>
//	/// Lógica de interacción para Info.xaml
//	/// </summary>
//	internal partial class Info : UserControl
//	{
//		private class CEje
//		{
//			public uint Posicion { get; set; }
//			public uint Maximo { get; set; }
//		}

//		private readonly System.Collections.Generic.List<Key> shifted = new();
//		private readonly System.Collections.Generic.List<Key> teclas = new();
//		private readonly System.Collections.Generic.List<string> raton = new();
//		private readonly System.Windows.Threading.DispatcherTimer borradoRueda = new();
//		private readonly CEje[,] hidReportEjes = new CEje[3, 8];
//		private readonly uint[] hidBotones = new uint[3];
//        private readonly short[,] hidPovs = new short[3, 4];
//        private readonly DatosJoy[] dispositivos = new DatosJoy[3];
//		private byte joySel = 0;

//		public Info()
//		{
//			InitializeComponent();
//			borradoRueda.Interval = new TimeSpan(5000000);
//			for (byte i = 0; i < 8; i++)
//			{
//				hidReportEjes[0, i] = new();
//				hidReportEjes[1, i] = new();
//				hidReportEjes[2, i] = new();
//				hidReportEjes[0, i].Maximo = 1;
//				hidReportEjes[1, i].Maximo = 1;
//				hidReportEjes[2, i].Maximo = 1;
//			}
//		}

//		private void FcheckBox_Checked(object sender, RoutedEventArgs e)
//		{
//			((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(true);
//		}
//		private void FcheckBox_Unchecked(object sender, RoutedEventArgs e)
//		{
//			((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(false);
//		}

//		private void UserControl_Loaded(object sender, RoutedEventArgs e)
//		{
//			borradoRueda.Tick += BorradoRueda_Tick;
//		}

//		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
//		{
//			borradoRueda.Tick -= BorradoRueda_Tick;
//		}

//		public void ActualizarEstado(string nombre, byte[] hidData, byte joy)
//		{
//            if (dispositivos[joy] == null)
//            {
//                DatosJoy nuevo = DatosJoy.GetInfo(nombre, joy);
//				dispositivos[joy] = nuevo;
//				byte eje = 0;
//				foreach (DatosJoy.CUsage u in nuevo.Usages)
//				{
//					hidReportEjes[joy, eje++].Maximo = u.Rango;
//					if (eje == 8) { break; }
//				}
//            }
//            if (joySel != joy)
//            {
//                return;
//            }

//			uint[] pos = new uint[8];
//			short[] povs = new short[4];
//            dispositivos[joySel].ToHiddata(hidData, ref pos, ref hidBotones[joy], ref povs);
//			for (byte e = 0; e < 8; e++)
//			{
//				hidReportEjes[joy, e].Posicion = pos[e];
//			}
//            for (byte p = 0; p < 4; p++)
//            {
//                hidPovs[joy, p] = povs[p];
//            }
//            Actualizar();
//		}

//		private void Actualizar()
//		{
//			long x = hidReportEjes[joySel, 0].Posicion;
//			long y = hidReportEjes[joySel, 1].Posicion;
//			Labelxy.Text = x + " # " + y;
//			ejeXY.Margin = new Thickness(x * (65536 / hidReportEjes[joySel, 0].Maximo), y * (65536 / hidReportEjes[joySel, 1].Maximo), 0, 0);

//			long z = hidReportEjes[joySel, 2].Posicion;
//			Labelz.Text = z.ToString();
//			ejeZ.Height = z * (65536 / hidReportEjes[joySel, 2].Maximo);

//			long r = hidReportEjes[joySel, 3].Posicion;
//			Labelr.Text = r.ToString();
//			ejeR.Width = r * (65536 / hidReportEjes[joySel, 3].Maximo);

//			long ry = hidReportEjes[joySel, 4].Posicion;
//			Labelrx.Text = ry.ToString();
//			double angulo = (Math.PI * (((double)360 / hidReportEjes[joySel, 4].Maximo) * ry)) / 180;
//			string ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
//			string ax = (ry == hidReportEjes[joySel, 4].Maximo) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
//			ejeRX.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

//			long rz = hidReportEjes[joySel, 5].Posicion;
//			Labelry.Text = rz.ToString();
//			angulo = (Math.PI * (((double)360 / hidReportEjes[joySel, 5].Maximo) * rz)) / 180;
//			ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
//			ax = (rz == 32767) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
//			ejeRY.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

//			long sl1 = hidReportEjes[joySel, 6].Posicion;
//			Labelsl2.Text = sl1.ToString();
//			ejeSl2.Width = sl1 * (65536 / hidReportEjes[joySel, 6].Maximo);

//			long sl2 = hidReportEjes[joySel, 7].Posicion;
//			Labelsl3.Text = sl2.ToString();
//			ejeSl3.Width = sl2 * (65536 / hidReportEjes[joySel, 7].Maximo);

//			long my = sl2;
//			long mx = sl1;
//			Labelmxy.Text = "mX: " + mx + "\n" + "mY: " + my;
//			ejeMXY.Margin = new Thickness(mx * (65536 / hidReportEjes[joySel, 6].Maximo), my * (65536 / hidReportEjes[joySel, 7].Maximo), 0, 0);

//			System.Windows.Shapes.Ellipse[] bts = new System.Windows.Shapes.Ellipse[] { bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8,
//																					bt9, bt10, bt11, bt12, bt13, bt14, bt15, bt16,
//																					bt17, bt18, bt19, bt20, bt21, bt22, bt23, bt24,
//																					bt25, bt26, bt27, bt28, bt29, bt30, bt31, bt32};
//			for (int i = 0; i < 32; i++)
//			{
//				bts[i].Visibility = ((hidBotones[joySel] & (1 << i)) != 0) ? Visibility.Visible : Visibility.Hidden;
//			}

//				pathP1.Visibility = (hidPovs[joySel, 0] == -1) ? Visibility.Hidden : Visibility.Visible;
//				pathP1.RenderTransform = new System.Windows.Media.RotateTransform(hidPovs[joySel, 0] / 100);
//				pathP2.Visibility = (hidPovs[joySel, 1] == -1) ? Visibility.Hidden : Visibility.Visible;
//				pathP2.RenderTransform = new System.Windows.Media.RotateTransform(hidPovs[joySel, 1] / 100);
//				pathP3.Visibility = (hidPovs[joySel, 2] == -1) ? Visibility.Hidden : Visibility.Visible;
//				pathP3.RenderTransform = new System.Windows.Media.RotateTransform(hidPovs[joySel, 2] / 100);
//				pathP4.Visibility = (hidPovs[joySel, 3] == -1) ? Visibility.Hidden : Visibility.Visible;
//				pathP4.RenderTransform = new System.Windows.Media.RotateTransform(hidPovs[joySel, 3] / 100);
//		}

//		public void ActualizarTeclado(API.CRawInput.RAWINPUTKEYBOARD data)
//		{
//			Key key = KeyInterop.KeyFromVirtualKey(data.VKey);
//			if ((data.Flags & API.CRawInput.RawKeyboardFlags.KeyBreak) == 0)
//			{
//				if (key.ToString().Contains("Ctrl") || key.ToString().Contains("Alt") || key.ToString().Contains("Win"))
//				{
//					if (shifted.IndexOf(key) == -1)  shifted.Add(key);
//				}
//				else
//				{
//					if (teclas.IndexOf(key) == -1) teclas.Add(key);
//				}
//			}
//			else
//			{
//				if (key.ToString().Contains("Ctrl") || key.ToString().Contains("Alt") || key.ToString().Contains("Win"))
//				{
//					if (shifted.IndexOf(key) != -1) shifted.Remove(key);
//				}
//				else
//				{
//					if (teclas.IndexOf(key) != -1) teclas.Remove(key);
//				}
//			}

//			txtTeclado.Text = "";
//			foreach (Key k in shifted)
//			{
//				txtTeclado.Text += " + " + k.ToString(); ;
//			}
//			foreach (Key k in teclas)
//			{
//				txtTeclado.Text += " + " + k.ToString();
//			}
//			if (txtTeclado.Text.StartsWith(" + "))
//			{
//				txtTeclado.Text = txtTeclado.Text.Remove(0, 3);
//			}
//		}

//		public void ActualizarRaton(API.CRawInput.RAWINPUTMOUSE data)
//		{
//			string[] botones = new string[] { "Bt1", "-Bt1", "Bt2", "-Bt2", "Bt3", "-Bt3", "Bt4", "-Bt4", "Bt5", "-Bt5", "RuedaV", "RuedaH" };
//			uint[] mapa = new uint[] { 0x01, 0x02, 0x4, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800 };
//			string boton = "";
//			for (int i = 0; i < mapa.Length; i++)
//			{
//				if ((mapa[i] & data.usButtonFlags) != 0)
//				{
//					if (botones[i].StartsWith("-"))
//					{
//						if (raton.IndexOf(botones[i].Remove(0, 1)) != -1) raton.Remove(botones[i].Remove(0, 1));
//					}
//					else
//					{
//						string txt = botones[i] + ((data.usButtonData > 0) ? "+" : "");
//						txt += (data.usButtonData < 0) ? "-" : "";
//						if (raton.IndexOf(txt) == -1)
//						{
//							raton.Add(txt);
//						}
//						if ((data.usButtonData > 0) || (data.usButtonData < 0))
//						{
//							borradoRueda.Start();
//						}
						
//					}
//					boton += botones[i];
//				}
//			}

//			txtRaton.Text = "";
//			foreach (string sr in raton)
//			{
//				txtRaton.Text += " " + sr;
//			}
//			if (txtTeclado.Text.StartsWith(" "))
//			{
//				txtTeclado.Text = txtTeclado.Text.Remove(0, 1);
//			}
//			txtRaton.Text = $"X: {data.lLastX} Y: {data.lLastY}" + txtRaton.Text;
//		}

//		private void BorradoRueda_Tick(object sender, EventArgs e)
//		{
//			int i = raton.Count - 1;
//			while(i >= 0)
//			{
//				if (raton[i].StartsWith("RuedaV") || raton[i].StartsWith("RuedaH"))
//				{
//					txtRaton.Text = txtRaton.Text.Replace(raton[i], "");
//					raton.RemoveAt(i);
					
//				}
//				i--;
//			}
//			borradoRueda.Stop();
//		}

//		private void ToggleButton_Checked(object sender, RoutedEventArgs e)
//		{
//			if (this.IsLoaded)
//			{
//				tbJ2.IsChecked = false;
//				tbJ3.IsChecked = false;
//				joySel = 0;
//				Actualizar();
//			}
//		}

//		private void ToggleButton_Checked_1(object sender, RoutedEventArgs e)
//		{
//			tbJ1.IsChecked = false;
//			tbJ3.IsChecked = false;
//			joySel = 1;
//			Actualizar();
//		}

//		private void ToggleButton_Checked_2(object sender, RoutedEventArgs e)
//		{
//			tbJ2.IsChecked = false;
//			tbJ1.IsChecked = false;
//			joySel = 2;
//			Actualizar();
//		}
//	}
//}
