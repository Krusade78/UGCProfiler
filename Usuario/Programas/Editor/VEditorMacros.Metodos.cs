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
                    byte[] stb =System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(st));

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
                            ListBox1.Items.Add("Soltar " + Convert.ToString(ComboBox1.Items(dato)).Remove(0, Convert.ToString(ComboBox1.Items(dato)).IndexOf(" ")));
                        }
                        else
                        {
                            ListBox1.Items.Add("Presionar " + Convert.ToString(ComboBox1.Items(dato)).Remove(0, Convert.ToString(ComboBox1.Items(dato)).IndexOf(" ")));
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
                    case 10:
                        ListBox1.Items.Add(Traduce.Txt("pause") + " " + dato);
                        break;
                    case 11:
                        ListBox1.Items.Add(Traduce.Txt("hold"));
                        break;
                    case 12:
                        if (soltar)
                            ListBox1.Items.Add(Traduce.Txt("repeat_end"));
                        else
                            ListBox1.Items.Add(Traduce.Txt("repeat"));
                        break;
                    case 13:
                        if (soltar)
                            ListBox1.Items.Add(Traduce.Txt("repeatn_end"));
                        else
                            ListBox1.Items.Add(Traduce.Txt("repeatn") + " " + dato);
                        break;
                    case 14:
                        ListBox1.Items.Add(Traduce.Txt("mode" + (dato + 1)));
                        break;
                    case 15:
                        ListBox1.Items.Add(Traduce.Txt("aux" + (dato + 1)));
                        break;
                    case 16:
                        if (dato == 0)
                            ListBox1.Items.Add(Traduce.Txt("pinkie") + " Off");
                        else
                            ListBox1.Items.Add(Traduce.Txt("pinkie") + " On");
                        break;
                    case 18:
                        if (soltar)
                        {
                            ListBox1.Items.Add("DX " + Traduce.Txt("button") + " " + dato + 1 + " Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("DX " + Traduce.Txt("button") + " " + dato + 1 + " On");
                        }
                        break;
                    case 19:
                        if (soltar)
                        {
                            ListBox1.Items.Add("DX " + Traduce.Txt("pov") + "_" + ((dato / 8) + 1) + ((dato % 8) + 1) + " Off");
                        }
                        else
                        {
                            ListBox1.Items.Add("DX " + Traduce.Txt("pov") + "_" + ((dato / 8) + 1) + ((dato % 8) + 1) + " On");
                        }
                        break;
                    case 20:
                        ListBox1.Items.Add(Traduce.Txt("mfd_light") + " " + dato);
                        break;
                    case 21:
                        ListBox1.Items.Add(Traduce.Txt("button_light") + " " + dato);
                        break;
                    case 22:
                        if (dato == 0)
                            ListBox1.Items.Add(Traduce.Txt("light") + " Info Off");
                        else
                            ListBox1.Items.Add(Traduce.Txt("light") + " Info On");
                        break;
                    case 23:
                        if (dato == 0)
                            ListBox1.Items.Add("MFD " + Traduce.Txt("pinkie") + " Off");
                        else
                            ListBox1.Items.Add("MFD " + Traduce.Txt("pinkie") + " On");
                        break;
                    case 24:
                        string texto = Traduce.Txt("text_line") + "_" + dato;
                        byte[] ascii = new byte[16];
                        byte j = 0;
                        while (macro(i + 1) != 56)
                        {
                            i = i + 1;
                            ascii(j) = macro(i) >> 8;
                            j = j + 1;
                        }
                        i = i + 1;
                        texto = texto + "  " + System.Text.ASCIIEncoding.ASCII.GetString(ascii);
                        ListBox1.Items.Add(texto);
                        break;
                    case 25:
                        if (dato == 1)
                        {
                            ListBox1.Items.Add("(AM/PM) " + Traduce.Txt("hour") + "_" + dato + " " + (macro(i + 1) >> 8) + ":" + (macro(i + 2) >> 8));
                        }
                        else
                        {
                            ListBox1.Items.Add("(AM/PM) " + Traduce.Txt("hour") + "_" + dato + " " + ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) / 60) + ":" + ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) % 60));
                        }
                        i = i + 2;
                        break;
                    case 26:
                        if (dato == 1)
                        {
                            ListBox1.Items.Add("(24H) " + Traduce.Txt("hour") + "_" + dato + " " + (macro(i + 1) >> 8) + ":" + (macro(i + 2) >> 8));
                        }
                        else
                        {
                            ListBox1.Items.Add("(24H) " + Traduce.Txt("hour") + "_" + dato + " " + ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) / 60) + ":" + ((((macro(i + 1) >> 8) * 256) + (macro(i + 2) >> 8)) % 60));
                        }
                        i = i + 2;
                        break;
                    case 27:
                        ListBox1.Items.Add(Traduce.Txt("date") + "_" + dato + " " + (macro(i + 1) >> 8));
                        i = i + 1;
                        break;
                }
            }

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
    }
}
