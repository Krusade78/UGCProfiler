using Microsoft.UI.Xaml;

namespace Profiler
{
    internal static class Translate
    {
        public static string Get(string st) => (string)Application.Current.Resources[st];
    }
}
