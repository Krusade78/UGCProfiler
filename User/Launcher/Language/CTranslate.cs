using System.Windows;

namespace Launcher
{
    internal static class CTranslate
    {
        private static ResourceDictionary langDict;

        public static void Load()
        {
            _ = Application.Current;

			if (System.IO.File.Exists($".\\Language\\launcher.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.xaml"))
            {
                using System.IO.FileStream sr = new($".\\Language\\launcher.{System.Threading.Thread.CurrentThread.CurrentUICulture.Parent.Name}.xaml", System.IO.FileMode.Open);
                langDict = (ResourceDictionary)System.Windows.Markup.XamlReader.Load(sr);
            }
            else
            {
                langDict =  new ResourceDictionary()
                {
                    Source = new System.Uri($"pack://application:,,,/launcher;component/Language/en.xaml")
                };
            }
        }

        public static ResourceDictionary GetDictionary() => langDict;

        public static string Get(string key) => (string)langDict[key];


    }
}
