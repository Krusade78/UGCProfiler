using System;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentUI = FluentAvalonia.UI.Controls;

namespace Profiler
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainWindow : Window
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
                FluentUI.ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == FluentUI.ContentDialogResult.None)
                    return;
                else if (r == FluentUI.ContentDialogResult.Primary)
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
                    FluentUI.ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (r == FluentUI.ContentDialogResult.None)
                        return;
                    else if (r == FluentUI.ContentDialogResult.Primary)
                    {
                        if (!await Save())
                            return;
                    }
                }
                var file = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    FileTypeFilter = [new FilePickerFileType(Translate.Get("profile")) { Patterns = [$"*{Translate.Get("profile filter")}"] }],
                    AllowMultiple = false
                });
                if (file.Count == 1)
                {
                    filename = file[0].TryGetLocalPath();
                }
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
            var file = await StorageProvider.SaveFilePickerAsync(new()
            {
                FileTypeChoices = [new FilePickerFileType(Translate.Get("profile")) { Patterns = [$"*{Translate.Get("profile filter")}"] }],
                SuggestedFileName = Translate.Get("profile_name"),
                
            });
            if (file != null && file.TryGetLocalPath() != null)
            {
                System.Collections.Generic.List<Shared.ProfileModel.DeviceInfo> devs = [];
                foreach (System.Collections.Generic.KeyValuePair<string, Devices.DeviceInfo> kv in ctlDevs.GetDevices())
                {
                    devs.Add(kv.Value);
                }
                if (await data.Save(file.TryGetLocalPath(), devs))
                {
                    profilePath = file.TryGetLocalPath();
                    this.Title = $"{Translate.Get("save title")} [{System.IO.Path.GetFileNameWithoutExtension(file.Name)}]";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private async void Launch()
        {
            if (data.Modified)
            {
                FluentUI.ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("warning"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == FluentUI.ContentDialogResult.None)
                    return;
                else if (r == FluentUI.ContentDialogResult.Primary)
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
