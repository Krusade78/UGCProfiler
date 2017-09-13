using System;
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
                            for (int i = 0; i < st.Length; i++)
                                macro.RemoveAt(0);

                            CheckBox1.IsChecked = true;
                        }
                    }
                }
                Cargar();
            }
            else
            {
                CheckBox1.IsChecked = true;
            }
            PasarABasico();

        }

        private void Cargar()
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

        private void Guardar()
        {
            if (TextBoxNombre.Text.Trim() == "")
                return;
            if (indicep == -1)
            {
                Dim idx As Integer = padre.ComboBoxMacro.Items.IndexOf(Me.TextBoxNombre.Text.Trim())
                If idx = -1 Then 'no existe
                { 
                    If CheckBox1.Checked Then 'texto x52
                    {
                        Dim st As String = Me.TextBoxNombre.Text.Trim()
                        Dim stb(32) As Byte
                        WideCharToMultiByte(20127, 0, st, st.Length, stb, 33, 0, 0)
                        macro.Insert(0, 56)
                        If st.Length > 16 Then
                            For j As Short = 15 To 0 Step - 1
                                macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                            Next
                        Else
                            For j As Short = st.Length - 1 To 0 Step - 1
                                macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                            Next
                        End If
                        macro.Insert(0, 24 + (3 << 8))
                    }
                    idx = padre.ComboBoxAssigned.Items.Add(Me.TextBoxNombre.Text.Trim())
                    padre.ComboBoxMacro.Items.Add(Me.TextBoxNombre.Text.Trim())
                    padre.datos.AñadirIndice(idx - 1, macro)
                    Me.Tag = idx - 1
                    Me.DialogResult = Windows.Forms.DialogResult.Ignore
                }
                else
                {
                    Traduce.Msg("name_repeated", "warning", MsgBoxStyle.Exclamation)
                    return;
                }
            }
            else
            {
                If CheckBox1.Checked Then 'texto x52
                {
                    Dim st As String = Me.TextBoxNombre.Text.Trim()
                    Dim stb(32) As Byte
                    WideCharToMultiByte(20127, 0, st, st.Length, stb, 33, 0, 0)
                    macro.Insert(0, 56)
                    If st.Length > 16 Then
                        For j As Short = 15 To 0 Step - 1
                            macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                        Next
                    Else
                        For j As Short = st.Length - 1 To 0 Step - 1
                            macro.Insert(0, 24 + (CInt(stb(j)) << 8))
                        Next
                    End If
                    macro.Insert(0, 24 + (3 << 8))
                }
                Dim idx As Integer = padre.ComboBoxAssigned.Items.Add(Me.TextBoxNombre.Text.Trim())
                padre.ComboBoxMacro.Items.Add(Me.TextBoxNombre.Text.Trim())
                If idx<>(indicep + 1) Then
                {
                    padre.datos.AñadirIndice(idx - 1, macro)
                    padre.datos.IntercambiarIndices(idx, indicep + 1, padre.ComboBoxAssigned, padre.ComboBoxMacro)
                }
                else
                {
                    padre.ComboBoxAssigned.Items.RemoveAt(idx + 1)
                    padre.ComboBoxMacro.Items.RemoveAt(idx)
                    padre.datos.GetMacro(indicep).Clear()
                    For i As Integer = 0 To macro.Count - 1
                        padre.datos.GetMacro(indicep).Add(macro(i))
                    Next
                }
                If idx <= (indicep + 1) Then
                    Me.Tag = idx - 1
                else
                    Me.Tag = idx - 2

                Me.DialogResult = Windows.Forms.DialogResult.OK
            }

            this.Close();
        }
    }
}
