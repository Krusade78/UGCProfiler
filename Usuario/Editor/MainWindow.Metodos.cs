﻿using System;
using System.Windows;

namespace Editor
{
    internal partial class MainWindow
    {
        private readonly CDatos datos = new CDatos();
        private string rutaPerfil = "";

        internal CDatos GetDatos()
        {
            return datos;
        }

        private void Nuevo()
        {
            if (datos.Modificado)
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
                gridVista.Children.Clear();
            }

            rtbEdicion.IsChecked = false;
            rtbListado.IsChecked = false;
            rtbEdicion.IsChecked = true;

            this.Title = "Editor de perfiles - [Sin nombre]";
            rutaPerfil = "";
        }

        private void Abrir(String archivo = null)
        {
            if (archivo == null)
            {
                if (datos.Modificado)
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
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Perfil (.xhp)|*.xhp"
                };
                if (dlg.ShowDialog(this) == true)
                    archivo = dlg.FileName;
            }
            if (archivo != null)
            {
                if (datos.Cargar(archivo))
                {
                    if (gridVista.Children.Count != 0)
                    {
                        gridVista.Children.Clear();
                    }

                    rtbEdicion.IsChecked = false;
                    rtbListado.IsChecked = false;
                    rtbEdicion.IsChecked = true;
                    cbPinkie.SelectedIndex = 0;
                    cbModo.SelectedIndex = 0;
                    rutaPerfil = archivo;
                    this.Title = "Editor de perfiles - [" + System.IO.Path.GetFileNameWithoutExtension(archivo) + "]";
                }
            }
        }

        private bool Guardar()
        {
            if (datos.Perfil.GENERAL.Rows.Count == 0)
                return true;
            else if (rutaPerfil == "")
                return GuardarComo();
            else
                return datos.Guardar(rutaPerfil);
        }

        private bool GuardarComo()
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Perfil (.xhp)|*.xhp",
                    FileName = "nombre_perfil"
                };
                if (dlg.ShowDialog() == true)
                {
                    if (datos.Guardar(dlg.FileName))
                    {
                        rutaPerfil = dlg.FileName;
                        this.Title = "Editor de perfiles - [" + System.IO.Path.GetFileNameWithoutExtension(dlg.FileName) + "]";
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
            if (datos.Modificado)
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

            using (System.IO.Pipes.NamedPipeClientStream pipeClient = new System.IO.Pipes.NamedPipeClientStream("LauncherPipe"))
            {
                try
                { 
                    pipeClient.Connect(1000);
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(pipeClient))
                    {
                        sw.Write(rutaPerfil);
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
                if (gridVista.Children[0] is CtlEditar ctlEditar)
                {
                    ctlEditar.ctlPropiedades.Refrescar();
                }
            }
        }

        public void GetModos(ref byte joy, ref byte pinkie, ref byte modo)
        {
            if (gridVista.Children[0] is CtlEditar ctlEditar)
            {
                joy = (byte)ctlEditar.GetTipoJoy();
            }
            pinkie = (byte)cbPinkie.SelectedIndex;
            modo = (byte)cbModo.SelectedIndex;
        }
    }
}
