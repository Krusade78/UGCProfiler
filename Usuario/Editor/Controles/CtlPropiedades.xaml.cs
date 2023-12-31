﻿using System;
using System.Windows;
using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlEdicionElemento.xaml
    /// </summary>
    internal partial class CtlPropiedades : UserControl
    {
        public CtlPropiedades()
        {
            InitializeComponent();
            padre = (MainWindow)App.Current.MainWindow;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
#endif
            ComboBoxAssigned.DataContext = padre.GetDatos().Perfil.ACCIONES;
            ComboBoxAssigned.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Nombre", System.ComponentModel.ListSortDirection.Ascending));
            ComboBoxAssigned.SelectedIndex = 0;
            ComboBoxMacro.DataContext = new System.Data.DataView(padre.GetDatos().Perfil.ACCIONES);
            ((System.Data.DataView)ComboBoxMacro.DataContext).RowFilter = "idAccion <> 0";
            ((System.Data.DataView)ComboBoxMacro.DataContext).Sort = "Nombre";
            ComboBoxMacro.SelectedIndex = 0;
        }

        #region "Ejes"
        private void ComboBoxAxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetEje();
        }

        private void CheckBoxInverted_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                SetEje();
            }
        }

        private void ButtonSensibility_Click(object sender, RoutedEventArgs e)
        {
            VEditorSensibilidad dlg = new VEditorSensibilidad(idActual)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
                padre.GetDatos().Modificado = true;
        }

        private void ButtonCopiaDe_Click(object sender, RoutedEventArgs e)
        {
            VCopiaDe dlg = new VCopiaDe(idActual)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
                padre.GetDatos().Modificado = true;
        }

        private void NumericUpDownMSensibility_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetSensibilidadRaton();
        }

        private void RadioButtonIncremental_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                eventos = false;
                RadioButtonBands.IsChecked = false;
                eventos = true;
                SetModoEje();
            }
        }
        private void RadioButtonBands_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                eventos = false;
                RadioButtonIncremental.IsChecked = false;
                eventos = true;
                SetModoEje();
            }
        }

        private void NumericUpDownResistanceInc_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetResistencia(true);
        }

        private void NumericUpDownResistanceDec_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetResistencia(false);
        }

        private void ButtonEditBands_Click(object sender, RoutedEventArgs e)
        {
            VEditorBandas dlg = new VEditorBandas(idActual)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
            {
                padre.GetDatos().Modificado = true;
                NumericUpDownPosition.Valor = 1;
            }
        }
        #endregion

        #region "botones"
        private void RadioButtonUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
                SetModoBoton(0);
        }

        private void RadioButtonToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
                SetModoBoton(1);
        }

        private void NumericUpDownPositions_TextChanged(object sender, EventArgs e)
        {
            if (eventos)
                SetModoBoton((byte)NumericUpDownPositions.Valor);
        }

        private void ButtonAssignModes_Click(object sender, RoutedEventArgs e)
        {
            VEditorPinkieModos dlg = new VEditorPinkieModos(true)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
            {
                padre.GetDatos().Modificado = true;
                Ver(idActual, tipoActual, "");
            }
        }

        private void ButtonAssignPinkie_Click(object sender, RoutedEventArgs e)
        {
            VEditorPinkieModos dlg = new VEditorPinkieModos(false)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
            {
                padre.GetDatos().Modificado = true;
                Ver(idActual, tipoActual, "");
            }
        }

        private void ButtonAssignPOV_Click(object sender, RoutedEventArgs e)
        {
            VEditorPOV dlg = new VEditorPOV(idActual)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
            {
                padre.GetDatos().Modificado = true;
                Ver(idActual, tipoActual, "");
            }
        }
        #endregion

        #region "Macros"
        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                eventos = false;
                RadioButton1.IsChecked = false;
                eventos = true;
                Refrescar();
            }
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                eventos = false;
                RadioButton2.IsChecked = false;
                eventos = true;
                Refrescar();
            }
        }

        private void NumericUpDownPosition_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                Refrescar();
        }

        private void ComboBoxAssigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && (ComboBoxMacro.SelectedIndex != -1) && eventos && (ComboBoxAssigned.SelectedValue != null))
                AsignarMacro((ushort)ComboBoxAssigned.SelectedValue);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            VEditorMacros dlg = new VEditorMacros(-1)
            {
                Owner = App.Current.MainWindow
            };
            if (dlg.ShowDialog() == true)
                padre.GetDatos().Modificado = true;
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxMacro.SelectedIndex != -1)
            {
                VEditorMacros dlg = new VEditorMacros((int)(ushort)ComboBoxMacro.SelectedValue)
                {
                    Owner = App.Current.MainWindow
                };
                if (dlg.ShowDialog() == true)
                    padre.GetDatos().Modificado = true;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxMacro.SelectedIndex != -1)
                padre.GetDatos().Perfil.ACCIONES.Rows.Remove(padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)ComboBoxMacro.SelectedValue));
        }
        #endregion
    }
}
