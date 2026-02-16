using FluentUI = Microsoft.UI.Xaml.Controls;
using System;


namespace Profiler
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : FluentUI.Page
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

            ((App)Microsoft.UI.Xaml.Application.Current).SetTitle($"{Translate.Get("save title")} [---]");
            profilePath = "";
            btSave.IsEnabled = true;
        }

        private async void Open(string? filename = null)
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
                Windows.Storage.Pickers.FileOpenPicker fop = new()
                {
                    ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Unspecified
                };
                fop.FileTypeFilter.Add(Translate.Get("profile filter"));

                var file = await fop.PickSingleFileAsync();
                if (file != null)
                {
                    filename = file.Path;
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
                        mainFrame?.Refresh();
                    }

                    profilePath = filename;
                    ((App)Microsoft.UI.Xaml.Application.Current).SetTitle($"{Translate.Get("save title")} [{System.IO.Path.GetFileNameWithoutExtension(filename)}]");
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
            var fsp = new Windows.Storage.Pickers.FileSavePicker()
            {
                SuggestedFileName = Translate.Get("profile_name")
            };
            fsp.FileTypeChoices.Add(Translate.Get("profile"), [Translate.Get("profile filter")]);
            var file = await fsp.PickSaveFileAsync();
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
                    ((App)Microsoft.UI.Xaml.Application.Current).SetTitle($"{Translate.Get("save title")} [{System.IO.Path.GetFileNameWithoutExtension(file.Path)}]");
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
