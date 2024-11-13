using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlModes : UserControl, IChangeMode
    {
        public CtlModes()
        {
            this.InitializeComponent();
            Panel2.Translation += new System.Numerics.Vector3(0, 0, 16);
        }


        #region "Modes"
        private void ButtonSetMode_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(uint)CommandType.Mode + (uint)(cbMode.SelectedIndex << 8)]);
        }


        private void ButtonSetSubmode_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(uint)CommandType.SubMode + (uint)(cbSubmode.SelectedIndex << 8)]);
        }


        private void ButtonPrecisionOn_Click(object sender, RoutedEventArgs e)
        {
            //if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            //ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + cbEje.SelectedIndex) | ((NumericUpDownPr.Valor - 1) << 5)) << 8);
            //((EditedMacro)DataContext).Insertar([(ushort)((ushort)CommandType.PrecisionMode + dato)], false);
        }

        private void ButtonPrecisionOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + cbEje.SelectedIndex) | (byte)CommandType.Release) << 8); 
            //Insertar([(ushort)(((byte)CommandType.PrecisionMode | (byte)CommandType.Release) + dato)], false);
        }
        #endregion

        public void GoToBasic()
        {
        }

        public void GoToAdvanced()
        {
        }
    }
}
