using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.UI;

namespace Profiler;
public partial class App : Application
{
    #region Win32
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern nint FindWindow(string? lpClassName, string lpWindowName);


    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern nint LoadIconW(nint hInst, nint name);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetModuleHandle(string? lpModuleName);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern nint SendMessage(nint hWnd, int Msg, int wParam, nint lParam);
    #endregion

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
        if (mainWindow.Content is not MainPage)
        {
            mainWindow.Content = new MainPage();
            ((MainPage)mainWindow.Content).Loaded += (s, e) =>
            {
                nint hInstance = GetModuleHandle(null);
                nint hIcon = LoadIconW(hInstance, new nint(101));
                var hwnd = FindWindow(null, mainWindow.Title);

                var b1 = SendMessage(hwnd, 0x80, 0, hIcon);
                var b2 =SendMessage(hwnd, 0x80, 1, hIcon);
            };
        }

        // Ensure the current window is active
        mainWindow.Activate();
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
