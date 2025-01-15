using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Profiler
{
    public partial class MainWindow : Window
    {
        private bool exit;
        private readonly Pages.Main mainFrame = new();
        public enum Section : byte
        {
            None,
            Calibrate,
            Macros,
            Edit,
            View,
        }

        private Section currentSection = Section.None;
        private bool forceToggle = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = (string)Avalonia.Application.Current.Resources["profiler"];
            ctlDevs.navView.SelectionChanged += (object s, FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs e) => { tbCalibrate.IsEnabled = ctlDevs.navView.SelectedItem != null; };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (Design.IsDesignMode) { return; }
#endif
            if (System.Environment.GetCommandLineArgs().Length == 2)
                Open(System.Environment.GetCommandLineArgs()[1]);

            await CLauncherPipe.SetRawMode(true);

            if (!ctlDevs.Prepare())
            {
                this.Close();
            }
            else
            {
                ctlDevs.SetMainFrame(mainFrame);
            }
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            if (!exit)
            {
                e.Cancel = true;
                Avalonia.Threading.Dispatcher.UIThread.Post(() => { Exit(); });
            }
        }

        private async void Exit()
        {
            if (data.Modified)
            {
                FluentAvalonia.UI.Controls.ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("save"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == FluentAvalonia.UI.Controls.ContentDialogResult.None)
                {
                    return;
                }
                else if (r == FluentAvalonia.UI.Controls.ContentDialogResult.Primary)
                {
                    if (!await Save())
                    {
                        return;
                    }
                }
            }
            await CLauncherPipe.SetRawMode(false, true);
            await CLauncherPipe.SetCalibrationMode(false, true);
            ctlDevs.Dispose();
            exit = true;
            Close();
        }

        #region "File"
        private void RibbonButtonNew_Click(object sender, RoutedEventArgs e)
        {
            New();
        }
        private void RibbonButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }
        private async void RibbonButtonSave_Click(object sender, RoutedEventArgs e)
        {
            await Save();
        }
        private async void RibbonButtonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            await SaveAs();
        }
        #endregion

        #region "Profile"
        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            Launch();
        }

        private async void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            await CLauncherPipe.Reset();
        }
        #endregion

        #region Devices
        private async void ButtonMouseConf_Click(object sender, RoutedEventArgs e)
        {
            await Dialogs.MouseConfig.Show(this);
        }

        private void TbCalibrate_Checked(object sender, RoutedEventArgs e)
        {
            forceToggle = true;
            tbMacroEdit.IsChecked = false;
            tbList.IsChecked = false;
            tbEdit.IsChecked = false;
            forceToggle = false;
            ctlDevs.SetMacroHeader(false);
            currentSection = tbCalibrate.IsChecked == true ? Section.Calibrate : Section.None;
            mainFrame.GoToSection(currentSection);

        }
        #endregion

        #region "View"
        private void FtbMacroEdit_Checked(object sender, RoutedEventArgs e)
        {
            forceToggle = true;
            tbCalibrate.IsChecked = false;
            tbList.IsChecked = false;
            tbEdit.IsChecked = false;
            forceToggle = false;
            ctlDevs.SetMacroHeader(true);
            currentSection = Section.Macros;
            mainFrame.GoToSection(Section.Macros);
        }

        private void FtbEdit_Checked(object sender, RoutedEventArgs e)
        {
            forceToggle = true;
            tbCalibrate.IsChecked = false;
            tbMacroEdit.IsChecked = false;
            tbList.IsChecked = false;
            forceToggle = false;
            ctlDevs.SetMacroHeader(false);
            currentSection = Section.Edit;
            mainFrame.GoToSection(Section.Edit);
        }
        private void FtbList_Checked(object sender, RoutedEventArgs e)
        {
            forceToggle = true;
            tbCalibrate.IsChecked = false;
            tbMacroEdit.IsChecked = false;
            tbEdit.IsChecked = false;
            forceToggle = false;
            ctlDevs.SetMacroHeader(false);
            currentSection = Section.View;
            mainFrame.GoToSection(Section.View);
        }

        private void TbUnchecked(object sender, RoutedEventArgs e)
        {
            if (!forceToggle)
            {
                ctlDevs.SetMacroHeader(false);
                currentSection = Section.None;
                mainFrame.GoToSection(currentSection);
            }
        }
        #endregion
    }
}