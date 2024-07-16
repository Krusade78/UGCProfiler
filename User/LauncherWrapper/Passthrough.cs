namespace LauncherWrapper
{
    //prevents lots of false warnings about WPF
    public class Passthrough
    {
        private readonly Launcher.CMain main = new();

        public void Init()
        {
            main.Init();
        }

        public void LoadDefault()
        {
            main.LoadDefault();
        }
    }
}
