using Avalonia.Interactivity;
using Avalonia.Controls;
using static Shared.CTypes;


namespace Profiler.Pages.Macros
{
    public partial class CtlModes : UserControl, IChangeMode
    {
        public CtlModes()
        {
            this.InitializeComponent();
        }


        #region "Modes"
        private void ButtonSetMode_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(uint)CommandType.Mode + (uint)(cbMode.SelectedIndex << 8)]);
        }


        private void ButtonSetSubmode_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
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
