using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calibrator
{
	/// <summary>
	/// Lógica de interacción para HIDCal.xaml
	/// </summary>
	internal sealed partial class HIDCal : Page
	{
		private class BufferTuple
		{
			public ushort Raw { get; set; }
			public ushort Calibrated { get; set; }
		}
        private class AxisTempData
        {
            public byte Id { get; set; }
            public ushort Minimun { get; set; } = ushort.MaxValue;
            public ushort Maximun { get; set; }
            public ushort Center { get; set; } = ushort.MaxValue / 2;
            public ushort CenterAlt { get; set; } = ushort.MaxValue / 2;
            public byte CenterAltRepeats { get; set; }
        }

        //private uint joySel = 0;
        //private byte axisSel = 0;
        private readonly Dictionary<uint, Dictionary<byte, Shared.CTypes.STJITTER>> jitters = [];
		private readonly Dictionary<uint, Dictionary<byte, Shared.CTypes.STLIMITS>> limits = [];

		private Profiler.Devices.DeviceInfo.HID_INPUT_DATA lastHidData = new();
		private System.Collections.ObjectModel.ObservableCollection<System.Collections.ObjectModel.ObservableCollection<BufferTuple>> Tuples { get; set; } = [[]];

		//private readonly ushort[] hidReport = new ushort[8];

		//private readonly Dictionary<uint, DatosJoy> dispositivos = new();
		private readonly Profiler.Devices.DeviceInfo devInfo;

		public HIDCal(Profiler.Devices.DeviceInfo deviceInfo)
		{
			InitializeComponent();
			bd1.Translation += new System.Numerics.Vector3(0, 0, 16);
			bd2.Translation += new System.Numerics.Vector3(0, 0, 16);
			bd3.Translation += new System.Numerics.Vector3(0, 0, 16);
			bd4.Translation += new System.Numerics.Vector3(0, 0, 16);
			bd4.Translation += new System.Numerics.Vector3(0, 0, 16);
			bd6.Translation += new System.Numerics.Vector3(0, 0, 48);

			devInfo = deviceInfo;
			ReadCalibration();
			InsertAxes();
		}

		private async void ReadCalibration()
		{
			if (!System.IO.File.Exists("calibration.dat"))
			{
				return;
			}
			try
			{
				Shared.Calibration.CCalibration cal = System.Text.Json.JsonSerializer.Deserialize<Shared.Calibration.CCalibration>(System.IO.File.ReadAllText("calibration.dat"));
				foreach (Shared.Calibration.Limits r in cal.Limits)
				{
					Shared.CTypes.STLIMITS l = new()
					{
						Center = r.Center,
						Left = r.Left,
						Right = r.Right,
						Null = r.Null,
						Cal = r.Cal,
						Range = r.Range,
					};
					if (!limits.TryGetValue(r.IdJoy, out Dictionary<byte, Shared.CTypes.STLIMITS> limitJoy))
					{
						limits.Add(r.IdJoy, new() { { r.IdAxis, l } });
					}
					else
					{
						limitJoy.Add(r.IdAxis, l);
					}
				}
				foreach (Shared.Calibration.Jitter r in cal.Jitters)
				{
					Shared.CTypes.STJITTER j = new()
					{
						Margin = r.Margin,
						Strength = r.Strength,
						Antiv = r.Antiv
					};
					if (!jitters.TryGetValue(r.IdJoy, out Dictionary<byte, Shared.CTypes.STJITTER> jitterJoy))
					{
						jitters.Add(r.IdJoy, new() { { r.IdAxis, j } });
					}
					else
					{
						jitterJoy.Add(r.IdAxis, j);
					}
				}
			}
			catch (System.IO.FileNotFoundException) { }
			catch (Exception ex)
			{
				await Profiler.MessageBox.Show(ex.Message, "Error", Profiler.MessageBoxButton.OK, Profiler.MessageBoxImage.Warning);
			}
		}

		private void InsertAxes()
		{
			byte x = 1, y = 1, z = 1, rx = 1, ry = 1, rz = 1, sl = 1;
			SortedList<byte, SelectorBarItem> navs = [];
			foreach (Profiler.Devices.DeviceInfo.CUsage u in devInfo.Usages)
			{
				switch (u.Type)
				{
					case 0:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"X {x++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					case 1:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Y {y++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					case 2:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Z {z++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					case 3:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Rx {rx++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					case 4:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Ry {ry++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					case 5:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Rz {rz++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					case 6:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Sl {sl++}", Tag = new AxisTempData() { Id = u.Id.Value } });
						break;
					default:
						break;
				}
			}
			foreach (KeyValuePair<byte, SelectorBarItem> nav in navs)
			{
				lsAxes.Items.Add(nav.Value);
			}
			if (lsAxes.Items.Count > 0)
			{
				lsAxes.SelectedItem = lsAxes.Items[0];
			}
		}

		private byte GetNavIdx() => ((AxisTempData)lsAxes.SelectedItem.Tag).Id;

		public void UpdateStatus(byte[] rawData, uint joy)
		{
			if ((joy != devInfo.Id) || (lsAxes.SelectedItem == null))
			{
				return;
			}

			Profiler.Devices.DeviceInfo.HID_INPUT_DATA hidData = new();
			if (!devInfo.ToHiddata(rawData, ref hidData))
			{
				return;
			}

			byte axisId = GetNavIdx();

			if (hidData.Axis[axisId] == lastHidData.Axis[axisId])
			{
				lastHidData = hidData;
				return;
			}

			int posr = hidData.Axis[axisId];
			lastHidData = hidData;

			// Filtrado de ejes
			ushort pollAxis = (ushort)posr;

			if (jitters.TryGetValue(joy, out Dictionary<byte, Shared.CTypes.STJITTER> jjit) && jjit.TryGetValue(axisId, out Shared.CTypes.STJITTER v) && (v.Antiv == 1))
			{
				// Antijitter
				if ((pollAxis == v.PosChosen) || (pollAxis < (v.PosChosen - v.Margin)) || (pollAxis > (v.PosChosen + v.Margin)))
				{
					v.PosRepeated = 0;
					v.PosChosen = pollAxis;
					//v.PosPosible = pollAxis;
					//jitter[joySel][ejeSel] = v;
				}
				else
				{
					if (v.PosRepeated < v.Strength)
					{
						v.PosRepeated++;
						pollAxis = v.PosChosen;
					}
					else
					{
						v.PosRepeated = 0;
						v.PosChosen = pollAxis;
					}
					//if (pollAxis == v.PosPosible)
					//{
					//	v.PosRepeated++;
					//	if (v.PosRepeated == v.Strength)
					//	{
					//		v.PosRepeated = 0;
					//		v.PosChosen = pollAxis;
					//		jitters[joySel][axisSel] = v;
					//	}
					//}
					//else
					//{
					//	v.PosRepeated = 0;
					//	v.PosPosible = pollAxis;
					//	jitters[joySel][axisSel] = v;
					//}
					//pollAxis = v.PosChosen;
				}
			}

			if (limits.TryGetValue(joy, out Dictionary<byte, Shared.CTypes.STLIMITS> jlim) && jlim.TryGetValue(axisId, out Shared.CTypes.STLIMITS limit) && (limit.Cal == 1))
			{
				// Calibration
				ushort width1, width2;
				width1 = (ushort)((limit.Center - limit.Null) - limit.Left);
				width2 = (ushort)(limit.Right - (limit.Center + limit.Null));
				if ((pollAxis >= (limit.Center - limit.Null)) && (pollAxis <= (limit.Center + limit.Null)))
				{
					//Zona nula
					pollAxis = limit.Center;
				}
				else
				{
					if (pollAxis < limit.Left)
						pollAxis = limit.Left;
					if (pollAxis > limit.Right)
						pollAxis = limit.Right;

					if (pollAxis < limit.Center)
					{
						if (width1 != limit.Center)
						{
							if (pollAxis >= width1) { pollAxis = width1; }
							pollAxis -= limit.Left;
							pollAxis = (ushort)((pollAxis * limit.Center) / width1);
						}
					}
					else
					{
						if (width2 != (limit.Right - limit.Center))
						{
							if (pollAxis >= limit.Right) { pollAxis = limit.Right; }
							pollAxis -= (ushort)(limit.Center + limit.Null);
							pollAxis = (ushort)(limit.Center + ((pollAxis * (limit.Right - limit.Center)) / width2));
						}
					}
				}
			}

			txtPosRaw.Text = posr.ToString();
			posRaw.Margin = new Thickness(posr, 0, 0, 0);
			txtPosCal.Text = pollAxis.ToString();
			posCal.Margin = new Thickness(pollAxis, 20, 0, 0);
			BufferTuple lvi = new() { Calibrated = hidData.Axis[axisId], Raw = pollAxis };
			if (Tuples[0].Count > 200)
			{
				Tuples[0].RemoveAt(0);
			}
			Tuples[0].Add(lvi);
			gvBuffer.ScrollIntoView(lvi);
		}

		private void LsAxes_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
		{
			Tuples[0].Clear();
			txtPosCal.Text = "0";
			txtPosRaw.Text = "0";

			if (!limits.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STLIMITS> axesl))
			{
				axesl = [];
				limits.Add(devInfo.Id, axesl);
			}
			if (!axesl.TryGetValue(GetNavIdx(), out Shared.CTypes.STLIMITS limit))
			{
				limit = new();
			}

			if (!jitters.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STJITTER> axesj))
			{
				axesj = [];
				jitters.Add(devInfo.Id, axesj);
			}
			if (!axesj.TryGetValue(GetNavIdx(), out Shared.CTypes.STJITTER jitter))
			{
				jitter = new();
			}

			tsCenter.IsOn = true;
			txtRawMin.Text = limit.Left.ToString();
			txtRawMax.Text = limit.Right.ToString();
			txtRawC.Text = limit.Center.ToString();
			txtCalcRawC.Text = limit.Center.ToString();

			chkCalActiva.IsOn = limit.Cal == 1;
			txtI.Text = limit.Left.ToString();
			txtN.Text = limit.Null.ToString();
			txtD.Text = limit.Right.ToString();

			chkAntivActiva.IsOn = jitter.Antiv == 1;
			txtMargen.Text = jitter.Margin.ToString();
			txtResistencia.Text = jitter.Strength.ToString();

			grTest.Width = devInfo.Usages.First(x => (x.Type < 128) && (x.Id == GetNavIdx())).Range + 1;
			posRaw.Width = posCal.Width = (grTest.Width * 100) / 32768;
			if (posRaw.Width < 1 ) { posRaw.Width = posCal.Width = 1; }
			grTest.Width += posRaw.Width;
		}

		private void Aplicar()
		{
			if (!limits.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STLIMITS> limitJoy))
			{
				limits.Add(devInfo.Id, []);
			}
			if (!limitJoy.TryGetValue(GetNavIdx(), out Shared.CTypes.STLIMITS limit))
			{
				limit = new();
				limitJoy.Add(GetNavIdx(), limit);
			}
			limit.Cal = chkCalActiva.IsOn ? (byte)1 : (byte)0;
			_ = ushort.TryParse(txtI.Text, out ushort s);
			limit.Left = s;
			_ = ushort.TryParse(txtRawC.Text, out s);
			limit.Center = s;
			_ = ushort.TryParse(txtD.Text, out s);
			limit.Right = s;
			_ = byte.TryParse(txtN.Text, out byte b);
			limit.Null = b;
			limitJoy[GetNavIdx()] = limit;

			if (!jitters.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STJITTER> jitterJoy))
			{
				jitters.Add(devInfo.Id, []);
			}
			if (!jitterJoy.TryGetValue(GetNavIdx(), out Shared.CTypes.STJITTER jitter))
			{
				jitter = new();
				jitterJoy.Add(GetNavIdx(), jitter);
			}
			jitter.Antiv = chkAntivActiva.IsOn ? (byte)1 : (byte)0;
			_ = byte.TryParse(txtMargen.Text, out b);
			jitter.Margin = b;
			_ = byte.TryParse(txtResistencia.Text, out b);
			jitter.Strength = b;
			jitterJoy[GetNavIdx()] = jitter;
		}

		private void FbtGuardar_Click(object sender, RoutedEventArgs e)
		{
			//	FbtAplicar_Click(null, null);
			//	{
			//		Shared.Calibration.CCalibration dsc = new();

			//		foreach (var v in limits)
			//		{                    
			//			foreach (var l in v.Value)
			//			{
			//				dsc.Limits.Add(new() { IdJoy = v.Key, IdAxis = l.Key, Cal =l.Value.Cal, Null = l.Value.Null, Left = l.Value.Left, Center = l.Value.Center, Right = l.Value.Right, Range = l.Value.Range });
			//			}
			//		}
			//		foreach (var v in jitters)
			//		{
			//			foreach (var j in v.Value)
			//			{
			//				dsc.Jitters.Add(new() { IdJoy = v.Key, IdAxis = j.Key, Antiv = j.Value.Antiv, Margin = j.Value.Margin, Strength = j.Value.Strength });
			//			}
			//		}

			//		try
			//		{
			//			System.IO.File.WriteAllText("configuracion.dat", System.Text.Json.JsonSerializer.Serialize(dsc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
			//		}
			//		catch (Exception ex)
			//		{
			//			MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
			//			return;
			//		}
			//	}

			//	using System.IO.Pipes.NamedPipeClientStream pipeClient = new("LauncherPipe");
			//	try
			//	{
			//		pipeClient.Connect(1000);
			//		using System.IO.StreamWriter sw = new(pipeClient);
			//		sw.WriteLine("CCAL");
			//		sw.Flush();
			//	}
			//	catch (Exception ex)
			//	{
			//		MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			//		return;
			//	}
		}

		private void Fchk_Changed(object sender, RoutedEventArgs e)
		{
			if (this.IsLoaded)
			{
				Aplicar();
			}
		}

		private void Ftxt_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (this.IsLoaded)
			{
				Aplicar();
			}
		}
	}
}
