using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Profiler.Controls.Properties
{
    /// <summary>
    /// Lógica de interacción para CtlEdicionElemento.xaml
    /// </summary>
    internal sealed partial class CtlProperties : UserControl
    {
        public CtlProperties()
        {
            Events = false;
            InitializeComponent();
            Events = true;
            spModes.Translation += new System.Numerics.Vector3(0, 0, 48);
            bd2.Translation += new System.Numerics.Vector3(0, 0, 32);
        }

        private void FcbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            parent = ((App)Application.Current).GetMainWindow();
        }

        private void SpConfs_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bd2.Visibility = spConfs.Children.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
