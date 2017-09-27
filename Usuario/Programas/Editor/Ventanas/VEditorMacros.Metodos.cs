using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using static Editor.CEnums;

namespace Editor
{
    internal partial class VEditorMacros
    {
        private MainWindow padre;
        private int indicep;
        private static int ultimaPlantilla = 0;
        private List<ushort> macro = new List<ushort>();
        private List<byte> teclas = new List<byte>();

        private void Iniciar()
        {
            CargarPlantillas();
            if (ComboBox1.Items.Count == 0)
            {
                ButtonAcepta.IsEnabled = false;
                return;
            }

            if (indicep > -1)
            {
                DSPerfil.ACCIONESRow r = padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)indicep);
                TextBoxNombre.Text = r.Nombre;
                foreach (ushort c in r.Comandos)
                    macro.Add(c);

                // Comprobar el check de enviar el nombre al mfd
                bool idc = false;
                if (macro.Count >= 2)
                {
                    if (macro[0] == ((byte)TipoC.TipoComando_MfdTexto + (3 << 8)))
                        idc = true;
                }
                if (idc)
                {
                    bool nombreOk = true;
                    string st = r.Nombre;
                    if (st.Length > 16)
                        st = st.Substring(0, 16);
                    byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(st));

                    if (macro.Count >= (st.Length + 2))
                    {
                        for (int i = 0; i < st.Length; i++)
                        {
                            ushort comando = (ushort)((byte)TipoC.TipoComando_MfdTexto + (stb[i] << 8));
                            if (comando != macro[i + 1])
                            {
                                nombreOk = false;
                                break;
                            }
                        }
                        if ((macro[st.Length + 1] == (byte)TipoC.TipoComando_MfdTextoFin) && nombreOk)
                        {
                            for (byte i = 0; i <= st.Length + 1; i++)
                                macro.RemoveAt(0);

                            CheckBox1.IsChecked = true;
                        }
                    }
                }
                CargarLista();
            }
            else
            {
                CheckBox1.IsChecked = true;
            }
            PasarABasico();
        }

        private void PasarABasico()
        {
            PanelX52.Visibility = System.Windows.Visibility.Visible;
            PanelSetas.Visibility = System.Windows.Visibility.Visible;
            PanelRatonOff.Visibility = System.Windows.Visibility.Visible;
            PanelMovimiento.Visibility = System.Windows.Visibility.Visible;
            PanelEspecial.Visibility = System.Windows.Visibility.Visible;
            PanelModos.Visibility = System.Windows.Visibility.Visible;
            PanelPlantilla.Visibility = System.Windows.Visibility.Visible;
            PanelTecla.Visibility = System.Windows.Visibility.Visible;

            ButtonDXOff.IsEnabled = false;
            ButtonSubir.IsEnabled = false;
            ButtonBajar.IsEnabled = false;
        }

        private void PasarAAvanzado()
        {
            PanelX52.Visibility = System.Windows.Visibility.Collapsed;
            PanelSetas.Visibility = System.Windows.Visibility.Collapsed;
            PanelRatonOff.Visibility = System.Windows.Visibility.Collapsed;
            PanelMovimiento.Visibility = System.Windows.Visibility.Collapsed;
            PanelEspecial.Visibility = System.Windows.Visibility.Collapsed;
            PanelModos.Visibility = System.Windows.Visibility.Collapsed;
            PanelPlantilla.Visibility = System.Windows.Visibility.Collapsed;
            PanelTecla.Visibility = System.Windows.Visibility.Collapsed;

            ButtonDXOff.IsEnabled = true;
            ButtonSubir.IsEnabled = true;
            ButtonBajar.IsEnabled = true;
        }

        #region "Plantillas"
        private void CargarPlantillas()
        {
            System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(".");
            foreach (System.IO.FileInfo f in d.GetFiles("*.kbp"))
            {
                vtSelPlantilla.Items.Add(System.IO.Path.GetFileNameWithoutExtension(f.Name));
            }
            if (vtSelPlantilla.Items.Count > 0)
            {
                if (vtSelPlantilla.Items.Count > VEditorMacros.ultimaPlantilla)
                {
                    vtSelPlantilla.SelectedIndex = VEditorMacros.ultimaPlantilla;
                    CargarPlantilla(VEditorMacros.ultimaPlantilla);
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
                            if (vtSelPlantilla.Items.IndexOf("spanish-es") != -1)
                                idx = vtSelPlantilla.Items.IndexOf("spanish-es");
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
                    CargarPlantilla(idx);
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

        private void CargarPlantilla(int idx)
        {
            ComboBox1.Items.Clear();
            using (System.IO.StreamReader f = new System.IO.StreamReader(vtSelPlantilla.SelectedItem + ".kbp"))
            {
                while (f.Peek() >= 0)
                {
                    ComboBox1.Items.Add(f.ReadLine());
                }
                ComboBox1.SelectedIndex = 0;
            }
        }
        #endregion

        #region "Lista"
        private void CargarLista()
        {
            bool soltar;
            byte tipo;
            byte dato;
            ListBox1.Items.Clear();
            for (int i = 0; i < macro.Count; i++)
            {
                tipo = (byte)(macro[i] & 0xff);
                dato = (byte)(macro[i] >> 8);
                if (tipo >= 32)
                {
                    soltar = true;
                    tipo = (byte)(tipo - 32);
                }
                else
                    soltar = false;
                switch ((TipoC)tipo)
                {
                    case TipoC.TipoComando_Tecla:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Soltar " + Convert.ToString(ComboBox1.Items[dato]).Remove(0, Convert.ToString(ComboBox1.Items[dato]).IndexOf(" ")));
                        }
                        else
                        {
                            ListBox1.Items.Add("Presionar " + Convert.ToString(ComboBox1.Items[dato]).Remove(0, Convert.ToString(ComboBox1.Items[dato]).IndexOf(" ")));
                        }
                        break;
                    case TipoC.TipoComando_RatonBt1:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Botón 1 Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Botón 1 On");
                        }
                        break;
                    case TipoC.TipoComando_RatonBt2:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Botón 2 Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Botón 2 On");
                        }
                        break;
                    case TipoC.TipoComando_RatonBt3:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Botón 3 Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Botón 3 On");
                        }
                        break;
                    case TipoC.TipoComando_RatonIzq:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Izquierda 0");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Izquierda " + dato);
                        }
                        break;
                    case TipoC.TipoComando_RatonDer:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Derecha 0");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Derecha " + dato);
                        }
                        break;
                    case TipoC.TipoComando_RatonAba:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Abajo 0");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Abajo " + dato);
                        }
                        break;
                    case TipoC.TipoComando_RatonArr:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Arriba 0");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Arriba " + dato);
                        }
                        break;
                    case TipoC.TipoComando_RatonWhArr:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Rueda arriba Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Rueda arriba On");
                        }
                        break;
                    case TipoC.TipoComando_RatonWhAba:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Ratón->Rueba abajo Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Ratón->Rueda abajo On");
                        }
                        break;
                    case TipoC.TipoComando_Delay:
                        ListBox1.Items.Add("Pausa " + dato);
                        break;
                    case TipoC.TipoComando_Hold:
                        ListBox1.Items.Add("Mantener");
                        break;
                    case TipoC.TipoComando_Repeat:
                        if (soltar)
                            ListBox1.Items.Add("/-- Repetir Fin");
                        else
                            ListBox1.Items.Add("/-- Repetir Inicio");
                        break;
                    case TipoC.TipoComando_RepeatN:
                        if (soltar)
                            ListBox1.Items.Add("/-- Repetir N Fin");
                        else
                            ListBox1.Items.Add("/-- Repetir N[" + dato + "] Inicio");
                        break;
                    case TipoC.TipoComando_Modo:
                        ListBox1.Items.Add("Modo " + (dato + 1));
                        break;
                    case TipoC.TipoComando_Pinkie:
                        if (dato == 0)
                            ListBox1.Items.Add("Pinkie Off");
                        else
                            ListBox1.Items.Add("Pinkie On");
                        break;
                    case TipoC.TipoComando_DxBoton:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Botón DX " + (dato + 1) + " Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Botón DX " + (dato + 1) + " On");
                        }
                        break;
                    case TipoC.TipoComando_DxSeta:
                        if (soltar)
                        {
                            ListBox1.Items.Add("Seta DX " + ((dato / 8) + 1) + ((dato % 8) + 1) + " Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Seta DX " + ((dato / 8) + 1) + ((dato % 8) + 1) + " On");
                        }
                        break;
                    case TipoC.TipoComando_MfdLuz:
                        ListBox1.Items.Add("Luz MFD " + dato);
                        break;
                    case TipoC.TipoComando_Luz:
                        ListBox1.Items.Add("Luz Botones " + dato);
                        break;
                    case TipoC.TipoComando_InfoLuz:
                        if (dato == 0)
                            ListBox1.Items.Add("Luz Info Off");
                        else
                            ListBox1.Items.Add("Luz Info On");
                        break;
                    case TipoC.TipoComando_MfdPinkie:
                        if (dato == 0)
                            ListBox1.Items.Add("MFD Pinkie Off");
                        else
                            ListBox1.Items.Add("MFD Pinkie On");
                        break;
                    case TipoC.TipoComando_MfdTexto:
                        string texto = "Línea de texto " + dato;
                        byte[] ascii = new byte[16];
                        byte j = 0;
                        while (macro[i + 1] != (ushort)TipoC.TipoComando_MfdTextoFin)
                        {
                            i++;
                            ascii[j] = (byte)(macro[i] >> 8);
                            j++;
                        }
                        i++;
                        texto += "  " + System.Text.Encoding.Convert(System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode, ascii);
                        texto = texto.Replace('ø', 'ñ').Replace('Ó', 'á').Replace('ß', 'í').Replace('Ô', 'ó').Replace('Ò', 'ú').Replace('£', 'Ñ').Replace('Ø', 'ª').Replace('×', 'º').Replace('ƒ', '¿').Replace('Ú', '¡');
                        ListBox1.Items.Add(texto);
                        break;
                    case TipoC.TipoComando_MfdHora:
                        if (dato == 1)
                        {
                            ListBox1.Items.Add("MFD Hora " + dato + "(AM/PM)" + (macro[i + 1] >> 8) + ":" + (macro[i + 2] >> 8));
                        }
                        else
                        {
                            ListBox1.Items.Add("MFD Hora " + dato + "(AM/PM)" + ((((macro[i + 1] >> 8) * 256) + (macro[i + 2] >> 8)) / 60) + ":" + ((((macro[i + 1] >> 8) * 256) + (macro[i + 2] >> 8)) % 60));
                        }
                        i += 2;
                        break;
                    case TipoC.TipoComando_MfdHora24:
                        if (dato == 1)
                        {
                            ListBox1.Items.Add("MFD Hora " + dato + "(24H)" + (macro[i + 1] >> 8) + ":" + (macro[i + 2] >> 8));
                        }
                        else
                        {
                            ListBox1.Items.Add("MFD Hora " + dato + "(24H)" + ((((macro[i + 1] >> 8) * 256) + (macro[i + 2] >> 8)) / 60) + ":" + ((((macro[i + 1] >> 8) * 256) + (macro[i + 2] >> 8)) % 60));
                        }
                        i = i + 2;
                        break;
                    case TipoC.TipoComando_MfdFecha:
                        ListBox1.Items.Add("MFD Fecha " + dato + " " + (macro[i + 1] >> 8));
                        i = i + 1;
                        break;
                }
            }

        }

        private int GetIndice()
        {
            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == (ListBox1.Items.Count - 1)))
                return macro.Count;
            else
            {
                int real = 0;
                int ficticio = 0;
                TipoC tipo;
                for (real = 0; real < macro.Count; real++)
                {
                    if (ficticio == ListBox1.SelectedIndex)
                        break;
                    tipo = (TipoC)(macro[real] & 0xFF);
                    if (tipo > (TipoC)31) tipo -= 32; //quitar el soltar
                    if (tipo < TipoC.TipoComando_MfdTexto)
                        ficticio++;
                    else if (tipo == TipoC.TipoComando_MfdTexto)
                    {
                        if ((TipoC)macro[real] == TipoC.TipoComando_MfdTextoFin)
                            ficticio++;
                    }
                    else if (tipo < TipoC.TipoComando_MfdFecha)
                    {
                        ficticio++;
                        real += 2;
                    }
                    else
                    {
                        ficticio++;
                        real++;
                    }
                }
                return real;
            }
        }

        private void BorrarMacroLista(int idx, bool noRepeats)
        {
            if (idx == -1)
                return;

            if (RadioButtonAvanzado.IsChecked != true)
            {
                if (RadioButtonBasico.IsChecked == true)
                {
                    macro.Clear();
                    ListBox1.Items.Clear();
                }
            }
            else
            {
                int idc = GetIndice();
                if (ListBox1.SelectedIndex == (ListBox1.Items.Count - 1))
                {
                    idc--;
                    if ((TipoC)macro[idc] == TipoC.TipoComando_MfdTextoFin)
                    {
                        int i;
                        for (i = idc - 1; i >= 0; i--)
                        {
                            if ((TipoC)(macro[i] & 0xFF) != TipoC.TipoComando_MfdTexto)
                            {
                                idc = i + 1;
                                break;
                            }
                        }
                        if (i == -1)
                            idc = 0;
                        else if ((idc - 1) > -1)
                        {
                            if ((TipoC)(macro[idc - 1] & 0xFF) == TipoC.TipoComando_MfdFecha)
                                idc--;
                            else if ((idc - 2) > -1)
                            {
                                if (((TipoC)(macro[idc - 2] & 0xFF) == TipoC.TipoComando_MfdHora24) || ((TipoC)(macro[idc - 2] & 0xFF) == TipoC.TipoComando_MfdHora))
                                    idc -= 2;
                            }
                        }
                    }
                    TipoC tipo = (TipoC)(macro[idc] & 0xFF);
                    if (!noRepeats)
                    {
                        switch (tipo)
                        {
                            case TipoC.TipoComando_Repeat:
                                for (int i = idc; i < macro.Count; i++)
                                {
                                    if ((TipoC)macro[i] == TipoC.TipoComando_RepeatFin)
                                    {
                                        macro.RemoveAt(i);
                                        break;
                                    }
                                }
                                break;
                            case TipoC.TipoComando_RepeatN:
                                for (int i = idc; i < macro.Count; i++)
                                {
                                    if ((TipoC)macro[i] == TipoC.TipoComando_RepeatNFin)
                                    {
                                        macro.RemoveAt(i);
                                        break;
                                    }
                                }
                                break;
                            case TipoC.TipoComando_RepeatFin:
                                macro.RemoveAt(idc);
                                for (int i = idc - 1; i >= 0; i--)
                                {
                                    if ((TipoC)macro[i] == TipoC.TipoComando_Repeat)
                                    {
                                        macro.RemoveAt(i);
                                        break;
                                    }
                                }
                                CargarLista();
                                return;
                            case TipoC.TipoComando_RepeatNFin:
                                macro.RemoveAt(idc);
                                for (int i = idc - 1; i >= 0; i--)
                                {
                                    if ((TipoC)macro[i] == TipoC.TipoComando_RepeatN)
                                    {
                                        macro.RemoveAt(i);
                                        break;
                                    }
                                }
                                CargarLista();
                                return;
                        }
                    }
                    if (tipo > (TipoC)31) tipo -= 32;
                    if (tipo < TipoC.TipoComando_MfdTexto)
                        macro.RemoveAt(idc);
                    else if (tipo == TipoC.TipoComando_MfdTexto)
                    {
                        macro.RemoveAt(idc);
                        for (byte i = 0; i <= 16; i++)
                        {
                            if ((TipoC)macro[idc] == TipoC.TipoComando_MfdTextoFin)
                                break;

                            macro.RemoveAt(idc);
                        }
                        macro.RemoveAt(idc);
                    }
                    else if (tipo < TipoC.TipoComando_MfdFecha)
                    {
                        macro.RemoveAt(idc);
                        macro.RemoveAt(idc);
                        macro.RemoveAt(idc);
                    }
                    else
                    {
                        macro.RemoveAt(idc);
                        macro.RemoveAt(idc);
                    }
                    CargarLista();
                }
                else
                {
                    macro.Clear();
                    ListBox1.Items.Clear();
                }
            }
        }

        private void SubirMacroLista(int idx)
        {
            if ((idx == -1) || (ListBox1.SelectedIndex == 0))
                return;

            int idc1 = GetIndice();
            int sel = idx;
            if (idx == (ListBox1.Items.Count - 1))
            {
                idc1--;
                if ((TipoC)macro[idc1] == TipoC.TipoComando_MfdTextoFin)
                {
                    for (int i = idc1 - 1; i >= 0; i--)
                    {
                        if ((TipoC)(macro[i] & 0xFF) != TipoC.TipoComando_MfdTexto)
                        {
                            idc1 = i + 1;
                            break;
                        }
                    }
                }
                else if ((idc1 - 1) > -1)
                {
                    if ((TipoC)(macro[idc1 - 1] & 0xFF) == TipoC.TipoComando_MfdFecha)
                        idc1--;
                    else if ((idc1 - 2) > -1)
                    {
                        if (((TipoC)(macro[idc1 - 2] & 0xFF) == TipoC.TipoComando_MfdHora24) || ((TipoC)(macro[idc1 - 2] & 0xFF) == TipoC.TipoComando_MfdHora))
                            idc1 -= 2;
                    }
                }
            }
            ListBox1.SelectedIndex = sel - 1;
            int idc2 = GetIndice();
            if ((((TipoC)macro[idc1] == TipoC.TipoComando_Hold) && ((TipoC)macro[idc2] == TipoC.TipoComando_RepeatNFin)) || (((TipoC)macro[idc1] == TipoC.TipoComando_Repeat) && ((TipoC)macro[idc2] == TipoC.TipoComando_RepeatNFin)) ||
                 (((TipoC)macro[idc1] == TipoC.TipoComando_RepeatN) && ((TipoC)macro[idc2] == TipoC.TipoComando_Hold)) || (((TipoC)macro[idc1] == TipoC.TipoComando_RepeatN) && ((TipoC)macro[idc2] == TipoC.TipoComando_Repeat)))
                return;

            if ((TipoC)(macro[idc1] & 31) < TipoC.TipoComando_MfdTexto)
                macro.Insert(idc2, macro[idc1]);
            else if ((TipoC)(macro[idc1] & 31) == TipoC.TipoComando_MfdTexto)
            {
                for (byte i = 0; i <= 17; i++)
                {
                    macro.Insert(idc2 + i, macro[idc1 + (i * 2)]);
                    if ((TipoC)macro[idc2 + i] == TipoC.TipoComando_MfdTextoFin) break;
                }
            }
            else if ((TipoC)(macro[idc1] & 31) < TipoC.TipoComando_MfdFecha)
            {
                macro.Insert(idc2, macro[idc1]);
                macro.Insert(idc2 + 1, macro[idc1 + 2]);
                macro.Insert(idc2 + 2, macro[idc1 + 4]);
            }
            else
            {
                macro.Insert(idc2, macro[idc1]);
                macro.Insert(idc2 + 1, macro[idc1 + 2]);
            }
            CargarLista();
            ListBox1.SelectedIndex = sel + 1;
            BorrarMacroLista(sel + 1, true);
        }

        private void BajarMacroLista(int idx)
        {
            if ((idx > -1) && (idx < ListBox1.Items.Count - 1))
            {
                ListBox1.SelectedIndex = ListBox1.SelectedIndex + 1;
                SubirMacroLista(ListBox1.SelectedIndex);
            }
        }
        #endregion

        private void Guardar()
        {
            if (TextBoxNombre.Text.Trim() == "")
                return;
            else
            {
                foreach (DSPerfil.ACCIONESRow r in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if ((r.Nombre == TextBoxNombre.Text.Trim()) && (r.idAccion != indicep))
                    {
                        MessageBox.Show("El de nombre de la macro está repetido.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }
            }

            if (CheckBox1.IsChecked == true) //texto x52
            {
                String st = TextBoxNombre.Text.Trim();
                if (st.Length > 16)
                    st = st.Substring(0, 16);
                st = st.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(st));
                macro.Insert(0, (byte)TipoC.TipoComando_MfdTextoFin);
                for (int i = (texto.Length - 1); i >= 0; i--)
                    macro.Insert(0, (ushort)((byte)TipoC.TipoComando_MfdTexto + (texto[i] << 8)));
                macro.Insert(0, (byte)TipoC.TipoComando_MfdTexto + (3 << 8));
            }

            if (indicep == -1)
            {
                ushort idnuevo = 0;
                foreach (DSPerfil.ACCIONESRow r in padre.GetDatos().Perfil.ACCIONES.Rows)
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
                DSPerfil.ACCIONESRow nr = padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)indicep);
                nr.Nombre = TextBoxNombre.Text.Trim();
                nr.Comandos = macro.ToArray();
                this.DialogResult = true;
            }

            this.Close();
        }

        #region "teclas"
        private void TeclasPulsar(bool mantener)
        {
            if (teclas.Count == 0)
                return;
            if ((macro.Count + ((mantener) ? 1 : 0) + (teclas.Count * 2)) > 237)
                return;

            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
            }

            int idc = GetIndice();
            byte i;
            for (i = 0; i < teclas.Count; i++)
            {
                int k = MapKey(teclas[i]);
                if (k > -1) macro.Insert(idc + i, (ushort)(k << 8));
            }
            if (mantener)
                macro.Insert(idc + i, (ushort)TipoC.TipoComando_Hold);
            for (int j = teclas.Count - 1; j >= 0; j--)
            {
                int k = MapKey(teclas[j]);
                if (k > -1)
                {
                    macro.Insert(idc + i, (ushort)(32 + (k << 8)));
                    i++;
                }
            }
            CargarLista();
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
            for (ushort i = 0; i < 256; i++)
            {
                if (((i < 0x10) || (i > 0x12)) && (i != 0x5B) && (i != 0x5C) && ((i < 0xA0) || (i > 0xA5)))
                {
                    if (buff[i])
                    {
                        s += ((s == "") ? "" : " + ") + KeyInterop.KeyFromVirtualKey(i).ToString();
                        teclas.Add((byte)i);
                        ButtonNormal.Focus();
                        break;
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
            if (macro.Count > 237)
                return;

            int idc = GetIndice();
            int reps = 0;
            for (int i = 0; i <= (idc - 1); i++)
            {
                if (((TipoC)macro[i] == TipoC.TipoComando_Repeat) || ((TipoC)macro[i] == TipoC.TipoComando_Hold))
                {
                    MessageBox.Show("Mantener y Repetir no pueden producirse a la vez", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if ((TipoC)macro[i] == TipoC.TipoComando_RepeatN)
                    reps++;
                if ((TipoC)macro[i] == TipoC.TipoComando_RepeatNFin)
                    reps--;
            }
            if (reps == 0)
            {
                macro.Insert(idc, (byte)TipoC.TipoComando_Hold);
                CargarLista();
            }
            else
                MessageBox.Show("Mantener no puede estar dentro de Repetir N.", "Advertencias", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Repetir()
        {
            if (macro.Count > 236)
                return;

            int idc = GetIndice();
            int reps = 0;
            for (int i = 0; i <= (idc - 1); i++)
            {
                if (((TipoC)macro[i] == TipoC.TipoComando_Repeat) || ((TipoC)macro[i] == TipoC.TipoComando_Hold))
                {
                    MessageBox.Show("Mantener y Repetir no pueden producirse a la vez", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if ((TipoC)macro[i] == TipoC.TipoComando_RepeatN)
                    reps++;
                if ((TipoC)macro[i] == TipoC.TipoComando_RepeatNFin)
                    reps--;
            }
            if (reps == 0)
            {
                macro.Insert(idc, (byte)TipoC.TipoComando_Repeat);
                macro.Insert(idc + 1, (byte)TipoC.TipoComando_RepeatFin);
                CargarLista();
            }
            else
                MessageBox.Show("repetir no puede estar dentro de Repetir N.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region "MFD"
        private void Linea()
        {
            if ((macro.Count + 2 + TextBox3.Text.Length) > 237)
                return;

            int idc = GetIndice();
            macro.Insert(idc, (ushort)((byte)TipoC.TipoComando_MfdTexto + ((ushort)NumericUpDown9.Value << 8)));
            byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(TextBox3.Text));
            for (byte i = 0; i < TextBox3.Text.Length; i++)
                macro.Insert(idc + i + 1, (ushort)((byte)TipoC.TipoComando_MfdTexto + (stb[i] << 8)));

            macro.Insert(idc + stb.Length, (byte)TipoC.TipoComando_MfdTextoFin);
            CargarLista();
        }

        private void Hora(bool f24h)
        {
            if (macro.Count > 235)
                return;
            if ((NumericUpDown10.Value < 0) && (NumericUpDown7.Value == 1))
            {
                MessageBox.Show("El reloj 1 no puede tener horas negativas.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int idc = GetIndice();
            TipoC tipo = (f24h) ? TipoC.TipoComando_MfdHora24 : TipoC.TipoComando_MfdHora;
            macro.Insert(idc, (ushort)((byte)tipo + ((ushort)NumericUpDown7.Value << 8)));
            if (NumericUpDown7.Value == 1)
            {
                macro.Insert(idc + 1, (ushort)((byte)tipo + ((ushort)NumericUpDown10.Value << 8)));
                macro.Insert(idc + 2, (ushort)((byte)tipo + ((ushort)NumericUpDown11.Value << 8)));
            }
            else
            {
                int minutos = (int)((NumericUpDown10.Value * 60) + NumericUpDown11.Value);
                if (minutos < 0)
                {
                    macro.Insert(idc + 1, (ushort)((byte)tipo + ((((ushort)-minutos >> 8) + 4) << 8) ));
                    macro.Insert(idc + 2, (ushort)((byte)tipo + (((ushort)-minutos & 0xff) << 8) ));
                }
                else
                {
                    macro.Insert(idc + 1, (ushort)((byte)tipo + ((((ushort)minutos >> 8)) << 8)));
                    macro.Insert(idc + 2, (ushort)((byte)tipo + (((ushort)minutos & 0xff) << 8)));
                }
            }
            CargarLista();
        }

        private void Fecha(ushort f)
        {
            if (macro.Count > 236) return;
            int idc = GetIndice();
            macro.Insert(idc, (ushort)((byte)TipoC.TipoComando_MfdFecha + (f << 8)));
            macro.Insert(idc + 1, (ushort)((byte)TipoC.TipoComando_MfdFecha + ((ushort)NumericUpDown13.Value << 8)));
            CargarLista();
        }
        #endregion
    }
}
