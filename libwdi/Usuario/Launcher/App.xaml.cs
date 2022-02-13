namespace Launcher
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);
            CMain.Iniciar();
        }
    }
}
