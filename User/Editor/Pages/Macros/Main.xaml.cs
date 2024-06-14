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
            ctlKeyboard.DataContext = CurrentMacro;
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
            ctlKeyboard.GoToBasic();
            ctlStatusCommands.GoToBasic();
            ctlModes.GoToBasic();
            ctlMouse.GoToBasic();
            ctlDirectX.GoToBasic();
            ctlVKBGladiatorNXT.GoToBasic();
            ctlSaitekX52.GoToBasic();

            ButtonSubir.IsEnabled = false;
            ButtonBajar.IsEnabled = false;
        }

        public void GoToAdvanced()
        {
            ctlKeyboard.GoToAdvanced();
            ctlStatusCommands.GoToAdvanced();
            ctlModes.GoToAdvanced();
            ctlMouse.GoToAdvanced();
            ctlDirectX.GoToAdvanced();
            ctlVKBGladiatorNXT.GoToAdvanced();
            ctlSaitekX52.GoToAdvanced();

            ButtonSubir.IsEnabled = true;
            ButtonBajar.IsEnabled = true;
        }
    }
}
