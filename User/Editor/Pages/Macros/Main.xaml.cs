using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace Profiler.Pages.Macros
{
    /// <summary>
    /// Lógica de interacción para VEditorMacros.xaml
    /// </summary>
    internal partial class Main : Page, IChangeMode
    {
        private MainWindow parent;
        //private static readonly int ultimaPlantilla = -1;
        //private readonly List<byte> teclas = [];
        internal EditedMacro CurrentMacro { get; private set; } = new();

        public Main()
        {
            InitializeComponent();
            CurrentMacro.SetListBox(ListBox1);
            ctlName.DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            parent = ((App)Application.Current).GetMainWindow();
            lbMacros.DataContext = parent.GetData().Profile.Macros.Where(x => x.Id != 0).OrderBy(x => x.Name);
            GoToBasic();
        }

        private void ButtonBorrar_Click(object sender, RoutedEventArgs e)
        {
            //BorrarMacroLista();
        }

        private void ButtonSubir_Click(object sender, RoutedEventArgs e)
        {
            //SubirMacroLista();
        }

        private void ButtonBajar_Click(object sender, RoutedEventArgs e)
        {
            //BajarMacroLista();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {

            ushort nextId = (ushort)(parent.GetData().Profile.Macros.Max(x => x.Id) + 1);
            parent.GetData().Profile.Macros.Add(new () { Id = nextId, Name = "- New macro -" });
            lbMacros.DataContext = parent.GetData().Profile.Macros.Where(x => x.Id != 0).OrderBy(x => x.Name);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbMacros.SelectedItem != null)
            {
                parent.GetData().Profile.Macros.Remove(parent.GetData().Profile.Macros.Find(x => x.Id == ((Shared.ProfileModel.MacroModel)lbMacros.SelectedItem).Id));
                lbMacros.DataContext = parent.GetData().Profile.Macros.Where(x => x.Id != 0).OrderBy(x => x.Name);
            }
        }

        private void LbMacros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMacros.SelectedItem == null)
            {
                btSave.IsEnabled = false;
                ctlName.IsEnabled = false;
                ctlDirectX.IsEnabled = false;
                ctlKeyboard.IsEnabled = false;
                ctlModes.IsEnabled = false;
                ctlMouse.IsEnabled = false;
                ctlSaitekX52.IsEnabled = false;
                ctlStatusCommands.IsEnabled = false;
                ctlVKBGladiatorNXT.IsEnabled = false;
                ListBox1.Items.Clear();
            }
            else
            {
                btSave.IsEnabled = true;
                ctlName.IsEnabled = true;
                ctlDirectX.IsEnabled = true;
                ctlKeyboard.IsEnabled = true;
                ctlModes.IsEnabled = true;
                ctlMouse.IsEnabled = true;
                ctlSaitekX52.IsEnabled = true;
                ctlStatusCommands.IsEnabled = true;
                ctlVKBGladiatorNXT.IsEnabled = true;
                ctlName.Load(((Shared.ProfileModel.MacroModel)lbMacros.SelectedItem).Id);
                bool MFDText = false;
                CurrentMacro.LoadData(((Shared.ProfileModel.MacroModel)lbMacros.SelectedItem).Id, ref MFDText);
                ctlSaitekX52.Load(MFDText);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            //Guardar();
        }

        public void GoToBasic()
        {
            //PanelNXT.Visibility = Visibility.Visible;
            //PanelX52.Visibility = Visibility.Visible;
            //PanelSetas.Visibility = Visibility.Visible;
            //PanelRatonOff.Visibility = Visibility.Visible;
            //PanelMovimiento.Visibility = Visibility.Visible;
            //PanelModos.Visibility = Visibility.Visible;
            //PanelPlantilla.Visibility = Visibility.Visible;
            ctlKeyboard.GoToBasic();
            ctlStatusCommands.GoToBasic();

            //ButtonDXOff.IsEnabled = false;
            ButtonSubir.IsEnabled = false;
            ButtonBajar.IsEnabled = false;
        }

        public void GoToAdvanced()
        {
            //PanelNXT.Visibility = Visibility.Collapsed;
            //PanelX52.Visibility = Visibility.Collapsed;
            //PanelSetas.Visibility = Visibility.Collapsed;
            //PanelRatonOff.Visibility = Visibility.Collapsed;
            //PanelMovimiento.Visibility = Visibility.Collapsed;
            //PanelModos.Visibility = Visibility.Collapsed;
            //PanelPlantilla.Visibility = Visibility.Collapsed;
            ctlKeyboard.GoToAdvanced();
            ctlStatusCommands.GoToAdvanced();

            //ButtonDXOff.IsEnabled = true;
            ButtonSubir.IsEnabled = true;
            ButtonBajar.IsEnabled = true;
        }
    }
}
