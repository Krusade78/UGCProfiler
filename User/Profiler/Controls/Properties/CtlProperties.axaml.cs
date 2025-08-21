using Avalonia.Controls;


namespace Profiler.Controls.Properties
{
    internal sealed partial class CtlProperties : UserControl
    {
        public CtlProperties()
        {
            Events = false;
            InitializeComponent();
            Events = true;
        }

        private void FcbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void Grid_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
#if DEBUG
            if (Design.IsDesignMode) { return; }
#endif
            parent = ((App)Avalonia.Application.Current).GetMainWindow();
        }

        private void SpConfs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bd2.IsVisible = spConfs.Children.Count == 0 ? false : true;
        }

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            AssignDefaultvJoy();
        }
    }
}
