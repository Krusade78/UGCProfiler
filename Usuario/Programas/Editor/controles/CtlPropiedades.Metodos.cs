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
            eventos = false;

            if (nombre != "")
            {
                Label2.Content = nombre;
                RadioButton1.IsChecked = true;
                RadioButton2.IsChecked = false;
                NumericUpDownPosition.Value = 1;
            }
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
                MiniStick(idc);

            eventos = true;
        }

        private void Boton(byte b, bool seta)
        {
            //'reset
            NumericUpDownPosition.Maximum = 15;
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
                RadioButtonUpDown.IsChecked = false;
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
                RadioButtonToggle.IsChecked = false;
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

            CargarIndexMacro();
        }

        private void Eje(byte e, bool peque)
        {
            ComboBoxAxes.Visibility = System.Windows.Visibility.Visible;
            ComboBoxAxesMini.Visibility = System.Windows.Visibility.Hidden;
            NumericUpDownPosition.Maximum = 16;

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
                NumericUpDownResistanceInc.Value = (!peque) ? padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(2, e, m, p).Indice : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(2, e, m, p).Indice;
                NumericUpDownResistanceDec.Value = (!peque) ? padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(3, e, m, p).Indice : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(3, e, m, p).Indice;
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

            CargarIndexMacro();
        }

        private void MiniStick(byte id)
        {
            ComboBoxAxes.Visibility = System.Windows.Visibility.Hidden;
            ComboBoxAxesMini.Visibility = System.Windows.Visibility.Visible;
            NumericUpDownPosition.Maximum = 16;

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

            if (neje == 0)
                ComboBoxAxesMini.SelectedIndex = 0;
            else
                ComboBoxAxesMini.SelectedIndex = neje - 7;
            if (neje > 9)
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
        }

        private void CargarIndexMacro()
        {
            eventos = false;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Seta)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(0, idActual, m, p).Indice : padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(1, idActual, m, p).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Indice;
            }
            else if (tipoActual == Tipo.Boton)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 0).Indice : padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 1).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, (byte)(NumericUpDownPosition.Value - 1)).Indice;
            }
            else if (tipoActual == Tipo.Eje)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(0, idActual, m, p).Indice : padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(1, idActual, m, p).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1),idActual, m, p).Indice;
            }
            else if (tipoActual == Tipo.EjePeque)
            {
                if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(0, idActual, m, p).Indice : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(1, idActual, m, p).Indice;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Indice;
            }

            eventos = true;
        }
        #endregion

        #region "Editar"

        #region "Ejes"
        private void SetEje()
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Eje)
            {
                byte inc = (byte)(padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).nEje & 128);
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).nEje = (byte)(inc | (ComboBoxAxes.SelectedIndex + 20));
                else
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).nEje = (byte)(inc | ComboBoxAxes.SelectedIndex);
            }
            else if (tipoActual == Tipo.EjePeque)
            {
                byte inc = (byte)(padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).nEje & 128);
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).nEje = (byte)(inc | (ComboBoxAxes.SelectedIndex + 20));
                else
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).nEje = (byte)(inc | ComboBoxAxes.SelectedIndex);

            }
            padre.GetDatos().Modificado = true;
            Refrescar();
        }

        private void SetEjeMini()
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);

            byte sel = (ComboBoxAxesMini.SelectedIndex == 0) ? (byte)0 : (byte)(ComboBoxAxesMini.SelectedIndex + 7);
            if (CheckBoxInverted.IsChecked == true)
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).nEje = (byte)(sel + 20);
            else
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).nEje = sel;

            padre.GetDatos().Modificado = true;
            Refrescar();
        }

        private void SetSensibilidadRaton()
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Eje)
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).Mouse = (byte)NumericUpDownMSensibility.Value;
            else if (tipoActual == Tipo.EjePeque)
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).Mouse = (byte)NumericUpDownMSensibility.Value;
            else
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).Mouse = (byte)NumericUpDownMSensibility.Value;

            padre.GetDatos().Modificado = true;
        }

        private void SetModoEje()
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            byte inc = (RadioButtonBands.IsChecked == true) ? (byte)0 : (byte)128;
            if (tipoActual == Tipo.Eje)
            {
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).nEje &= 127;
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).nEje |= inc;
                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(0, idActual, m, p).Indice = 0;
                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(1, idActual, m, p).Indice = 0;
                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(2, idActual, m, p).Indice = 0;
                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(3, idActual, m, p).Indice = 0;
            }
            else
            {
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).nEje &= 127;
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).nEje |= inc;
                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(0, idActual, m, p).Indice = 0;
                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(1, idActual, m, p).Indice = 0;
                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(2, idActual, m, p).Indice = 0;
                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(3, idActual, m, p).Indice = 0;
            }

            padre.GetDatos().Modificado = true;
        }

        private void SetResistencia(bool inc)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Eje)
                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie((byte)((inc) ? 2 : 3), idActual, m, p).Indice = (byte)((inc) ? NumericUpDownResistanceInc.Value : NumericUpDownResistanceDec.Value);
            else
                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie((byte)((inc) ? 2 : 3), idActual, m, p).Indice = (byte)((inc) ? NumericUpDownResistanceInc.Value : NumericUpDownResistanceDec.Value);

            padre.GetDatos().Modificado = true;
        }
        #endregion

        #region "Botones"
        private void SetModoBoton(byte tipo)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);

            if (tipoActual == CEnums.Tipo.Seta)
                padre.GetDatos().Perfil.MAPASETAS.FindByidSetaidModoidPinkie(idActual, m, p).Estado = tipo;
            else
                padre.GetDatos().Perfil.MAPABOTONES.FindByidBotonidModoidPinkie(idActual, m, p).Estado = tipo;

            padre.GetDatos().Modificado = true;
            Refrescar();
        }
        #endregion

        #region "Macros"
        private void AsignarMacro(ushort idc)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            switch (tipoActual)
            {
                case Tipo.Seta:
                    if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    {
                        if (RadioButton1.IsChecked == true)
                            padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(0, idActual, m, p).Indice = idc;
                        else
                            padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(1, idActual, m, p).Indice = idc;
                    }
                    else
                        padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Indice = idc;
                    break;
                case Tipo.Boton:
                    if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    {
                        if (RadioButton1.IsChecked == true)
                            padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 0).Indice = idc;
                        else
                            padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 1).Indice = idc;
                    }
                    else
                        padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, (byte)(NumericUpDownPosition.Value - 1)).Indice = idc;
                    break;
                case Tipo.Eje:
                    if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    {
                        if (RadioButton1.IsChecked == true)
                            padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(0, idActual, m, p).Indice = idc;
                        else
                            padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(1, idActual, m, p).Indice = idc;
                    }
                    else
                        padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Indice = idc;

                    break;
                case Tipo.EjePeque:
                    if (RadioButton1.Visibility == System.Windows.Visibility.Visible)
                    {
                        if (RadioButton1.IsChecked == true)
                            padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(0, idActual, m, p).Indice = idc;
                        else
                            padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(1, idActual, m, p).Indice = idc;
                    }
                    else
                        padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Indice = idc;

                    break;
            }

            padre.GetDatos().Modificado = true;
        }
        #endregion

        #endregion
    }
}
