﻿using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private readonly CDatos data = new();
		private string profilePath = "";

		internal CDatos GetData()
		{
			return data;
		}

		private async void New()
		{
			if (data.Modified)
			{
				ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (r == ContentDialogResult.None)
					return;
				else if (r == ContentDialogResult.Primary)
				{
					if (!await Save())
						return;
				}
			}

			data.New();

			if (currentSection != Section.Calibrate)
			{
				tbList.IsChecked = false;
				tbEdit.IsChecked = true;
			}

			this.Title = $"{Translate.Get("save title")} [---]";
			profilePath = "";
			btSave.IsEnabled = true;
		}

		private async void Open(string filename = null)
		{
			if (filename == null)
			{
				if (data.Modified)
				{
					ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
					if (r == ContentDialogResult.None)
						return;
					else if (r == ContentDialogResult.Primary)
					{
						if (!await Save())
							return;
					}
				}
				Windows.Storage.Pickers.FileOpenPicker dlg = new();
				WinRT.Interop.InitializeWithWindow.Initialize(dlg, WinRT.Interop.WindowNative.GetWindowHandle(this));

				dlg.FileTypeFilter.Add(Translate.Get("profile filter"));
				Windows.Storage.StorageFile file = await dlg.PickSingleFileAsync();
				if (file != null)
					filename = file.Path;
			}
			if (filename != null)
			{
				if (await data.Load(filename))
				{
					tbEdit.IsChecked = false;
					tbList.IsChecked = false;
					if (currentSection != Section.Calibrate)
					{
						currentSection = Section.None;
						mainFrame.Refresh();
					}

					profilePath = filename;
					this.Title = $"{Translate.Get("save title")} [{System.IO.Path.GetFileNameWithoutExtension(filename)}]";
					btSave.IsEnabled = true;

					foreach (Shared.ProfileModel.DeviceInfo di in data.Profile.DevicesIncluded)
					{
						ctlDevs.AddProfileDevice(di);
					}
				}
			}
		}

		private async System.Threading.Tasks.Task<bool> Save()
		{
			if (profilePath == "")
				return await SaveAs();
			else
			{
				System.Collections.Generic.List<Shared.ProfileModel.DeviceInfo> devs = [];
				foreach (System.Collections.Generic.KeyValuePair<string, Devices.DeviceInfo> kv in ctlDevs.GetDevices())
				{
					devs.Add(kv.Value);
				}
				return await data.Save(profilePath, devs);
			}
		}

		private async System.Threading.Tasks.Task<bool> SaveAs()
		{
			Windows.Storage.Pickers.FileSavePicker dlg = new();
			WinRT.Interop.InitializeWithWindow.Initialize(dlg, WinRT.Interop.WindowNative.GetWindowHandle(this));

			dlg.FileTypeChoices.Add(Translate.Get("profile"), [Translate.Get("profile filter")]);
			dlg.SuggestedFileName = Translate.Get("profile_name");
			Windows.Storage.StorageFile file = await dlg.PickSaveFileAsync();
			if (file != null)
			{
                System.Collections.Generic.List<Shared.ProfileModel.DeviceInfo> devs = [];
                foreach (System.Collections.Generic.KeyValuePair<string, Devices.DeviceInfo> kv in ctlDevs.GetDevices())
                {
                    devs.Add(kv.Value);
                }
                if (await data.Save(file.Path, devs))
				{
					profilePath = file.Path;
					this.Title = $"{Translate.Get("save title")} [{System.IO.Path.GetFileNameWithoutExtension(file.DisplayName)}]";
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		private async void Launch()
		{
			if (data.Modified)
			{
				ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (r == ContentDialogResult.None)
					return;
				else if (r == ContentDialogResult.Primary)
				{
					if (!await Save())
						return;
				}
			}

			if (await CLauncherPipe.LaunchProfileAsync(profilePath))
			{
				await MessageBox.Show(Translate.Get("profile launched ok"), "Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
	}
}
