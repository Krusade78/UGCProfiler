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
            NumericUpDownPosition.IsEnabled = true;
            //'/--------

            byte p = 0, m = 0;
            byte st;

            padre.GetModos(ref p, ref m);
            if (seta)
                st = padre.GetDatos().Perfil.MAPASETAS.FindByidPinkieidModoIdSeta(p, m, b).TamIndices;
            else
                st = padre.GetDatos().Perfil.MAPABOTONES.FindByidPinkieidModoidBoton(p, m, b).TamIndices;

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
                NumericUpDownPosition.Maximum = st;
                //macros
                panelPS.Visibility = System.Windows.Visibility.Hidden;
                panelPos.Visibility = System.Windows.Visibility.Visible;
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
                panelPos.Visibility = System.Windows.Visibility.Hidden;
                panelPS.Visibility = System.Windows.Visibility.Visible;
                RadioButton1.Content = "Presionado";
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
            NumericUpDownPosition.IsEnabled = true;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            byte neje = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).Eje : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).Eje;
            byte tipoEje = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).TipoEje : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).TipoEje;

            CheckBoxInverted.IsChecked = ((tipoEje & 0b10) == 0b10); //invertido

            ComboBoxAxes.SelectedIndex = ((tipoEje & 0b1) == 0) ? 0 : neje + 1;
            ButtonSensibility.IsEnabled = (!peque);

            if ((tipoEje & 0b1000) == 0b1000) //ratón
            {
                ComboBoxAxes.SelectedIndex = neje + 12;
                LabelMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.Value = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).Mouse : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).Mouse;
            }
            else
            {
                LabelMSensibility.IsEnabled = false;
                NumericUpDownMSensibility.IsEnabled = false;
            }

            RadioButtonBands.IsChecked = false;
            RadioButtonIncremental.IsChecked = false;
            panelPos.Visibility = System.Windows.Visibility.Hidden;
            panelPS.Visibility = System.Windows.Visibility.Visible;
            PanelMacro.Visibility = System.Windows.Visibility.Visible;
            if ((tipoEje & 0b10000) == 0b10000) //incremental
            {
                RadioButtonIncremental.IsChecked = true;
                NumericUpDownResistanceInc.Value = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).ResistenciaInc : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).ResistenciaInc;
                NumericUpDownResistanceDec.Value = (!peque) ? padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).ResistenciaDec : padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).ResistenciaDec;
                //'macros
                PanelMacro.Visibility = System.Windows.Visibility.Collapsed;
                RadioButton1.Content = "Incrementar";
                RadioButton2.Content = "Reducir";
            }
            else if ((tipoEje & 0b100000) == 0b100000) //bandas
            {
                RadioButtonBands.IsChecked = true;
                byte[] bandas;
                if (!peque)
                {
                    bandas = padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).Bandas;
                }
                else
                {
                    bandas = padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).Bandas;
                }
                NumericUpDownPosition.Maximum = 1;
                foreach (byte b in bandas)
                {
                    if (b == 0)
                        break;
                    else
                        NumericUpDownPosition.Maximum++;
                }

                //'macros
                panelPos.Visibility = System.Windows.Visibility.Visible;
                panelPS.Visibility = System.Windows.Visibility.Hidden;
                PanelMacro.Visibility = System.Windows.Visibility.Collapsed;
            }

            PanelMapaEjes.Visibility = System.Windows.Visibility.Collapsed;
            PanelDigital.Visibility = System.Windows.Visibility.Collapsed;
            PanelButton.Visibility = System.Windows.Visibility.Visible;

            CargarIndexMacro();
        }

        private void MiniStick(byte id)
        {
            ComboBoxAxes.Visibility = System.Windows.Visibility.Hidden;
            ComboBoxAxesMini.Visibility = System.Windows.Visibility.Visible;
            NumericUpDownPosition.Maximum = 16;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            byte neje = padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(id, m, p).Eje;
            byte tipoEje = padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(id, m, p).TipoEje;

            CheckBoxInverted.IsChecked = ((tipoEje & 0b10) == 0b10); //invertido

            ComboBoxAxesMini.SelectedIndex = ((tipoEje & 0b100) == 0b100) ? neje + 1 : 0;

            if ((padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(id, m, p).TipoEje & 0b1000) == 0b1000) //ratón
            {
                ComboBoxAxesMini.SelectedIndex = neje + 4;
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
            panelPos.Visibility = System.Windows.Visibility.Hidden;
            panelPS.Visibility = System.Windows.Visibility.Visible;
            PanelMacro.Visibility = System.Windows.Visibility.Visible;
        }

        private void CargarIndexMacro()
        {
            eventos = false;

            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Seta)
            {
                if (padre.GetDatos().Perfil.MAPASETAS.FindByidPinkieidModoIdSeta(p, m, idActual).TamIndices == 0)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(0, idActual, m, p).Accion : padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(1, idActual, m, p).Accion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Accion;
            }
            else if (tipoActual == Tipo.Boton)
            {
                if (padre.GetDatos().Perfil.MAPABOTONES.FindByidPinkieidModoidBoton(p, m, idActual).TamIndices == 0)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 0).Accion : padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 1).Accion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, (byte)(NumericUpDownPosition.Value - 1)).Accion;
            }
            else if (tipoActual == Tipo.Eje)
            {
                if ((padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje & 0b10000) == 0b10000)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(0, idActual, m, p).Accion : padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(1, idActual, m, p).Accion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Accion;
            }
            else if (tipoActual == Tipo.EjePeque)
            {
                if ((padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje & 0b10000) == 0b10000)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(0, idActual, m, p).Accion : padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(1, idActual, m, p).Accion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Accion;
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
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= 0b1111_0000; //tipos a 0
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).Eje = 0; //X por defecto
                if (ComboBoxAxes.SelectedIndex > 9) //raton
                {
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b1000;
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).Eje = (byte)(ComboBoxAxes.SelectedIndex - 11);
                }
                else if (ComboBoxAxes.SelectedIndex > 0)
                {
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b1;
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).Eje = (byte)(ComboBoxAxes.SelectedIndex - 1);
                }
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b10; //invertido
            }
            else if (tipoActual == Tipo.EjePeque)
            {
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= 0b1111_0000; // tipos a 0
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).Eje = 0; //X por defecto
                if (ComboBoxAxes.SelectedIndex > 9) //raton
                {
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b1000;
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).Eje = (byte)(ComboBoxAxes.SelectedIndex - 11);
                }
                else if (ComboBoxAxes.SelectedIndex > 0)
                {
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b1;
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).Eje = (byte)(ComboBoxAxes.SelectedIndex - 1);
                }
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b10; //invertido

            }
            padre.GetDatos().Modificado = true;
            Refrescar();
        }

        private void SetEjeMini()
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);

            padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= 0b1111_0000; //ninguno, sin invertir
            padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).Eje = 0; //X por defecto

            if (ComboBoxAxesMini.SelectedIndex > 2) //ratón
            {
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b1000;
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).Eje = (byte)(ComboBoxAxesMini.SelectedIndex - 4);
            }
            else if (ComboBoxAxesMini.SelectedIndex > 0)
            {
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= (byte)0b100;
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).Eje = (byte)(ComboBoxAxesMini.SelectedIndex - 1);
            }
            if (CheckBoxInverted.IsChecked == true)
                padre.GetDatos().Perfil.MAPAEJESMINI.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje = (byte)0b10; //invertido

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
            byte inc = 0b10000;
            byte bandas = 0b100000;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Eje)
            {
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= (byte)~inc;
                padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= (byte)~bandas;
                if (RadioButtonBands.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= bandas;
                else if (RadioButtonIncremental.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= inc;
            }
            else
            {
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= (byte)~inc;
                padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje &= (byte)~bandas;
                if (RadioButtonBands.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= bandas;
                else if (RadioButtonIncremental.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje |= inc;
            }

            padre.GetDatos().Modificado = true;
            Refrescar();
        }

        private void SetResistencia(bool inc)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);
            if (tipoActual == Tipo.Eje)
            {
                if (inc)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).ResistenciaInc = (byte)NumericUpDownResistanceInc.Value;
                else
                    padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).ResistenciaDec = (byte)NumericUpDownResistanceDec.Value;
            }
            else
            {
                if (inc)
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).ResistenciaInc = (byte)NumericUpDownResistanceInc.Value;
                else
                    padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).ResistenciaDec = (byte)NumericUpDownResistanceDec.Value;
            }

            padre.GetDatos().Modificado = true;
        }
        #endregion

        #region "Botones"
        private void SetModoBoton(byte posiciones)
        {
            byte p = 0, m = 0;
            padre.GetModos(ref p, ref m);

            if (tipoActual == CEnums.Tipo.Seta)
                padre.GetDatos().Perfil.MAPASETAS.FindByidPinkieidModoIdSeta(p, m, idActual).TamIndices = posiciones;
            else
                padre.GetDatos().Perfil.MAPABOTONES.FindByidPinkieidModoidBoton(p, m, idActual).TamIndices = posiciones;

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
                    {
                        if (padre.GetDatos().Perfil.MAPASETAS.FindByidPinkieidModoIdSeta(p, m, idActual).TamIndices == 0)
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(0, idActual, m, p).Accion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie(1, idActual, m, p).Accion = idc;
                        }
                        else
                            padre.GetDatos().Perfil.INDICESSETAS.FindByididSetaidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Accion = idc;
                        break;
                    }
                case Tipo.Boton:
                    {
                        if (padre.GetDatos().Perfil.MAPABOTONES.FindByidPinkieidModoidBoton(p, m, idActual).TamIndices == 0)
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 0).Accion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, 1).Accion = idc;
                        }
                        else
                            padre.GetDatos().Perfil.INDICESBOTONES.FindByidBotonidModoidPinkieid(idActual, m, p, (byte)(NumericUpDownPosition.Value - 1)).Accion = idc;
                        break;
                    }
                case Tipo.Eje:
                    {
                        if ((padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje & 0b10000) == 0b10000) // incremental
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(0, idActual, m, p).Accion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie(1, idActual, m, p).Accion = idc;
                        }
                        else if ((padre.GetDatos().Perfil.MAPAEJES.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje & 0b100000) == 0b100000) // bandas
                            padre.GetDatos().Perfil.INDICESEJES.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Accion = idc;

                        break;
                    }
                case Tipo.EjePeque:
                    {
                        if ((padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje & 0b10000) == 0b10000) // incremental
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(0, idActual, m, p).Accion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie(1, idActual, m, p).Accion = idc;
                        }
                        else if ((padre.GetDatos().Perfil.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(idActual, m, p).TipoEje & 0b100000) == 0b100000) // bandas
                            padre.GetDatos().Perfil.INDICESEJESPEQUE.FindByididEjeidModoidPinkie((byte)(NumericUpDownPosition.Value - 1), idActual, m, p).Accion = idc;

                        break;
                    }
            }

            padre.GetDatos().Modificado = true;
        }
        #endregion

        #endregion
    }
}
