using System;
using System.Windows;

namespace Editor
{
    internal partial class MainWindow
    {
        private CDatos datos = new CDatos();
        private String nombrePerfil = "";

        internal CDatos GetDatos()
        {
            return datos;
        }

        private void Nuevo()
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
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

            datos.Nuevo();
            if (gridVista.Children.Count != 0)
            {
                ((IDisposable)gridVista.Children[0]).Dispose();
                gridVista.Children.Clear();
            }

            rtbEdicion.IsChecked = false;
            rtbListado.IsChecked = false;
            rtbEdicion.IsChecked = true;

            this.Title = "Editor de perfiles - [Sin nombre]";
            nombrePerfil = "";
        }

        private void Abrir(String archivo = null)
        {
            if (archivo == null)
            {
                if (datos.Perfil.GENERAL.Rows.Count != 0)
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
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.Filter = "Perfil (.xhp)|*.xhp";
                if (dlg.ShowDialog(this) == true)
                    archivo = dlg.FileName;
            }
            if (archivo != null)
            {
                if (datos.Cargar(archivo))
                {
                    if (gridVista.Children.Count != 0)
                    {
                        ((IDisposable)gridVista.Children[0]).Dispose();
                        gridVista.Children.Clear();
                    }

                    rtbEdicion.IsChecked = false;
                    rtbListado.IsChecked = false;
                    rtbEdicion.IsChecked = true;
                    cbPinkie.SelectedIndex = 0;
                    cbModo.SelectedIndex = 0;
                    nombrePerfil = System.IO.Path.GetFileNameWithoutExtension(archivo);
                    this.Title = "Editor de perfiles - [" + nombrePerfil + "]";
                }
            }
        }

        private bool Guardar()
        {
            if (datos.Perfil.GENERAL.Rows.Count == 0)
                return true;
            else if (nombrePerfil == "")
                return GuardarComo();
            else
                return datos.Guardar(nombrePerfil + ".xhp");
        }

        private bool GuardarComo()
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.Filter = "Perfil (.xhp)|*.xhp";
                dlg.FileName = "nombre_perfil";
                if (dlg.ShowDialog() == true)
                {
                    if (datos.Guardar(dlg.FileName))
                    {
                        nombrePerfil = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                        this.Title = "Editor de perfiles - [" + nombrePerfil + "]";
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return true;
        }

        private void Lanzar()
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
            {
                MessageBoxResult r = MessageBox.Show("¿Quieres guardar los cambios antes de lanzar el perfil?", "Advertencia", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (r == MessageBoxResult.Cancel)
                    return;
                else if (r == MessageBoxResult.Yes)
                {
                    if (!Guardar())
                        return;
                }
            }
            else
                return;

            using (System.IO.Pipes.NamedPipeClientStream pipeClient = new System.IO.Pipes.NamedPipeClientStream("LauncherPipe"))
            {
                try
                { 
                    pipeClient.Connect(1000);
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(pipeClient))
                    {
                        sw.Write(nombrePerfil + ".xhp");
                        sw.Flush();
                    }
                }
                catch (Exception ex)
                {
                     MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show("Perfil cargado correctamente", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SetModos()
        {
            if (gridVista.Children.Count == 1)
            {
                if (gridVista.Children[0] is CtlEditar)
                    ((CtlEditar)gridVista.Children[0]).ctlPropiedades.Refrescar();
            }
        }

        public void GetModos(ref byte pinkie, ref byte modo)
        {
            pinkie = (byte)cbPinkie.SelectedIndex;
            modo = (byte)cbModo.SelectedIndex;
        }
    }
}
