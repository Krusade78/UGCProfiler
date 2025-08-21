using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.UI;

namespace Profiler;
public partial class App : Application
{
    public static readonly System.Text.Json.JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    private Window? mainWindow;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (System.IO.File.Exists($".\\Language\\profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.json"))
        {
            ResourceDictionary rd = [];
            string json = System.Text.RegularExpressions.Regex.Replace(
                System.IO.File.ReadAllText($".\\Language\\profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.json"),
                @"^\s*//.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            var dict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(json);
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    rd.Add(kvp.Key, kvp.Value);
                }
            }
            Current.Resources.MergedDictionaries.RemoveAt(1);
            Current.Resources.MergedDictionaries.Add(rd);
        }
        else if (System.IO.File.Exists($".\\Language\\profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.json"))
        {
            ResourceDictionary rd = [];
            string json = System.Text.RegularExpressions.Regex.Replace(
                System.IO.File.ReadAllText($".\\Language\\profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.json"),
                @"^\s*//.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            var dict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(json);
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    rd.Add(kvp.Key, kvp.Value);
                }
            }
            Current.Resources.MergedDictionaries.RemoveAt(1);
            Current.Resources.MergedDictionaries.Add(rd);
        }

         mainWindow = new();
#if DEBUG
        mainWindow.UseStudio();
#endif

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (mainWindow.Content is not Frame rootFrame)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            // Place the frame in the current Window
            mainWindow.Content = rootFrame;

            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            // When the navigation stack isn't restored navigate to the first page,
            // configuring the new page by passing required information as a navigation
            // parameter
            rootFrame.Navigate(typeof(MainPage), args.Arguments);
        }

        //MainWindow.SetWindowIcon();
        // Ensure the current window is active
        mainWindow.Activate();
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
    {
        throw new System.InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    #region Window
    public void SetTitle(string title)
    {
        if (mainWindow != null)
        {
            mainWindow.Title = title;
        }
    }

    public XamlRoot? GetRoot()
    {
        return mainWindow?.Content?.XamlRoot;
    }

    public void Close()
    {
        mainWindow?.Close();
    }

    public void SetClosing(Windows.Foundation.TypedEventHandler<Microsoft.UI.Windowing.AppWindow, Microsoft.UI.Windowing.AppWindowClosingEventArgs> closingHandler)
    {
        if (mainWindow != null)
        {
            mainWindow.AppWindow.Closing += closingHandler;
        }
    }

    public MainPage GetMainPage()
    {
        return (MainPage?)mainWindow?.Content ?? new();
    }
    #endregion
}
