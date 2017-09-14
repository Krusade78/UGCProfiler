using System;
using System.Windows;
using System.Collections.Generic;
using static Editor.CEnums;

namespace Editor
{
    internal partial class VEditorMacros
    {
        private MainWindow padre;
        private int indicep;
        private static int ultimaPlantilla = 0;
        private List<ushort> macro = new List<ushort>();

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
                    string st = TextBoxNombre.Text.Trim();
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
                            for (byte i = 0; i < st.Length + 1; i++)
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
                            ListBox1.Items.Add("Botón DX " + dato + 1 + " Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("Botón DX " + dato + 1 + " On");
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
                        if ( ((TipoC)(macro[idc1 - 2] & 0xFF) == TipoC.TipoComando_MfdHora24) || ((TipoC)(macro[idc1 - 2] & 0xFF) == TipoC.TipoComando_MfdHora) )
                            idc1 -= 2;
                    }
                }
            }
            ListBox1.SelectedIndex = sel - 1;
            int idc2 = GetIndice();
            if ( ( ((TipoC)macro[idc1] == TipoC.TipoComando_Hold   ) && ((TipoC)macro[idc2] == TipoC.TipoComando_RepeatNFin) ) || ( ((TipoC)macro[idc1] == TipoC.TipoComando_Repeat ) && ((TipoC)macro[idc2] == TipoC.TipoComando_RepeatNFin) ) ||
                 ( ((TipoC)macro[idc1] == TipoC.TipoComando_RepeatN) && ((TipoC)macro[idc2] == TipoC.TipoComando_Hold) )       || ( ((TipoC)macro[idc1] == TipoC.TipoComando_RepeatN) && ((TipoC)macro[idc2] == TipoC.TipoComando_Repeat) ) )
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
                for (int i = (texto.Length -1); i >= 0; i--)
                    macro.Insert(0, (ushort)((byte)TipoC.TipoComando_MfdTexto + (texto[i] << 8)));
                macro.Insert(0, (byte)TipoC.TipoComando_MfdTexto + (3 << 8));
            }

            if (indicep == -1)
            {
                DSPerfil.ACCIONESRow nr = padre.GetDatos().Perfil.ACCIONES.NewACCIONESRow();
                nr.Comandos = macro.ToArray();
                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(nr);
                this.DialogResult = null;
            }
            else
            {
                DSPerfil.ACCIONESRow nr = padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)indicep);
                nr.Comandos = macro.ToArray();
                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(nr);
                this.DialogResult = true;
            }

            this.Close();
        }
    }
}
