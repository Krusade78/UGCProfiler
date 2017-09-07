using System;
using System.Windows;

namespace Editor
{
    public partial class MainWindow
    {
        private void Nuevo()
        {
            if (datos.GetModificado())
            {
                MessageBoxResult r = MessageBox.Show("¿Quieres guardar los cambios?", "Advertencia", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (r == MessageBoxResult.Cancel)
                    return;
                else if (r == MessageBoxResult.Yes)
                {
                    if (!Guardar())
                        return;
                }
            }
            Label1.Visible = False
            RadioButton13.Visible = False
            RadioButton14.Visible = False
            ComboBoxMacro.Items.Clear()
            ComboBoxAssigned.Items.Clear()
            ComboBoxAssigned.Items.Add("</------- " & Traduce.Txt("none") & " -------/>")
            ComboBoxAssigned.SelectedIndex = 0
            datos = Nothing
            datos = new Datos();
            If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy) : Me.Propiedades.Enabled = False
            Vista.Clear(False);
            this.Title = "Editor de perfiles - [Sin nombre]";
            esNuevo = true;
            if (report != null)
            {
                Fondo.Visible = True;
                Me.Controls.Remove(report);
            }
        }

        private void Abrir(String archivo = null)
        {
            if (archivo == null)
            {
                if (datos.GetModificado())
                {
                    MessageBoxResult r = MessageBox.Show("¿Quieres guardar los cambios?", "Advertencia", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                    if (r == MessageBoxResult.Cancel)
                        return;
                    else if (r == MessageBoxResult.Yes)
                    {
                        if (!Guardar())
                            return;
                    }
                }
                if (OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK)
                    archivo = OpenFileDialog1.FileName;
            }
            if (archivo != null)
            {
                ComboBoxMacro.Items.Clear()
                ComboBoxAssigned.Items.Clear()
                ComboBoxAssigned.Items.Add("</------- " & Traduce.Txt("none") & " -------/>")
                ComboBoxAssigned.SelectedIndex = 0
                datos = null;
                datos = new Datos();
                if (datos.CargarArchivo(archivo, ComboBoxAssigned, ComboBoxMacro))
                {
                    if (report != null) // 'a vista normal
                    {
                        Fondo.Visible = True
                        Me.Controls.Remove(report)
                    }
                    Label1.Visible = True
                    RadioButton13.Visible = True
                    RadioButton14.Visible = True
                    If RadioButton13.Checked Then RadioButton14.Checked = True
                    Vista.Clear(True)
                    RadioButton13.Checked = True
                    this.Title = "Editor de perfiles - [" + archivo.Remove(0, archivo.LastIndexOf("\\") + 1) + "]";
                    esNuevo = false;
                }
                else
                {
                    Label1.Visible = False
                    RadioButton13.Visible = False
                    RadioButton14.Visible = False
                    ComboBoxMacro.Items.Clear()
                    ComboBoxAssigned.Items.Clear()
                    ComboBoxAssigned.Items.Add("</------- " & Traduce.Txt("none") & " -------/>")
                    ComboBoxAssigned.SelectedIndex = 0
                    datos = Nothing
                    GC.Collect()
                    datos = New Datos()
                    If parteJoy IsNot Nothing Then Fondo.Controls.Remove(parteJoy) : Me.Propiedades.Enabled = False
                    Vista.Clear(False)
                    Me.Text = Traduce.Txt("profile_editor") & " - [" & Traduce.Txt("untitled") & "]"
                    esNuevo = True
                }
            }
        }

        private bool Guardar()
        {
            if (Fondo.Controls.Count == 5)
            {
                if (esNuevo)
                    return GuardarComo();
                else
                    return datos.Guardar(ComboBoxMacro);
            }
        }

        private bool GuardarComo()
        {
            if (Fondo.Controls.Count == 5)
            {
                SaveFileDialog1.FileName = "nombre_perfil";
                if (SaveFileDialog1.ShowDialog() == Windows.Forms.DialogResult.OK)
                {
                    if (datos.Guardar(SaveFileDialog1.FileName, ComboBoxMacro))
                    {
                        esNuevo = false;
                        this.Title = "Editor de perfiles - [" + SaveFileDialog1.FileName.Remove(0, SaveFileDialog1.FileName.LastIndexOf("\\") + 1) + "]";
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        private void Lanzar()
        {
            if (datos.GetModificado())
            {
                MessageBoxResult r = MessageBox.Show("¿Guardar los cambios antes de lanzar el perfil?", "Advertencia", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (r == MessageBoxResult.Cancel)
                    return;
                else
                {
                    if (!Guardar())
                        return;
                }
            }
            if (Driver.Lanzar(datos.GetRutaPerfil()))
                MessageBox.Show("Perfil cargado correctamente", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
