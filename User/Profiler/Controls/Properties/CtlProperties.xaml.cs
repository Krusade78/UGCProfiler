using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Profiler.Controls.Properties
{
    internal sealed partial class CtlProperties : UserControl
    {
        public CtlProperties()
        {
            CurrentDevInfo = new();
            parent = ((App)Application.Current).GetMainPage();
            Events = false;
            InitializeComponent();
            Events = true;
        }

        private void FcbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
//#if DEBUG
//            if (Design.IsDesignMode) { return; }
//#endif
        }

        private void SpConfs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bd2.Visibility = spConfs.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AssignDefaultvJoy();
        }
    }
}
