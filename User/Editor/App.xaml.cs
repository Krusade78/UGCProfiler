using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application
	{
		private readonly bool duplicated = false;
		private MainWindow m_window;

		public static readonly System.Text.Json.JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

		public MainWindow GetMainWindow() => m_window;

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			this.InitializeComponent();
			if (Microsoft.Windows.AppLifecycle.AppInstance.GetInstances().Count > 1)
			{
				duplicated = true;
				App.Current.Exit();
			}
		}

		/// <summary>
		/// Invoked when the application is launched.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs args)
		{			
			if (!duplicated)
			{
				if (System.IO.File.Exists($".\\Language\\profiler.{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.xaml"))
				{
					Current.Resources.MergedDictionaries.RemoveAt(1);
					Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
					{
						Source = new System.Uri($"ms-appx:///Language/profiler.{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.xaml")
					});
				}
				else if (System.IO.File.Exists($".\\Language\\profiler.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.xaml"))
				{
					Current.Resources.MergedDictionaries.RemoveAt(1);
					Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
					{
						Source = new System.Uri($"ms-appx:///Language/profiler.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.xaml")
					});
				}

				m_window = new MainWindow();
				if (m_window.AppWindow is not null)
				{
					m_window.AppWindow.Resize(new() { Width = 1270, Height = 700 });
					Microsoft.UI.Windowing.DisplayArea displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(WinRT.Interop.WindowNative.GetWindowHandle(m_window)), Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);
					if (displayArea is not null)
					{
						m_window.AppWindow.Move(new()
						{
							X = (displayArea.WorkArea.Width - m_window.AppWindow.Size.Width) / 2,
							Y = ((displayArea.WorkArea.Height - m_window.AppWindow.Size.Height) / 2)
						});
                    }
				}
				//Icon   res/Editor.ico
				//this.AppWindow.SetIcon
				m_window.Activate();
			}
		}
    }
}

