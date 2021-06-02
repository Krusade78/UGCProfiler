using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using static Comunes.CTipos;

namespace Editor
{
    internal partial class VEditorMacros
    {
        private MainWindow padre;
        private readonly int indicep;
        private static readonly int ultimaPlantilla = -1;
        private readonly List<byte> teclas = new List<byte>();
        private readonly DataSetMacros dsMacros = new DataSetMacros();

        private void Iniciar()
        {
            CargarPlantillas();
            if (ComboBox1.Items.Count == 0)
            {
                ButtonAcepta.IsEnabled = false;
                return;
            }

            dsMacros.MACROS.DefaultView.Sort = "id";
            ListBox1.DataContext = dsMacros.MACROS.DefaultView;

            if (indicep > -1)
            {
                Comunes.DSPerfil.ACCIONESRow r = padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)indicep);
                TextBoxNombre.Text = r.Nombre;

                List<ushort> macro = new List<ushort>();
                foreach (ushort c in r.Comandos)
                {
                    macro.Add(c);
                }

                // Comprobar el check de enviar el nombre al mfd
                bool idc = false;
                if (macro.Count >= 2)
                {
                    if (macro[0] == ((byte)TipoComando.TipoComando_MfdTextoIni + (3 << 8)))
                        idc = true;
                }
                if (idc)
                {
                    bool nombreOk = true;
                    string st = r.Nombre;
                    if (st.Length > 16)
                    {
                        st = st.Substring(0, 16);
                    }
                    st = st.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                    byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));

                    if (macro.Count >= (st.Length + 2))
                    {
                        for (int i = 0; i < st.Length; i++)
                        {
                            ushort comando = (ushort)((byte)TipoComando.TipoComando_MfdTexto + (stb[i] << 8));
                            if (comando != macro[i + 1])
                            {
                                nombreOk = false;
                                break;
                            }
                        }
                        if ((macro[st.Length + 1] == (byte)TipoComando.TipoComando_MfdTextoFin) && nombreOk)
                        {
                            for (byte i = 0; i <= (st.Length + 1); i++)
                                macro.RemoveAt(0);

                            CheckBox1.IsChecked = true;
                        }
                    }
                }
                CargarLista(ref macro);
            }
            else
            {
                CheckBox1.IsChecked = true;
            }
            PasarABasico();
        }

        private void CargarLista(ref List<ushort> macros)
        {
            byte mtr = 0;

            List<ushort> bloque = new List<ushort>();
            for (int i = 0; i < macros.Count; i++)
            {
                byte tipo = (byte)(macros[i] & 0x7f);

                if (tipo == (byte)TipoComando.TipoComando_MfdTextoIni)
                {
                    bloque.Add(macros[i]);
                }
                else if (tipo  == (byte)TipoComando.TipoComando_MfdTexto)
                {
                    bloque.Add(macros[i]);
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdTextoFin)
                {
                    bloque.Add(macros[i]);
                    dsMacros.MACROS.AddMACROSRow(mtr++, "", bloque.ToArray());
                    bloque.Clear();

                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdHora)
                {
                    dsMacros.MACROS.AddMACROSRow(mtr++, "", new ushort[] { macros[i], macros[i + 1], macros[i + 2] });
                    i += 2;
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdHora24)
                {
                    dsMacros.MACROS.AddMACROSRow(mtr++, "", new ushort[] { macros[i], macros[i + 1], macros[i + 2] });
                    i += 2;
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdFecha)
                {
                    dsMacros.MACROS.AddMACROSRow(mtr++, "", new ushort[] { macros[i], macros[i + 1] });
                    i++;
                }
                else if (tipo == (byte)TipoComando.TipoComando_NxtLeds)
                {
                    dsMacros.MACROS.AddMACROSRow(mtr++, "", new ushort[] { macros[i], macros[i + 1], macros[i + 2], macros[i + 3] });
                    i += 3;
                }
                else
                {
                    dsMacros.MACROS.AddMACROSRow(mtr++, "", new ushort[] { macros[i] });
                }
            }
        }

        private void PasarABasico()
        {
            PanelNXT.Visibility = Visibility.Visible;
            PanelX52.Visibility = Visibility.Visible;
            PanelSetas.Visibility = Visibility.Visible;
            PanelRatonOff.Visibility = Visibility.Visible;
            PanelMovimiento.Visibility = Visibility.Visible;
            PanelEspecial.Visibility = Visibility.Visible;
            PanelModos.Visibility = Visibility.Visible;
            PanelPlantilla.Visibility = Visibility.Visible;
            PanelTecla.Visibility = Visibility.Visible;

            ButtonDXOff.IsEnabled = false;
            ButtonSubir.IsEnabled = false;
            ButtonBajar.IsEnabled = false;
        }

        private void PasarAAvanzado()
        {
            PanelNXT.Visibility = Visibility.Collapsed;
            PanelX52.Visibility = Visibility.Collapsed;
            PanelSetas.Visibility = Visibility.Collapsed;
            PanelRatonOff.Visibility = Visibility.Collapsed;
            PanelMovimiento.Visibility = Visibility.Collapsed;
            PanelEspecial.Visibility = Visibility.Collapsed;
            PanelModos.Visibility = Visibility.Collapsed;
            PanelPlantilla.Visibility = Visibility.Collapsed;
            PanelTecla.Visibility = Visibility.Collapsed;

            ButtonDXOff.IsEnabled = true;
            ButtonSubir.IsEnabled = true;
            ButtonBajar.IsEnabled = true;
        }

        #region "Plantillas"
        private void CargarPlantillas()
        {
            vtSelPlantilla.Items.Add(System.IO.Path.GetFileNameWithoutExtension("english-us"));
            vtSelPlantilla.Items.Add(System.IO.Path.GetFileNameWithoutExtension("español-es"));

            if (vtSelPlantilla.Items.Count > 0)
            {
                if (VEditorMacros.ultimaPlantilla != -1)
                {
                    vtSelPlantilla.SelectedIndex = VEditorMacros.ultimaPlantilla;
                    CargarPlantilla();
                }
                else
                {
                    int idx = 0;
                    switch ((System.Globalization.CultureInfo.CurrentCulture.LCID & 0xff))
                    {
                        case 0xc:
                            if (vtSelPlantilla.Items.IndexOf("french") != -1)
                                idx = vtSelPlantilla.Items.IndexOf("french");
                            break;
                        case 0x10:
                            if (vtSelPlantilla.Items.IndexOf("italian") != -1)
                                idx = vtSelPlantilla.Items.IndexOf("italian");
                            break;
                        case 0xa:
                            if (vtSelPlantilla.Items.IndexOf("español-es") != -1)
                                idx = vtSelPlantilla.Items.IndexOf("español-es");
                            break;
                        case 0x7:
                            if (vtSelPlantilla.Items.IndexOf("german") != -1)
                                idx = vtSelPlantilla.Items.IndexOf("german");
                            break;
                        default:
                            if (vtSelPlantilla.Items.IndexOf("english-us") != -1)
                                idx = vtSelPlantilla.Items.IndexOf("english-us");
                            break;
                    }
                    vtSelPlantilla.SelectedIndex = idx;
                    CargarPlantilla();
                }
            }
            else
            {
                ButtonPresionar.IsEnabled = false;
                ButtonSoltar.IsEnabled = false;
                ButtonNormal.IsEnabled = false;
                ButtonMantener.IsEnabled = false;
            }

        }

        private void CargarPlantilla()
        {
            ComboBox1.Items.Clear();
            dsMacros.TECLAS.Clear();
            using (System.IO.StreamReader f = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Editor.Plantillas." + vtSelPlantilla.SelectedItem + ".kbp")))
            {
                while (f.Peek() >= 0)
                {
                    string linea = f.ReadLine();
                    ComboBox1.Items.Add(linea);
                    dsMacros.TECLAS.AddTECLASRow(linea);
                }
                ComboBox1.SelectedIndex = 0;
            }
        }
        #endregion

        #region "Lista"
        private int GetCuenta()
        {
            int n = 0;
            foreach (System.Data.DataRowView rv in dsMacros.MACROS.DefaultView)
            {
                foreach (ushort c in ((DataSetMacros.MACROSRow)rv.Row).comando)
                {
                    n++;
                }
            }
            return n;
        }

        /// <summary>
        /// Se inserta por encima de la selección
        /// </summary>
        /// <returns>Devuelve la posición de inserción</returns>
        private byte GetIndice()
        {
            if (dsMacros.MACROS.DefaultView.Count == 0)
            {
                return 0;
            }
            if (ListBox1.SelectedIndex == -1) //al final
            {              
                return (byte)(((DataSetMacros.MACROSRow)dsMacros.MACROS.DefaultView[dsMacros.MACROS.DefaultView.Count - 1].Row).id + 1);
            }
            else
            {
                return ((DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row).id;
            }
        }

        private void Insertar(ushort[] bloque, bool separados)
        {
            byte idInicio = GetIndice();

            //hacer hueco
            for (int i = dsMacros.MACROS.Count - 1; i >= 0; i--)
            {
                System.Data.DataRowView rvBusca = dsMacros.MACROS.DefaultView[i];
                if (((DataSetMacros.MACROSRow)rvBusca.Row).id >= idInicio)
                {
                    if (separados)
                    {
                        ((DataSetMacros.MACROSRow)rvBusca.Row).id += (byte)bloque.Length;
                    }
                    else
                    {
                        ((DataSetMacros.MACROSRow)rvBusca.Row).id++;
                    }
                }
                else
                    break;
            }

            if (separados)
            {
                byte c = 0;
                foreach (ushort comando in bloque)
                {
                    dsMacros.MACROS.AddMACROSRow((byte)(idInicio + c++), "", new ushort[] { comando });
                }
            }
            else
            {
                dsMacros.MACROS.AddMACROSRow(idInicio, "", bloque);
            }
        }

        private void BorrarMacroLista()
        {
            if (ListBox1.SelectedIndex == -1)
                return;

            if (RadioButtonAvanzado.IsChecked != true)
            {
                if (RadioButtonBasico.IsChecked == true)
                {
                    dsMacros.MACROS.Clear();
                }
            }
            else
            {
                DataSetMacros.MACROSRow rsel = (DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row;
                byte tipo = (byte)(rsel.comando[0] & 0x7F);
                if (tipo == (byte)TipoComando.TipoComando_Repeat)
                {
                    if (((byte)(rsel.comando[0] & 0xff) & (byte)TipoComando.TipoComando_Soltar) == 0) //inicio repeat
                    {
                        foreach (DataSetMacros.MACROSRow rBusca in dsMacros.MACROS)
                        {
                            if ((rBusca.id > rsel.id) &&  
                                ((byte)(rBusca.comando[0] & 0x7F) == (byte)TipoComando.TipoComando_Repeat))
                            {
                                rBusca.Delete();
                                break;
                            }
                        }
                        rsel.Delete();
                    }
                    else //fin repeat
                    {
                        foreach (DataSetMacros.MACROSRow rBusca in dsMacros.MACROS)
                        {
                            if ((rBusca.id < rsel.id) && //hace falta porque en la tabla puede estar desordenado
                                ((byte)(rBusca.comando[0] & 0x7F) == (byte)TipoComando.TipoComando_Repeat))
                            {
                                rBusca.Delete();
                                break;
                            }
                        }
                        rsel.Delete();
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RepeatN)
                {
                    if ((byte)(rsel.comando[0] & 0xff & (byte)TipoComando.TipoComando_Soltar) == 0)
                    {
                        byte anidado = 1;
                        foreach (System.Data.DataRowView rvBusca in dsMacros.MACROS.DefaultView)
                        {
                            DataSetMacros.MACROSRow rBusca = (DataSetMacros.MACROSRow)rvBusca.Row;
                            if (rBusca.id > rsel.id)
                            {
                                if ((byte)(rBusca.comando[0] & 0x7F) == (byte)TipoComando.TipoComando_RepeatN)
                                {
                                    if ((rBusca.comando[0] & 0xff & (byte)TipoComando.TipoComando_Soltar) != 0)
                                        anidado--;
                                    else
                                        anidado++;
                                }
                                if ((anidado == 0) &&
                                        ((byte)(rBusca.comando[0] & 0x7F) == (byte)TipoComando.TipoComando_RepeatN) &&
                                        ((byte)(rBusca.comando[0] & (byte)TipoComando.TipoComando_Soltar) != 0)) //fin repeatn
                                {
                                    rBusca.Delete();
                                    break;
                                }
                            }
                        }
                        rsel.Delete();
                    }
                    else
                    {
                        byte anidado = 1;
                        DataSetMacros.MACROSRow rUltima = null;
                        for (int i = rsel.id - 1; i >= 0; i--)
                        {
                            System.Data.DataRowView rvBusca = dsMacros.MACROS.DefaultView[i];
                            DataSetMacros.MACROSRow rBusca = (DataSetMacros.MACROSRow)rvBusca.Row;
                            {
                                if ((byte)(rBusca.comando[0] & 0x7F) == (byte)TipoComando.TipoComando_RepeatN)
                                {
                                    if ((rBusca.comando[0] & 0xff & (byte)TipoComando.TipoComando_Soltar) != 0)
                                        anidado++;
                                    else
                                        anidado--;
                                }
                                if ((anidado == 0) &&
                                    ((byte)(rBusca.comando[0] & 0x7F) == (byte)TipoComando.TipoComando_RepeatN) &&
                                    ((rBusca.comando[0] & 0xff & (byte)TipoComando.TipoComando_Soltar) == 0))
                                {
                                    rUltima = rBusca;
                                    break;
                                }
                            }
                        }
                        rUltima.Delete();
                        rsel.Delete();
                    }
                }
                else
                {
                    rsel.Delete();
                }

                //reordenar
                byte id = 0;
                foreach (System.Data.DataRowView rvBusca in dsMacros.MACROS.DefaultView)
                {
                    DataSetMacros.MACROSRow rBusca = (DataSetMacros.MACROSRow)rvBusca.Row;
                    if (rBusca.id != id)
                    {
                        rBusca.id = id;
                    }
                    id++;
                }
            }
        }

        private void SubirMacroLista()
        {
            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == 0))
                return;

            DataSetMacros.MACROSRow rSel = (DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row;
            DataSetMacros.MACROSRow rAnterior = dsMacros.MACROS.FindByid((byte)(rSel.id - 1));
            byte tipoSel = (byte)(rSel.comando[0] & 0x7f);
            byte tipoAnterior = (byte)(rAnterior.comando[0] & 0x7f);

            if ((tipoSel  == (byte)TipoComando.TipoComando_Hold) && 
                (tipoAnterior == (byte)TipoComando.TipoComando_RepeatN) && ((byte)(rAnterior.comando[0] & (byte)TipoComando.TipoComando_Soltar) != 0)) //fin repeatn
            {
                return;
            }
            if ((tipoSel == (byte)TipoComando.TipoComando_Repeat) &&   //cualquier repeat
                ((tipoAnterior == (byte)TipoComando.TipoComando_RepeatN) || (tipoAnterior == (byte)TipoComando.TipoComando_Repeat)))
            {
                return;
            }
            if ((tipoSel == (byte)TipoComando.TipoComando_RepeatN) && //inicio repeat
                ((tipoAnterior == (byte)TipoComando.TipoComando_RepeatN) || (tipoAnterior == (byte)TipoComando.TipoComando_Repeat)))
            {
                return;
            }


            int sel = ListBox1.SelectedIndex;

            byte idAnterior = rAnterior.id;
            rAnterior.id = 255;
            rSel.id = idAnterior;
            rAnterior.id = (byte)(idAnterior + 1);

            ListBox1.SelectedIndex = sel - 1;
        }

        private void BajarMacroLista()
        {
            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == (ListBox1.Items.Count - 1)))
                return;

            DataSetMacros.MACROSRow rSel = (DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row;
            DataSetMacros.MACROSRow rSiguiente = dsMacros.MACROS.FindByid((byte)(rSel.id + 1));
            byte tipoSel = (byte)(rSel.comando[0] & 0x7f);
            byte tipoSiguiente = (byte)(rSiguiente.comando[0] & 0x7f);

            if ((tipoSel == (byte)TipoComando.TipoComando_Hold) && (tipoSiguiente == (byte)TipoComando.TipoComando_RepeatN))
            {
                return;
            }
            if ((tipoSel == (byte)TipoComando.TipoComando_Repeat) &&
                ((tipoSiguiente == (byte)TipoComando.TipoComando_RepeatN) || (tipoSiguiente == (byte)TipoComando.TipoComando_Repeat)))
            {
                return;
            }
            if ((tipoSel == (byte)TipoComando.TipoComando_RepeatN) &&
                ((tipoSiguiente == (byte)TipoComando.TipoComando_RepeatN) || (tipoSiguiente == (byte)TipoComando.TipoComando_Repeat)))
            {
                return;
            }

            int sel = ListBox1.SelectedIndex;

            byte idSiguiente = rSiguiente.id;
            rSiguiente.id = 255;
            rSel.id = idSiguiente;
            rSiguiente.id = (byte)(idSiguiente - 1);

            ListBox1.SelectedIndex = sel + 1;
        }
        #endregion

        private void Guardar()
        {
            if (TextBoxNombre.Text.Trim() == "")
                return;
            else
            {
                foreach (Comunes.DSPerfil.ACCIONESRow r in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if ((r.Nombre == TextBoxNombre.Text.Trim()) && (r.idAccion != indicep))
                    {
                        MessageBox.Show("El de nombre de la macro está repetido.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }
            }

            List<ushort> macro = new List<ushort>();

            if (CheckBox1.IsChecked == true) //texto x52
            {
                String st = TextBoxNombre.Text.Trim();
                if (st.Length > 16)
                {
                    st = st.Substring(0, 16);
                }
                st = st.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));

                macro.Add((byte)TipoComando.TipoComando_MfdTextoIni + (3 << 8));
                for (int i = 0; i < texto.Length; i++)
                {
                    macro.Add((ushort)((byte)TipoComando.TipoComando_MfdTexto + (texto[i] << 8)));
                }
                macro.Add((byte)TipoComando.TipoComando_MfdTextoFin);
            }

            foreach (System.Data.DataRowView rv in dsMacros.MACROS.DefaultView)
            {
                foreach (ushort c in ((DataSetMacros.MACROSRow)rv.Row).comando)
                {
                    macro.Add(c);
                }
            }

            if (indicep == -1)
            {
                ushort idnuevo = 0;
                foreach (Comunes.DSPerfil.ACCIONESRow r in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if (r.idAccion > idnuevo)
                        idnuevo = r.idAccion;
                }
                idnuevo++;

                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(idnuevo, TextBoxNombre.Text.Trim(), macro.ToArray());
                this.DialogResult = null;
            }
            else
            {
                Comunes.DSPerfil.ACCIONESRow nr = padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)indicep);
                nr.Nombre = TextBoxNombre.Text.Trim();
                nr.Comandos = macro.ToArray();
                this.DialogResult = true;
            }
        }

        #region "teclas"
        private void TeclasPulsar(bool mantener)
        {
            if (teclas.Count == 0)
                return;
            if ((GetCuenta() + ((mantener) ? 1 : 0) + (teclas.Count * 2)) > 237)
                return;

            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
            }

            List<ushort> bloque = new List<ushort>();
            for (byte i = 0; i < teclas.Count; i++)
            {
                int k = MapKey(teclas[i]);
                if (k > -1) bloque.Add((ushort)((byte)TipoComando.TipoComando_Tecla + (k << 8)));
            }
            if (mantener)
            {
                bloque.Add((ushort)TipoComando.TipoComando_Hold);
            }
            for (int j = teclas.Count - 1; j >= 0; j--)
            {
                int k = MapKey(teclas[j]);
                if (k > -1)
                {
                    bloque.Add((ushort)((byte)(TipoComando.TipoComando_Soltar | TipoComando.TipoComando_Tecla) + (k << 8)));
                }
            }
            Insertar(bloque.ToArray(), true);
        }

        private void LeerTeclado()
        {
            String s = "";
            bool[] buff = new bool[256];
            for (Key k = Key.Cancel; k <= Key.DeadCharProcessed; k++)
            {
                if (Keyboard.IsKeyDown(k))
                    buff[KeyInterop.VirtualKeyFromKey(k)] = true;
            }
            teclas.Clear();
            if (buff[0x10] && !buff[0xa0] && !buff[0xa1])
            {
                s += "May.";
                teclas.Add(0x10);
            }
            if (buff[0x11] && !buff[0xa2] && !buff[0xa3])
            {
                s += ((s == "") ? "" : " + ") + "Control" ;
                teclas.Add(0x11);
            }
            if (buff[0x12] && !buff[0xa4] && !buff[0xa5])
            {
                s += ((s == "") ? "" : " + ") + "Alt";
                teclas.Add(0x12);
            }
            if (buff[0xa0])
            {
                s += ((s == "") ? "" : " + ") + "May.I";
                teclas.Add(0xa0);
            }
            if (buff[0xa1])
            {
                s += ((s == "") ? "" : " + ") + "May.D";
                teclas.Add(0xa1);
            }
            if (buff[0xa2])
            {
                s += ((s == "") ? "" : " + ") + "ControlI";
                teclas.Add(0xa2);
            }
            if (buff[0xa3])
            {
                s += ((s == "") ? "" : " + ") + "ControlD";
                teclas.Add(0xa3);
            }
            if (buff[0xa4])
            {
                s += ((s == "") ? "" : " + ") + "AltI";
                teclas.Add(0xa4);
            }
            if (buff[0xa5])
            {
                s += ((s == "") ? "" : " + ") + "AltD";
                teclas.Add(0xa5);
            }
            if (buff[0x5b])
            {
                s += ((s == "") ? "" : " + ") + "WinI";
                teclas.Add(0x5b);
            }
            if (buff[0x5c])
            {
                s += ((s == "") ? "" : " + ") + "WinD";
                teclas.Add(0x5c);
            }
            for (ushort i = 1; i < 256; i++)
            {
                if (((i < 0x10) || (i > 0x12)) && (i != 0x5B) && (i != 0x5C) && ((i < 0xA0) || (i > 0xA5)))
                {
                    if (buff[i])
                    {
                        s += ((s == "") ? "" : " + ") + KeyInterop.KeyFromVirtualKey(i).ToString();
                        teclas.Add((byte)i);
                        ButtonNormal.Focus();
                    }
                }
            }
            TextBoxTecla.Text = s;
        }

        #region "Conversiones"
        private int MapKey(int vkey)
        {
            //especiales 0xE0
            switch(vkey)
            {
                case 1: //Left mouse button
                case 2: //Right mouse button
                    return -1;
                case 3: // crtl+break
                    return 0x47;
                case 4: //Middle mouse button (three-button mouse)
                case 5: //X1 mouse button
                case 6: //X2 mouse button
                case 7: //Undefined
                    return -1;
                case 8:
                    return 0x2a;
                case 9:
                    return 0x2b;
                case 0xa:
                case 0xb:
                    return -1;
                case 0xc: //CLEAR key
                    return 0x9c;
                case 0xD: // enter
                    return 0x28;
                case 0xe:
                case 0xf:
                    return -1;
                case 0x10: //SHIFT key
                    return 0xe1;
                case 0x11: // ctrl
                    return 0xE0;
                case 0x12: //alt
                    return 0xE2;
                case 0x13: //pausa
                    return 0x48;
                case 0x14: //CAPS LOCK key
                    return 0x39;
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1a:
                    return -1;
                case 0x1b: //escape
                    return 0x29;
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f:
                    return -1;
                case 0x20: //espacio
                    return 0x2c; 
                case 0x21: //pg up
                    return 0x4B;
                case 0x22: //pg down
                    return 0x4E;
                case 0x23: //end
                    return 0x4D;
                case 0x24: //home
                    return 0x4A;
                case 0x25: //left
                    return 0x50;
                case 0x26:  //up
                    return 0x52;
                case 0x27: // derecha
                    return 0x4F;
                case 0x28: //down
                    return 0x51;
                case 0x29: //select
                    return 0x77;
                case 0x2a: //print
                    return -1;
                case 0x2b: //execute
                    return 0x74;
                case 0x2C: //print screen
                    return 0x46;
                case 0x2D: //insert
                    return 0x49;
                case 0x2E:
                    return 0x4C; //delete;
                case 0x2f: //help
                    return 0x75;
                case 0x30: //0
                    return 0x27;
                case 0x31:
                    return 0x1e;
                case 0x32:
                    return 0x1f;
                case 0x33:
                    return 0x20;
                case 0x34:
                    return 0x21;
                case 0x35:
                    return 0x22;
                case 0x36:
                    return 0x23;
                case 0x37:
                    return 0x24;
                case 0x38:
                    return 0x25;
                case 0x39:
                    return 0x26;
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f:
                case 0x40:
                    return -1;
                case 0x41: //a
                    return 0x04;
                case 0x42:
                    return 0x05;
                case 0x43:
                    return 0x06;
                case 0x44:
                    return 0x07;
                case 0x45:
                    return 0x08;
                case 0x46:
                    return 0x09;
                case 0x47:
                    return 0x0a;
                case 0x48:
                    return 0x0b;
                case 0x49:
                    return 0x0c;
                case 0x4a:
                    return 0x0d;
                case 0x4b:
                    return 0x0e;
                case 0x4c:
                    return 0x0f;
                case 0x4d:
                    return 0x10;
                case 0x4e:
                    return 0x11;
                case 0x4f:
                    return 0x12;
                case 0x50:
                    return 0x13;
                case 0x51:
                    return 0x14;
                case 0x52:
                    return 0x15;
                case 0x53:
                    return 0x16;
                case 0x54:
                    return 0x17;
                case 0x55:
                    return 0x18;
                case 0x56:
                    return 0x19;
                case 0x57:
                    return 0x1a;
                case 0x58:
                    return 0x1b;
                case 0x59:
                    return 0x1c;
                case 0x5a:
                    return 0x1d;
                case 0x5B: //left win
                    return 0xE3;
                case 0x5C: //right win
                    return 0xE7;
                case 0x5D: //app
                    return 0x65;
                case 0x5E:
                    return -1;
                case 0x5f: //sleep
                    return -1;
                case 0x60: // kp 0
                    return 0x62;
                case 0x61: //kp 1
                    return 0x59;
                case 0x62: //kp 2
                    return 0x5A;
                case 0x63: //kp 3
                    return 0x5B;
                case 0x64: //hp 4
                    return 0x5C;
                case 0x66: //kp 6
                    return 0x5E;
                case 0x67: //kp 7
                    return 0x5F;
                case 0x68: //kp 8
                    return 0x60;
                case 0x69: //kp 9
                    return 0x62;
                case 0x6A: // kp *
                    return 0x55;
                case 0x6b: // kp +
                    return 0x57;
                case 0x6c: //separator
                    return 0x9f;
                case 0x6d: //kp -
                    return 0x56;
                case 0x6E: //kp.
                    return 0x63;
                case 0x6F: //kp/
                    return 0x54;
                case 0x70: // F1
                    return 0x3a;
                case 0x71:
                    return 0x3b;
                case 0x72:
                    return 0x3c;
                case 0x73:
                    return 0x3d;
                case 0x74:
                    return 0x3e;
                case 0x75:
                    return 0x3f;
                case 0x76:
                    return 0x40;
                case 0x77:
                    return 0x41;
                case 0x78:
                    return 0x42;
                case 0x79:
                    return 0x43;
                case 0x7a:
                    return 0x44;
                case 0x7b:
                    return 0x45;
                case 0x7c:
                    return 0x68;
                case 0x7d:
                    return 0x69;
                case 0x7e:
                    return 0x6a;
                case 0x7f:
                    return 0x6b;
                case 0x80:
                    return 0x6c;
                case 0x81:
                    return 0x6d;
                case 0x82:
                    return 0x6e;
                case 0x83:
                    return 0x6f;
                case 0x84:
                    return 0x70;
                case 0x85:
                    return 0x71;
                case 0x86:
                    return 0x72;
                case 0x87:
                    return 0x73;
                //0x88 - 0x8f
                case 0x90: //num lock
                    return 0x53;
                case 0x91: //scroll lock
                    return 0x47;
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                    return -1;
                //0x97 - 0x9f
                case 0xA0: // left shifht
                    return 0xE1;
                case 0xA1: // right shift
                    return 0xE5;
                case 0xA2: // left control
                    return 0xE0;
                case 0xA3: //right control
                    return 0xE4;
                case 0xA4: //left alt
                    return 0xE2;
                case 0xA5: //right alt
                    //return 0xE6;
                case 0xa6:
                    //return 0x72;
                case 0xa7:
                    //return 0x71;
                case 0xa8:
                    //return 0x6f;
                case 0xa9:
                    //return 0x70;
                case 0xaa:
                    //return 0x6d;
                case 0xab:
                    //return 0x6e;
                case 0xac:
                    return -1;
                case 0xad: //Volume Mute key
                    return 0x7f;
                case 0xae: //Volume Down key
                    return 0x81;
                case 0xaf: //Volume Up key
                    return 0x80;
                case 0xb0:
                case 0xb1:
                    return -1;
                case 0xb2: // stop media
                    return 0x78;
                case 0xb3:
                case 0xb4:
                case 0xb5:
                    return -1;
                case 0xb6:
                    return 0x73;
                case 0xb7:
                    return -1;
                //0xb8 - 0xb9
                case 0xba: //VK_OEM_1
                    return 0x2f;
                case 0xbb: //For any country/region, the '+' key
                    return 0x30;
                case 0xbc: //For any country/region, the ',' key
                    return 0x36;
                case 0xBD: //For any country/region, the '-' key
                    return 0x38;
                case 0xbe: //For any country/region, the '.' key
                    return 0x37;
                case 0xbf: //Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '/?' key
                    return 0x32;
                case 0xc0: //Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '`~' key
                    return 0x33;
                //0xc1 - 0xd7
                //0xd8 - 0xda
                case 0xdb: //Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '[{' key
                    return 0x2d;
                case 0xdc: //Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '\|' key
                    return 0x35;
                case 0xdd: //Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the ']}' key
                    return 0x2e;
                case 0xde: //Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the 'single-quote/double-quote' key
                    return 0x34;
                case 0xdf: //Used for miscellaneous characters; it can vary by keyboard.
                    return -1;
                //0xe0
                case 0xe1: //OEM specific
                    return -1;
                case 0xe2: //Either the angle bracket key or the backslash key on the RT 102-key keyboard
                    return 0x64 ;
                case 0xe3: //OEM specific
                case 0xe4: //OEM specific
                    return -1;
                case 0xe5: //IME PROCESS key
                    return -1;
                case 0xe6: //OEM specific
                    return -1;
                case 0xe7: //Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods
                    return -1;
                //0xe8
                case 0xe9: //OEM specific
                    return -1;
                case 0xea: 
                    return 0xe7;
                case 0xeb:
                case 0xec:
                case 0xed:
                case 0xee:
                case 0xef:
                case 0xf0:
                case 0xf1: //0xE3
                case 0xf2:
                case 0xf3:
                case 0xf4:
                case 0xf5:
                    return -1;
                case 0xf6: //Attn key
                    return 0x9a;
                case 0xf7: //CrSel key
                    return 0xa3;
                case 0xf8: //ExSel key
                    return 0xa4;
                case 0xF9: //Erase EOF key
                    return 0x6a; //0x65
                case 0xfa: //Play key
                case 0xfb: //Zoom key
                    return -1;
                //0xfc
                case 0xfd: //PA1 key
                    return -1;
                case 0xfe: //Clear key
                    return 0x9c;
                default:
                    return -1;
            }
        }
        #endregion
        #endregion

        #region "comandos estado"
        private void Mantener()
        {
            if (GetCuenta() > 237)
                return;

            if (dsMacros.MACROS.DefaultView.Count > 0)
            {
                if (!ComprobarManternerConRepetir()) return;
            }
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_Hold }, false);
        }
        private void Repetir()
        {
            if (GetCuenta() > 236)
                return;

            if (dsMacros.MACROS.DefaultView.Count > 0)
            {
                if (!ComprobarManternerConRepetir()) return;
            }
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_Repeat, (byte)TipoComando.TipoComando_Repeat | (byte)TipoComando.TipoComando_Soltar }, true);
        }

        private bool ComprobarManternerConRepetir()
        {
            foreach (DataSetMacros.MACROSRow r in dsMacros.MACROS)
            {
                byte tipo = (byte)(r.comando[0] & 0x7f);
                if ((tipo == (byte)TipoComando.TipoComando_Repeat) || (tipo == (byte)TipoComando.TipoComando_Hold))
                {
                    MessageBox.Show("Repetir y Mantener deben ser únicos", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }

            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == 0))
            {
                return true;
            }

            int reps = 0;
            foreach (System.Data.DataRowView rv in dsMacros.MACROS.DefaultView)
            {
                if (rv == (System.Data.DataRowView)ListBox1.SelectedItem)
                {
                    break;
                }
                byte tipo = (byte)(((DataSetMacros.MACROSRow)rv.Row).comando[0] & 0xff);
                if ((TipoComando)(tipo & 0x7f) == TipoComando.TipoComando_RepeatN)
                {
                    if ((tipo & (byte)TipoComando.TipoComando_Soltar) != 0)
                    reps--;
                else
                    reps++;
                }
            }
            if (reps != 0)
            {
                MessageBox.Show("Repetir y Mantener no pueden estar dentro de Repetir N.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }
        #endregion

        #region "MFD"
        private void Linea()
        {
            if ((GetCuenta() + 2 + TextBox3.Text.Length) > 237)
                return;

            List<ushort> bloque = new List<ushort>
            {
                (ushort)((byte)TipoComando.TipoComando_MfdTextoIni + ((ushort)NumericUpDown9.Valor << 8))
            };
            String st = TextBox3.Text.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
            byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));
            for (byte i = 0; i < st.Length; i++)
            {
                bloque.Add((ushort)((byte)TipoComando.TipoComando_MfdTexto + (stb[i] << 8)));
            }
            bloque.Add((byte)TipoComando.TipoComando_MfdTextoFin);
            Insertar(bloque.ToArray(), false);
        }

        private void Hora(bool f24h)
        {
            if (GetCuenta() > 235)
                return;
            if ((NumericUpDown10.Valor < 0) && (NumericUpDown7.Valor == 1))
            {
                MessageBox.Show("El reloj 1 no puede tener horas negativas.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ushort[] bloque = new ushort[3];
            TipoComando tipo = (f24h) ? TipoComando.TipoComando_MfdHora24 : TipoComando.TipoComando_MfdHora;
            bloque[0] =(ushort)((byte)tipo + ((ushort)NumericUpDown7.Valor << 8));
            if (NumericUpDown7.Valor == 1)
            {
                bloque[1] = (ushort)((byte)tipo + ((ushort)NumericUpDown10.Valor << 8));
                bloque[2] = (ushort)((byte)tipo + ((ushort)NumericUpDown11.Valor << 8));
            }
            else
            {
                int minutos = (int)((NumericUpDown10.Valor * 60) + NumericUpDown11.Valor);
                if (minutos < 0)
                {
                    bloque[1] = (ushort)((byte)tipo + ((((ushort)-minutos >> 8) + 4) << 8) );
                    bloque[2] = (ushort)((byte)tipo + (((ushort)-minutos & 0xff) << 8) );
                }
                else
                {
                    bloque[1] = (ushort)((byte)tipo + ((((ushort)minutos >> 8)) << 8));
                    bloque[2] = (ushort)((byte)tipo + (((ushort)minutos & 0xff) << 8));
                }
            }
            Insertar(bloque, false);
        }

        private void Fecha(ushort f)
        {
            if (GetCuenta() > 236) return;
            ushort[] bloque = new ushort[2];
            bloque[0] = (ushort)((byte)TipoComando.TipoComando_MfdFecha + (f << 8));
            bloque[1] = (ushort)((byte)TipoComando.TipoComando_MfdFecha + ((ushort)NumericUpDown13.Valor << 8));
            Insertar(bloque, false);
        }
        #endregion

        #region "Gladiator NXT"
        private void Leds(byte nLed, CEnums.OrdenLed orden, CEnums.ModoColor mColor, string scolor1, string scolor2)
        {
            byte[] cmds = new byte[]{ 0, 0, 0, 0 };
            ushort color1 = (ushort)(byte.Parse(scolor1.Split(';')[0]) << 8);
            color1 |= (ushort)(byte.Parse(scolor1.Split(';')[1]) << 4);
            color1 |= byte.Parse(scolor1.Split(';')[2]);
            ushort color2 = (ushort)(byte.Parse(scolor2.Split(';')[0]) << 8);
            color2 |= (ushort)(byte.Parse(scolor2.Split(';')[1]) << 4);
            color2 |= byte.Parse(scolor2.Split(';')[2]);

            cmds[0] = nLed; //base 0, joy1 10, joy2 11

            cmds[1] |= (byte)(color1 >> 8);//r;
            cmds[1] |= (byte)((color1 & 0x70) >> 1); //g
            byte color = (byte)(color1 & 0x07); //b
            cmds[1] |= (byte)((color & 0x3) << 6);
            cmds[2] |= (byte)(color >> 2);

            cmds[2] |= (byte)(color2 >> 7); //r
            cmds[2] |= (byte)(color2 & 0x70); //g
            color = (byte)(color2 & 0x07); //b
            cmds[2] |= (byte)((color & 1) << 7);
            cmds[3] |= (byte)(color >> 1);

            cmds[3] = (byte)((byte)orden << 2);
            cmds[3] |= (byte)((byte)mColor << 5);

            ushort[] bloque = new ushort[4];
            TipoComando tipo = TipoComando.TipoComando_NxtLeds;
            bloque[0] = (ushort)((byte)tipo + (cmds[0] << 8));
            bloque[1] = (ushort)((byte)tipo + (cmds[1] << 8));
            bloque[2] = (ushort)((byte)tipo + (cmds[2] << 8));
            bloque[3] = (ushort)((byte)tipo + (cmds[3] << 8));
            Insertar(bloque, false);
        }
        #endregion
    }
}
