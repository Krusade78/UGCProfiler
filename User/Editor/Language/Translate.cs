namespace Profiler
{
    internal static class Translate
    {
        public static string Get(string st) => (string)Microsoft.UI.Xaml.Application.Current.Resources[st];
    }
}
