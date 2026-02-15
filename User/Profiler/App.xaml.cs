using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static readonly System.Text.Json.JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
        private Window? mainWindow;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
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

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (mainWindow.Content is not MainPage)
            {
                mainWindow.Content = new MainPage();
                ((MainPage)mainWindow.Content).Loaded += (s, e) =>
                {
                    System.Drawing.Icon? ico = System.Drawing.Icon.ExtractAssociatedIcon(System.Environment.ProcessPath ?? "");
                    if (ico != null)
                    {
                        Microsoft.UI.WindowId wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(WinRT.Interop.WindowNative.GetWindowHandle(mainWindow));
                        Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId).SetIcon(Microsoft.UI.Win32Interop.GetIconIdFromIcon(ico.Handle));
                    }
                    InitializeMinSize(WinRT.Interop.WindowNative.GetWindowHandle(mainWindow));
                };
            }

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

        #region Win32 MinSize
        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProcDelegate pfnSubclass, uint uIdSubclass, uint dwRefData);
        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x, y; }
        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO { public POINT ptReserved; public POINT ptMaxSize; public POINT ptMaxPosition; public POINT ptMinTrackSize; public POINT ptMaxTrackSize; }

        private delegate IntPtr SubclassProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

        private static void InitializeMinSize(IntPtr hwnd)
        {
            SetWindowSubclass(hwnd, SubclassProc, 0, 0);
        }

        private static IntPtr SubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
        {
            if (msg == 0x24)
            {
                var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                mmi.ptMinTrackSize.x = 1280;
                mmi.ptMinTrackSize.y = 720;
                Marshal.StructureToPtr(mmi, lParam, true);
            }
            return DefSubclassProc(hWnd, msg, wParam, lParam);
        }
        #endregion
    }
}
