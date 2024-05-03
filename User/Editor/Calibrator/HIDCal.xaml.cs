using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calibrator
{
	/// <summary>
	/// HIDCal.xaml
	/// </summary>
	internal sealed partial class HIDCal : Page
	{
		private class BufferTuple
		{
			public ushort Raw { get; set; }
			public ushort Calibrated { get; set; }
		}
		private class AxisTempRawData
		{
			public byte Id { get; set; }
			public ushort Minimun { get; set; } = ushort.MaxValue;
			public ushort Maximun { get; set; }
			public ushort CenterManual { get; set; }
			public ushort CenterAuto { get; set; }
			public ushort CenterAlt { get; set; }
			public byte CenterAltRepeats { get; set; }
			public bool CenterManualOn { get; set; }
		}

		private readonly Dictionary<uint, Dictionary<byte, Shared.CTypes.STJITTER>> jitters = [];
		private readonly Dictionary<uint, Dictionary<byte, Shared.CTypes.STLIMITS>> limitsCal = [];
		private System.Collections.ObjectModel.ObservableCollection<System.Collections.ObjectModel.ObservableCollection<BufferTuple>> Tuples { get; set; } = [[]];
		private readonly Profiler.Devices.DeviceInfo devInfo;
		private readonly DispatcherTimer refresh = new();
		private BufferTuple toRefresh;
		private int lastRaw;


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

			refresh.Interval = new TimeSpan(3000000);
			refresh.Tick += (sender, e) => { refresh.Stop(); if (toRefresh != null) { gvBuffer.ScrollIntoView(toRefresh); } };
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
						Range = r.Range,
					};
					if (!limitsCal.TryGetValue(r.IdJoy, out Dictionary<byte, Shared.CTypes.STLIMITS> limitJoy))
					{
						limitsCal.Add(r.IdJoy, new() { { r.IdAxis, l } });
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
				await Profiler.MessageBox.Show(ex.Message, Profiler.Translate.Get("error"), Profiler.MessageBoxButton.OK, Profiler.MessageBoxImage.Warning);
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
						navs.Add(u.Type, new SelectorBarItem() { Text = $"X {x++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
						break;
					case 1:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Y {y++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
						break;
					case 2:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Z {z++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
						break;
					case 3:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Rx {rx++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
						break;
					case 4:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Ry {ry++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
						break;
					case 5:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Rz {rz++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
						break;
					case 6:
						navs.Add(u.Type, new SelectorBarItem() { Text = $"Sl {sl++}", Tag = new AxisTempRawData() { Id = u.Id, CenterAuto = (ushort)(u.Range / 2), CenterAlt = (ushort)(u.Range / 2) } });
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

		private AxisTempRawData GetNavData() => (AxisTempRawData)lsAxes.SelectedItem.Tag;

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

			byte axisId = GetNavData().Id;

			int posr = hidData.Axis[axisId];

			//Axes filters
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

			if (limitsCal.TryGetValue(joy, out Dictionary<byte, Shared.CTypes.STLIMITS> jlim) && jlim.TryGetValue(axisId, out Shared.CTypes.STLIMITS limit))
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
						if ((width1 != limit.Center) && (width1 != 0))
						{
							if (pollAxis >= width1) { pollAxis = width1; }
							pollAxis -= limit.Left;
							pollAxis = (ushort)((pollAxis * limit.Center) / width1);
						}
					}
					else
					{
						if ((width2 != (devInfo.Usages[axisId].Range - limit.Center)) && (width2 != 0))
						{
							if (pollAxis >= limit.Right) { pollAxis = limit.Right; }
							pollAxis -= (ushort)(limit.Center + limit.Null);
							pollAxis = (ushort)(limit.Center + ((pollAxis * (devInfo.Usages[axisId].Range - limit.Center)) / width2));
						}
					}
				}
			}

			DispatcherQueue.TryEnqueue(() => UpdateGUI((ushort)posr, pollAxis, (ushort)(devInfo.Usages[axisId].Range / 2), GetNavData()));
		}

		private void UpdateGUI(ushort raw, ushort calibrated, ushort center, AxisTempRawData temprawData)
		{
			if (raw < temprawData.Minimun)
			{
				temprawData.Minimun = raw;
				txtRawMin.Text = raw.ToString();
			}
			if (raw > temprawData.Maximun)
			{
				temprawData.Maximun = raw;
				txtRawMax.Text = raw.ToString();
			}
			if ((raw != temprawData.CenterAuto) && (raw > (center - 2)) & (raw < (center + 2)))
			{
				if (raw == temprawData.CenterAlt)
				{
					temprawData.CenterAltRepeats++;
				}
				else
				{
					temprawData.CenterAlt = raw;
					temprawData.CenterAltRepeats = 0;
				}
				if (temprawData.CenterAltRepeats == 50)
				{
					temprawData.CenterAuto = temprawData.CenterAlt;
					temprawData.CenterAltRepeats = 0;
					txtCalcRawC.Text = temprawData.CenterAuto.ToString();
				}
			}
			if (lastRaw != raw)
			{
				lastRaw = raw;
				txtPosRaw.Text = raw.ToString();
				posRaw.Margin = new Thickness(raw, 0, 0, 0);
				txtPosCal.Text = calibrated.ToString();
				posCal.Margin = new Thickness(calibrated, 20, 0, 0);
				if (tbPlay.IsChecked == true)
				{
					BufferTuple lvi = new() { Calibrated = calibrated, Raw = raw };
					if (Tuples[0].Count > 400)
					{
						Tuples[0].RemoveAt(0);
					}
					Tuples[0].Add(lvi);

					toRefresh = lvi;
					refresh.Start();
				}
			}
		}

		private void LsAxes_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
		{
			Tuples[0].Clear();
			GetNavData().Maximun = 0;
			GetNavData().Minimun = 65535;
            GetNavData().CenterAuto = 0;

            txtPosCal.Text = "0";
			txtPosRaw.Text = "0";
			txtRawMin.Text = "0";
			txtRawMax.Text = "0";
			txtRawC.Text = GetNavData().CenterManual.ToString();
			txtCalcRawC.Text = "0";
			tsCenter.IsOn = GetNavData().CenterManualOn;

			if (!limitsCal.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STLIMITS> axesl))
			{
				axesl = [];
				limitsCal.Add(devInfo.Id, axesl);
			}
			if (!axesl.TryGetValue(GetNavData().Id, out Shared.CTypes.STLIMITS limit))
			{
				limit = new();
			}

			if (!jitters.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STJITTER> axesj))
			{
				axesj = [];
				jitters.Add(devInfo.Id, axesj);
			}
			if (!axesj.TryGetValue(GetNavData().Id, out Shared.CTypes.STJITTER jitter))
			{
				jitter = new();
			}

			txtL.Text = limit.Left.ToString();
            txtC.Text = limit.Center.ToString();
            txtN.Text = limit.Null.ToString();
			txtR.Text = limit.Right.ToString();

			chkAntivActiva.IsOn = jitter.Antiv == 1;
			txtMargen.Text = jitter.Margin.ToString();
			txtResistencia.Text = jitter.Strength.ToString();

			grTest.Width = devInfo.Usages.First(x => (x.Type < 253) && (x.Id == GetNavData().Id)).Range + 1;
			posRaw.Width = posCal.Width = (grTest.Width * 100) / 32768;
			if (posRaw.Width < 1 ) { posRaw.Width = posCal.Width = 1; }
			grTest.Width += posRaw.Width;
		}

		private void ButtonTakeFromRaw_Click(object sender, RoutedEventArgs e)
		{
			txtL.Text = txtRawMin.Text;
			txtC.Text = tsCenter.IsOn ? txtRawC.Text : txtCalcRawC.Text;
			txtR.Text = txtRawMax.Text;
		}

		private async void FbtSave_Click(object sender, RoutedEventArgs e)
		{
			Apply();
			{
				Shared.Calibration.CCalibration dsc = new();

				foreach (var v in limitsCal)
				{
					foreach (KeyValuePair<byte, Shared.CTypes.STLIMITS> l in v.Value)
					{
						dsc.Limits.Add(new() { IdJoy = v.Key, IdAxis = l.Key, Null = l.Value.Null, Left = l.Value.Left, Center = l.Value.Center, Right = l.Value.Right, Range = l.Value.Range });
					}
				}
				foreach (var v in jitters)
				{
					foreach (KeyValuePair<byte, Shared.CTypes.STJITTER> j in v.Value)
					{
						dsc.Jitters.Add(new() { IdJoy = v.Key, IdAxis = j.Key, Antiv = j.Value.Antiv, Margin = j.Value.Margin, Strength = j.Value.Strength });
					}
				}

				try
				{
					System.IO.File.WriteAllText("calibration.dat", System.Text.Json.JsonSerializer.Serialize(dsc, Profiler.App.jsonOptions));
				}
				catch (Exception ex)
				{
					await Profiler.MessageBox.Show(ex.Message, "Error", Profiler.MessageBoxButton.OK, Profiler.MessageBoxImage.Warning);
					return;
				}
			}

			using System.IO.Pipes.NamedPipeClientStream pipeClient = new("LauncherPipe");
			try
			{
				pipeClient.Connect(1000);
				using System.IO.StreamWriter sw = new(pipeClient);
				sw.WriteLine("CCAL");
				sw.Flush();
			}
			catch (Exception ex)
			{
				await Profiler.MessageBox.Show(ex.Message, "Error", Profiler.MessageBoxButton.OK, Profiler.MessageBoxImage.Error);
				return;
			}
		}

		private void Fchk_Changed(object sender, RoutedEventArgs e)
		{
			if (this.IsLoaded)
			{
				Apply();
			}
		}

		private void Ftxt_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (this.IsLoaded)
			{
				Apply();
			}
		}

		private void Apply()
		{
			if (!limitsCal.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STLIMITS> limitJoy))
			{
				limitsCal.Add(devInfo.Id, []);
			}
			if (!limitJoy.TryGetValue(GetNavData().Id, out Shared.CTypes.STLIMITS limit))
			{
				limit = new();
				limitJoy.Add(GetNavData().Id, limit);
			}

			_ = ushort.TryParse(txtL.Text, out ushort s);
			limit.Left = s;
			_ = ushort.TryParse(txtR.Text, out s);
			limit.Right = s;
			_ = ushort.TryParse(txtC.Text, out s);
			limit.Center = s;
			limit.Range = devInfo.Usages[GetNavData().Id].Range;

			_ = byte.TryParse(txtN.Text, out byte b);
			limit.Null = b;
			limitJoy[GetNavData().Id] = limit;

			_ = ushort.TryParse(txtRawC.Text, out s);
			GetNavData().CenterManual = s;
			GetNavData().CenterManualOn = tsCenter.IsOn;

			if (!jitters.TryGetValue(devInfo.Id, out Dictionary<byte, Shared.CTypes.STJITTER> jitterJoy))
			{
				jitters.Add(devInfo.Id, []);
			}
			if (!jitterJoy.TryGetValue(GetNavData().Id, out Shared.CTypes.STJITTER jitter))
			{
				jitter = new();
				jitterJoy.Add(GetNavData().Id, jitter);
			}
			jitter.Antiv = chkAntivActiva.IsOn ? (byte)1 : (byte)0;
			_ = byte.TryParse(txtMargen.Text, out b);
			jitter.Margin = b;
			_ = byte.TryParse(txtResistencia.Text, out b);
			jitter.Strength = b;
			jitterJoy[GetNavData().Id] = jitter;
		}

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
			Tuples[0].Clear();
        }
    }
}
