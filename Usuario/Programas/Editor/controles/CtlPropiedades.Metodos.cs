using System;
using System.Windows.Controls;
using static Editor.CEnums;

namespace Editor
{
    internal partial class CtlPropiedades : UserControl
    {
        private MainWindow padre;
        private byte idActual = 0;
        private Tipo tipoActual;
        private bool eventos = true;

        public void Refrescar()
        {
            Ver(idActual, tipoActual, "");
        }

        #region "Cargar"
        public void Ver(byte idc, Tipo tipo, String nombre)
        {
            if (nombre != "")
                Label2.Content = nombre;
            idActual = idc;
            tipoActual = tipo;
            if (tipo == Tipo.Seta)
                Boton(idc, true);
            else if (tipo == Tipo.Boton)
                Boton(idc, false);
            else if (tipo == Tipo.Eje)
                Eje(idc, false);
            else if (tipo == Tipo.EjePeque)
                Eje(idc, true);
            else
                MiniStick((byte)(idc - 64));
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
                RadioButtonToggle.IsChecked = true;
                LabelPositions.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownPositions.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownPositions.Value = st;
                //macros
                RadioButton1.Visibility = System.Windows.Visibility.Hidden;
                RadioButton2.Visibility = System.Windows.Visibility.Hidden;
                LabelPosition.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownPosition.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                RadioButtonUpDown.IsChecked = true;
                LabelPositions.Visibility = System.Windows.Visibility.Hidden;
                NumericUpDownPositions.Visibility = System.Windows.Visibility.Hidden;
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
                //macros
                LabelPosition.Visibility = System.Windows.Visibility.Hidden;
                NumericUpDownPosition.Visibility = System.Windows.Visibility.Hidden;
                RadioButton1.Visibility = System.Windows.Visibility.Visible;
                RadioButton1.Content = "Presionado";
                RadioButton2.Visibility = System.Windows.Visibility.Visible;
                RadioButton2.Content = "Soltado";
            }

            PanelButton.Visibility = System.Windows.Visibility.Collapsed;
            PanelMapaEjes.Visibility = System.Windows.Visibility.Visible;
            PanelDigital.Visibility = System.Windows.Visibility.Collapsed;
            PanelMacro.Visibility = System.Windows.Visibility.Collapsed;

            eventos = true;
            CargarIndexMacro();
        }

        private void Eje(byte e, bool peque)
        {
            eventos = false;
            NumericUpDownPosition.Maximum = 16;
            NumericUpDownPosition.Value = 1;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            byte neje = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).nEje : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).nEje;
            bool incremental = ((neje & 128) == 128);
            neje &= 127;

            if (neje > 19)
            {
                CheckBoxInverted.IsChecked = true;
                neje -= 20;
            }
            else
                CheckBoxInverted.IsChecked = false;

            ComboBoxAxes.SelectedIndex = neje;
            ButtonSensibility.IsEnabled = (!peque);

            if (neje > 10) //ratón
            {
                LabelMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.Value = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).Mouse : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).Mouse;
            }
            else
            {
                LabelMSensibility.IsEnabled = false;
                NumericUpDownMSensibility.IsEnabled = false;
            }

            if (incremental)
            {
                RadioButtonBands.IsChecked = false;
                RadioButtonIncremental.IsChecked = true;
                ButtonEditBands.Visibility = System.Windows.Visibility.Hidden;
                PanelIncremental.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownResistanceInc.Value = (!peque) ? padre.GetDatos().Perfil.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 0).Indice : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 0).Indice;
                NumericUpDownResistanceDec.Value = (!peque) ? padre.GetDatos().Perfil.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 1).Indice : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 1).Indice;
                //'macros
                LabelPosition.Visibility = System.Windows.Visibility.Hidden;
                NumericUpDownPosition.Visibility = System.Windows.Visibility.Hidden;
                RadioButton1.Visibility = System.Windows.Visibility.Visible;
                RadioButton2.Visibility = System.Windows.Visibility.Visible;
                RadioButton1.IsChecked = true;
                RadioButton2.IsChecked = false;
                RadioButton1.Content = "Incrementar";
                RadioButton2.Content = "Reducir";
            }
            else
            {
                RadioButtonBands.IsChecked = true;
                RadioButtonIncremental.IsChecked = false;
                ButtonEditBands.Visibility = System.Windows.Visibility.Visible;
                PanelIncremental.Visibility = System.Windows.Visibility.Hidden;
                //'macros
                LabelPosition.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownPosition.Visibility = System.Windows.Visibility.Visible;
                RadioButton1.Visibility = System.Windows.Visibility.Hidden;
                RadioButton2.Visibility = System.Windows.Visibility.Hidden;
            }

            PanelMapaEjes.Visibility = System.Windows.Visibility.Collapsed;
            PanelDigital.Visibility = System.Windows.Visibility.Collapsed;
            PanelButton.Visibility = System.Windows.Visibility.Visible;
            PanelMacro.Visibility = System.Windows.Visibility.Collapsed;

            eventos = true;
            CargarIndexMacro();
        }

        private void MiniStick(byte id)
        {
            eventos = false;
            NumericUpDownPosition.Maximum = 16;
            NumericUpDownPosition.Value = 1;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            byte neje = padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(id, m, p).nEje;
            if (neje > 19)
            {
                CheckBoxInverted.IsChecked = true;
                neje -= 20;
            }
            else
                CheckBoxInverted.IsChecked = false;

            ComboBoxAxes.SelectedIndex = neje;
            if (neje > 10)
            {
                LabelMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.Value = padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(id, m, p).Mouse;
            }
            else
            {
                LabelMSensibility.IsEnabled = false;
                NumericUpDownMSensibility.IsEnabled = false;
            }

            PanelMapaEjes.Visibility = System.Windows.Visibility.Collapsed;
            PanelButton.Visibility = System.Windows.Visibility.Visible;
            PanelDigital.Visibility = System.Windows.Visibility.Visible;
            PanelMacro.Visibility = System.Windows.Visibility.Collapsed;

            eventos = true;
        }

        private void CargarIndexMacro()
        {
            eventos = false;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Seta)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | idActual), 0).Indice : padre.GetDatos().Perfil.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | idActual), 1).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | (idActual - 100)), (byte)(NumericUpDownPosition.Value - 1)).Indice;
            }
            else if (tipoActual == Tipo.Boton)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | idActual), 0).Indice : padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | idActual), 1).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | idActual), (byte)(NumericUpDownPosition.Value - 1)).Indice;
            }
            else if (tipoActual == Tipo.Eje)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | idActual), 0).Indice : padre.GetDatos().Perfil.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | idActual), 1).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | idActual), (byte)(NumericUpDownPosition.Value - 1)).Indice;
            }
            else if (tipoActual == Tipo.EjePeque)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | idActual), 0).Indice : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | idActual), 1).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | idActual), (byte)(NumericUpDownPosition.Value - 1)).Indice;
            }

            eventos = true;
        }
        #endregion

        #region "Editar"

        #region "Ejes"
        private void SetEje()
        {
            //byte p = 0, m = 0, ej = ((idActual > 63) && (idActual < 100)) ? (byte)(idActual - 64) : idActual;
            //padre.GetModos(ref p, ref m);
            //if (CheckBoxInverted.IsChecked == true)
            //    padre.GetDatos().Perfil.mSetMapaEjes_nEje(p, m, ej, (datos.GetMapaEjes_nEje(p, m, a, ej) And 128) Or(ComboBoxAxes.SelectedIndex + 20))
            //else
            //    padre.GetDatos().SetMapaEjes_nEje(p, m, ej, (datos.GetMapaEjes_nEje(p, m, a, ej) And 128) Or ComboBoxAxes.SelectedIndex)

            //Refrescar();
        }
        #endregion

        #region "Macros"
        private void AsignarMacro(ushort idc)
        {

        }
        #endregion

        #endregion
    }
}
