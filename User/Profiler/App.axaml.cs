using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Profiler
{
    public partial class App : Application
    {
        public static readonly System.Text.Json.JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (System.IO.File.Exists($".\\Language\\profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.axaml"))
            {
                Current.Resources.Clear();
                Avalonia.Markup.Xaml.Styling.MergeResourceInclude res = new(new System.Uri($"avares://Profiler")) { Source = new System.Uri($"avares://Profiler/Language/profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Name}.axaml") };
                foreach (var item in res.Loaded)
                {
                    Current.Resources.Add(item);
                }
            }
            else if (System.IO.File.Exists($".\\Language\\profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.axaml"))
            {
                Current.Resources.Clear();
                Avalonia.Markup.Xaml.Styling.MergeResourceInclude res = new(new System.Uri("avares://Profiler")) { Source = new System.Uri($"avares://Profiler/Language/profiler-{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.axaml") };
                foreach (var item in res.Loaded)
                {
                    Current.Resources.Add(item);
                }
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public MainWindow GetMainWindow()
        {
            return (MainWindow)((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).MainWindow;
        }
    }
}