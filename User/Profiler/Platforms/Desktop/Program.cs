using Uno.UI.Hosting;

namespace Profiler;
internal class Program
{
    [System.STAThread]
    public static void Main(string[] args)
    {
        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            //.UseX11()
            //.UseLinuxFrameBuffer()
            //.UseMacOS()
            .UseWin32()
            .Build();

        host.Run();
    }
}
