using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Profiler
{
	public sealed partial class MainWindow : Window
	{
		private bool exit;
		private readonly Pages.Main mainFrame = new();
		public Grid Root { get => root; }
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
			this.AppWindow.Title = (string)Application.Current.Resources["profiler"];
			this.AppWindow.Closing += Window_Closing;
			root.Loaded += Window_Loaded;
			ctlDevs.SelectionChanged += (NavigationView, NavigationViewSelectionChangedEventArgs)  => { tbCalibrate.IsEnabled = ctlDevs.SelectedItem != null; };
			grMenu.Translation += new System.Numerics.Vector3(0, 0, 64);
        }

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
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

		private void Window_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
		{
			if (!exit)
			{ 
				args.Cancel = true;
				this.DispatcherQueue.TryEnqueue(() => { Exit(); });
			}
		}

		private async void Exit()
		{
			if (data.Modified)
			{
				ContentDialogResult r = await MessageBox.Show(Translate.Get("do you want to save changes?"), Translate.Get("save"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (r == ContentDialogResult.None)
				{
					return;
				}
				else if (r == ContentDialogResult.Primary)
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
