using System;
using System.Windows.Controls;

namespace Editor
{
    internal partial class CtlPropiedades : UserControl
    {
        private MainWindow padre;
        private byte idActual = 0;
        private bool eventos = true;

        public void Refrescar()
        {
            Ver(idActual, "");
        }

        public void Ver(byte idc, String nombre)
        {
            if (nombre != "")
                Label2.Content = nombre;
            idActual = idc;
            if (idc > 99)
                Boton((byte)(idc - 100), true);
            else if (idc < 64)
                Boton(idc, false);
            //else if (idc < 71)
            //    Eje(idc - 64);
            //else
            //    MiniStick(idc - 64);
        }

        private void Boton(byte b, bool seta)
        {
            eventos = false;
            //'reset
            RadioButton1.IsChecked = true;
            RadioButton2.IsChecked = false;
            NumericUpDownPosition.Maximum = 15;
            NumericUpDownPosition.Value = 1;
            //'/--------

            byte p = 0, m = 0, st;
            padre.GetModos(ref p, ref m);
            if (seta)
                st = padre.GetDatos().Perfil.MAPASETAS.FindByidSetaidModoidPinkie(b, m, p).Estado;
            else
                st = padre.GetDatos().Perfil.MAPABOTONES.FindByidBotonidModoidPinkie(b, m, p).Estado;

            ButtonAssignPOV.Visibility = System.Windows.Visibility.Hidden;
            ButtonAssignPinkie.Visibility = System.Windows.Visibility.Hidden;
            ButtonAssignModes.Visibility = System.Windows.Visibility.Hidden;
            if (st > 0)
            {
                //    padre.RadioButtonToggle.Checked = True
                //    padre.LabelPositions.Visible = True
                //    padre.NumericUpDownPositions.Visible = True
                //    padre.NumericUpDownPositions.Value = st
                //    'macros
                //    padre.RadioButton1.Visible = False
                //    padre.RadioButton2.Visible = False
                //    padre.LabelPosition.Visible = True
                //    padre.NumericUpDownPosition.Visible = True
                //    'padre.LabelPosition.Enabled = True
                //    'padre.NumericUpDownPosition.Enabled = True
            }
            else
            {
                //    padre.RadioButtonUpDown.Checked = True
                //    padre.LabelPositions.Visible = False
                //    padre.NumericUpDownPositions.Visible = False
                if (seta)
                    ButtonAssignPOV.Visibility = System.Windows.Visibility.Visible;
                else
                {
                    if (b == 6)
                        ButtonAssignPinkie.Visibility = System.Windows.Visibility.Visible;
                    else
                    {
                        if ((b > 7) && (b < 11))
                            ButtonAssignModes.Visibility = System.Windows.Visibility.Visible;
                    }
                }
                //    'macros
                //    padre.LabelPosition.Visible = False
                //    padre.NumericUpDownPosition.Visible = False
                //    padre.RadioButton1.Visible = True
                //    padre.RadioButton1.Text = Traduce.Txt("pressed")
                //    padre.RadioButton2.Visible = True
                //    padre.RadioButton2.Text = Traduce.Txt("raised")
                //    padre.RadioButton1.Enabled = True
                //    padre.RadioButton2.Enabled = True
            }

            //padre.PanelButton.Enabled = True
            //padre.PanelMapaEjes.Enabled = False
            //padre.ComboBoxAssigned.Enabled = True

            //eventos = True
            //CargarIndice()
        }
    }
}
