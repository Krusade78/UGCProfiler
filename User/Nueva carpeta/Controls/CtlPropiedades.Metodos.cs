using System;
using System.Windows.Controls;
using static Editor.CEnums;

namespace Editor
{
    internal partial class CtlPropiedades : UserControl
    {
        private readonly MainWindow padre;
        private byte idActual = 0;
        private CEnums.ElementType tipoActual;
        private bool eventos = true;

        public void Refrescar()
        {
            Ver(idActual, tipoActual, "");
        }

        #region "Cargar"
        public void Ver(byte idc, CEnums.ElementType tipo, String nombre)
        {
            eventos = false;

            if (nombre != "")
            {
                Label2.Content = nombre;
                RadioButton1.IsChecked = true;
                RadioButton2.IsChecked = false;
                NumericUpDownPosition.Valor = 1;
            }
            idActual = idc;
            tipoActual = tipo;

            if (tipo == CEnums.ElementType.Hat)
				Boton(idc, true);
            else if (tipo == CEnums.ElementType.Button)
				Boton(idc, false);
            else if (tipo == CEnums.ElementType.Axis)
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
                st = padre.GetData().Profile.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, b).TamIndices;
            else
                st = padre.GetData().Profile.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, b).TamIndices;

            ButtonAssignPOV.Visibility = System.Windows.Visibility.Hidden;
            ButtonAssignPinkie.Visibility = System.Windows.Visibility.Hidden;
            ButtonAssignModes.Visibility = System.Windows.Visibility.Hidden;
            if (st > 0)
            {
                RadioButtonUpDown.IsChecked = false;
                RadioButtonToggle.IsChecked = true;
                LabelPositions.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownPositions.Visibility = System.Windows.Visibility.Visible;
                NumericUpDownPositions.Valor = st;
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
                    if (((b == 3) && (idJ == (byte)Comunes.CTipos.TipoJoy.X52_Joy)) || ((b == 10) && (idJ == (byte)Comunes.CTipos.TipoJoy.NXT)))
                        ButtonAssignPinkie.Visibility = System.Windows.Visibility.Visible;
                    else
                    {
                        if (((b > 4) && (b < 8) && (idJ == (byte)Comunes.CTipos.TipoJoy.X52_Joy)) ||
                                ((b > 3) && (b < 7) && (idJ == (byte)Comunes.CTipos.TipoJoy.NXT)))
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
            byte neje = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m , e).Eje;
            byte tipoEje = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m , e).TipoEje;

            CheckBoxInverted.IsChecked = ((tipoEje & 0b10) == 0b10); //invertido

            nudJoy.Valor = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).JoySalida + 1;
            if (neje == 6) { neje = 7; }
            else if (neje == 7) { neje = 6; }
            ComboBoxAxes.SelectedIndex = ((tipoEje & 0b1) == 0) ? 0 : neje + 1;
            ButtonSensibility.IsEnabled = true;

            if ((tipoEje & 0b1000) == 0b1000) //ratón
            {
                ComboBoxAxes.SelectedIndex = neje + 10;
                LabelMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.IsEnabled = true;
                NumericUpDownMSensibility.Valor = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).Mouse;
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
                NumericUpDownResistanceInc.Valor = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).ResistenciaInc;
                NumericUpDownResistanceDec.Valor = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).ResistenciaDec;
                //'macros
                PanelMacro.Visibility = System.Windows.Visibility.Collapsed;
                RadioButton1.Content = "Incrementar";
                RadioButton2.Content = "Reducir";
            }
            else if ((tipoEje & 0b100000) == 0b100000) //bandas
            {
                RadioButtonBands.IsChecked = true;
                byte[] bandas;
                bandas = padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).Bandas;
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
            if (tipoActual == CEnums.ElementType.Hat)
            {
                if (padre.GetData().Profile.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, idActual).TamIndices == 0)
					ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, 0).idAccion : padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, 1).idAccion;
                else
					ComboBoxAssigned.SelectedValue = padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Valor - 1)).idAccion;
            }
            else if (tipoActual == CEnums.ElementType.Button)
            {
                if (padre.GetData().Profile.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, idActual).TamIndices == 0)
					ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetData().Profile.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 0).idAccion : padre.GetData().Profile.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 1).idAccion;
                else
					ComboBoxAssigned.SelectedValue = padre.GetData().Profile.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Valor - 1)).idAccion;
            }
            else if (tipoActual == CEnums.ElementType.Axis)
            {
                if ((padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje & 0b10000) == 0b10000)
					ComboBoxAssigned.SelectedValue = (RadioButton1.IsChecked == true) ? padre.GetData().Profile.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 0).idAccion : padre.GetData().Profile.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 1).idAccion;
                else
					ComboBoxAssigned.SelectedValue = padre.GetData().Profile.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Valor - 1)).idAccion;
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

            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje &= 0b1111_0000; //tipos a 0
            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = 0; //X por defecto
            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).JoySalida = (byte)(nudJoy.Valor - 1);
            if (ComboBoxAxes.SelectedIndex > 9) //raton
            {
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b1000;
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = (byte)(ComboBoxAxes.SelectedIndex - 10);
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b10; //invertido
            }
            else if (ComboBoxAxes.SelectedIndex > 0)
            {
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b1;
                if ((ComboBoxAxes.SelectedIndex - 1) == 6)
                {
                    padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = 7;
                }
                else if ((ComboBoxAxes.SelectedIndex - 1) == 7)
                {
                    padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = 6;
                }
                else
                    padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Eje = (byte)(ComboBoxAxes.SelectedIndex - 1);
                if (CheckBoxInverted.IsChecked == true)
                    padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= (byte)0b10; //invertido
            }

            padre.GetData().Modified = true;
            Refrescar();
        }

        private void SetSensibilidadRaton()
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).Mouse = (byte)NumericUpDownMSensibility.Valor;
            padre.GetData().Modified = true;
        }

        private void SetModoEje()
        {
            byte idJ = 0, p = 0, m = 0;
            byte inc = 0b10000;
            byte bandas = 0b100000;
            padre.GetModos(ref idJ, ref p, ref m);
            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje &= (byte)~inc;
            padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje &= (byte)~bandas;
            if (RadioButtonBands.IsChecked == true)
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= bandas;
            else if (RadioButtonIncremental.IsChecked == true)
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje |= inc;

            padre.GetData().Modified = true;
            Refrescar();
        }

        private void SetResistencia(bool inc)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);
            if (inc)
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).ResistenciaInc = (byte)NumericUpDownResistanceInc.Valor;
            else
                padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).ResistenciaDec = (byte)NumericUpDownResistanceDec.Valor;

            padre.GetData().Modified = true;
        }
        #endregion

        #region "Botones"
        private void SetModoBoton(byte posiciones)
        {
            byte idJ = 0, p = 0, m = 0;
            padre.GetModos(ref idJ, ref p, ref m);

            if (tipoActual == CEnums.ElementType.Hat)
                padre.GetData().Profile.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, idActual).TamIndices = posiciones;
            else
                padre.GetData().Profile.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, idActual).TamIndices = posiciones;

            padre.GetData().Modified = true;
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
                case CEnums.ElementType.Hat:
                    {
                        if (padre.GetData().Profile.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, idActual).TamIndices == 0)
                        {
                            if (RadioButton1.IsChecked == true)
								padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, 0).idAccion = idc;
                            else
								padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p , m, idActual, 1).idAccion = idc;
                        }
                        else
							padre.GetData().Profile.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Valor - 1)).idAccion = idc;
                        break;
                    }
                case CEnums.ElementType.Button:
                    {
                        if (padre.GetData().Profile.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, idActual).TamIndices == 0)
                        {
                            if (RadioButton1.IsChecked == true)
								padre.GetData().Profile.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 0).idAccion = idc;
                            else
								padre.GetData().Profile.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, 1).idAccion = idc;
                        }
                        else
							padre.GetData().Profile.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Valor - 1)).idAccion = idc;
                        break;
                    }
                case CEnums.ElementType.Axis:
                    {
                        if ((padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje & 0b10000) == 0b10000) // incremental
                        {
                            if (RadioButton1.IsChecked == true)
								padre.GetData().Profile.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 0).idAccion = idc;
                            else
								padre.GetData().Profile.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, 1).idAccion = idc;
                        }
                        else if ((padre.GetData().Profile.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, idActual).TipoEje & 0b100000) == 0b100000) // bandas
							padre.GetData().Profile.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, idActual, (byte)(NumericUpDownPosition.Valor - 1)).idAccion = idc;

                        break;
                    }
            }

            padre.GetData().Modified = true;
        }
        #endregion

        #endregion
    }
}
