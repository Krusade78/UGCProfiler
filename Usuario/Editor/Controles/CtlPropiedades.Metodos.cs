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
                Eje(idc);
 
            eventos = true;
        }

        private void Boton(byte b, bool seta)
        {
            //'reset
            NumericUpDownPosition.Maximum = 15;
            NumericUpDownPosition.IsEnabled = true;
            //'/--------

            byte idJ = 0, p = 0, m = 0;
            byte st;

            padre.GetModos(ref idJ, ref p, ref m);
            if (seta)
                st = padre.GetDatos().Perfil.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, b).TamIndices;
            else
                st = padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, b).TamIndices;

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
                    if ((b == 3) && (idJ == 1))
                        ButtonAssignPinkie.Visibility = System.Windows.Visibility.Visible;
                    else
                    {
                        if ((b > 4) && (b < 8) && (idJ == 1))
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

        private void Eje(byte e)
        {
            NumericUpDownPosition.Maximum = 16;
            NumericUpDownPosition.IsEnabled = true;

            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            byte neje = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m , e).Eje;
            byte tipoEje = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m , e).TipoEje;

            CheckBoxInverted.IsChecked = ((tipoEje & 0b10) == 0b10); //invertido

            nudJoy.Value = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).JoySalida + 1;
            ComboBoxAxes.SelectedIndex = ((tipoEje & 0b1) == 0) ? 0 : neje + 1;
            ButtonSensibility.IsEnabled = true;

            if ((tipoEje & 0b1000) == 0b1000) //ratón
            {
                ComboBoxAxes.SelectedIndex = neje + 12;
                LabelMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.Value = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).Mouse;
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
                NumericUpDownResistanceInc.Value = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).ResistenciaInc;
                NumericUpDownResistanceDec.Value = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).ResistenciaDec;
                //'macros
                PanelMacro.Visibility = System.Windows.Visibility.Collapsed;
                RadioButton1.Content = "Incrementar";
                RadioButton2.Content = "Reducir";
            }
            else if ((tipoEje & 0b100000) == 0b100000) //bandas
            {
                RadioButtonBands.IsChecked = true;
                byte[] bandas;
                bandas = padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).Bandas;
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

        private void CargarIndexMacro()
        {
            eventos = false;

            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            if (tipoActual == Tipo.Seta)
            {
                if (padre.GetDatos().Perfil.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, idActual).TamIndices == 0)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, 0).idAccion : padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, 1).idAccion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Value - 1)).idAccion;
            }
            else if (tipoActual == Tipo.Boton)
            {
                if (padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, idActual).TamIndices == 0)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 0).idAccion : padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 1).idAccion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Value - 1)).idAccion;
            }
            else if (tipoActual == Tipo.Eje)
            {
                if ((padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje & 0b10000) == 0b10000)
                    ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetDatos().Perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 0).idAccion : padre.GetDatos().Perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 1).idAccion;
                else
                    ComboBoxAssigned.SelectedValue = padre.GetDatos().Perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Value - 1)).idAccion;
            }

            eventos = true;
        }
        #endregion

        #region "Editar"

        #region "Ejes"
        private void SetEje()
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);

            padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje &= 0b1111_0000; //tipos a 0
            padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = 0; //X por defecto
            padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).JoySalida = (byte)(nudJoy.Value - 1);
            if (ComboBoxAxes.SelectedIndex > 9) //raton
            {
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b1000;
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = (byte)(ComboBoxAxes.SelectedIndex - 11);
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b10; //invertido
            }
            else if (ComboBoxAxes.SelectedIndex > 0)
            {
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b1;
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = (byte)(ComboBoxAxes.SelectedIndex - 1);
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b10; //invertido
            }

            padre.GetDatos().Modificado = true;
            Refrescar();
        }

        private void SetSensibilidadRaton()
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Mouse = (byte)NumericUpDownMSensibility.Value;
            padre.GetDatos().Modificado = true;
        }

        private void SetModoEje()
        {
            byte idJ = 0, p = 0, m = 0;
            byte inc = 0b10000;
            byte bandas = 0b100000;
            padre.GetModos(ref idJ, ref p, ref m);
            padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje &= (byte)~inc;
            padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje &= (byte)~bandas;
            if (RadioButtonBands.IsChecked == true)
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= bandas;
            else if (RadioButtonIncremental.IsChecked == true)
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= inc;

            padre.GetDatos().Modificado = true;
            Refrescar();
        }

        private void SetResistencia(bool inc)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            if (inc)
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).ResistenciaInc = (byte)NumericUpDownResistanceInc.Value;
            else
                padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).ResistenciaDec = (byte)NumericUpDownResistanceDec.Value;

            padre.GetDatos().Modificado = true;
        }
        #endregion

        #region "Botones"
        private void SetModoBoton(byte posiciones)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);

            if (tipoActual == CEnums.Tipo.Seta)
                padre.GetDatos().Perfil.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, idActual).TamIndices = posiciones;
            else
                padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, idActual).TamIndices = posiciones;

            padre.GetDatos().Modificado = true;
            Refrescar();
        }
        #endregion

        #region "Macros"
        private void AsignarMacro(ushort idc)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            switch (tipoActual)
            {
                case Tipo.Seta:
                    {
                        if (padre.GetDatos().Perfil.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, idActual).TamIndices == 0)
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, 0).idAccion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p , m, idActual, 1).idAccion = idc;
                        }
                        else
                            padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Value - 1)).idAccion = idc;
                        break;
                    }
                case Tipo.Boton:
                    {
                        if (padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, idActual).TamIndices == 0)
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 0).idAccion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 1).idAccion = idc;
                        }
                        else
                            padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Value - 1)).idAccion = idc;
                        break;
                    }
                case Tipo.Eje:
                    {
                        if ((padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje & 0b10000) == 0b10000) // incremental
                        {
                            if (RadioButton1.IsChecked == true)
                                padre.GetDatos().Perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 0).idAccion = idc;
                            else
                                padre.GetDatos().Perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 1).idAccion = idc;
                        }
                        else if ((padre.GetDatos().Perfil.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje & 0b100000) == 0b100000) // bandas
                            padre.GetDatos().Perfil.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Value - 1)).idAccion = idc;

                        break;
                    }
            }

            padre.GetDatos().Modificado = true;
        }
        #endregion

        #endregion
    }
}
