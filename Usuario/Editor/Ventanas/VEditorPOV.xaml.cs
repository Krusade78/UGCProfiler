using System;
using System.Windows;
using static Comunes.CTipos;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal partial class VEditorPOV : Window
    {
        private byte pov;
        public VEditorPOV(byte idSeta)
        {
            InitializeComponent();
            pov = idSeta;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Guardar();
            this.DialogResult = true;
            this.Close();
        }

        private void Guardar()
        {
            MainWindow padre = (MainWindow)App.Current.MainWindow;
            String[] st = new String[] { "<DX Seta 1 N>", "<DX Seta 1 NO>", "<DX Seta 1 O>", "<DX Seta 1 SO>", "<DX Seta 1 S>", "<DX Seta 1 SE>", "<DX Seta 1 E>", "<DX Seta 1 NE>" };
            byte idJ = 0,p = 0, m = 0;

            padre.GetModos(ref idJ, ref p, ref m);
            pov /= 8;


            for (byte i = 0; i < 8; i++)
                st[i] = st[i].Replace("1", NumericUpDownJ.Value.ToString() + "-" + NumericUpDown1.Value.ToString());

            for (byte i = 0; i < 8; i++)
            {
                ushort idx = 0;
                foreach (Comunes.DSPerfil.ACCIONESRow ar in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if (ar.Nombre == st[i])
                    {
                        idx = ar.idAccion;
                        break;
                    }
                }

                if (idx == 0)
                {
                    idx = 0;
                    foreach (Comunes.DSPerfil.ACCIONESRow aar in padre.GetDatos().Perfil.ACCIONES.Rows)
                    {
                        if (aar.idAccion > idx)
                            idx = aar.idAccion;
                    }
                    idx++;

                    Comunes.DSPerfil.ACCIONESRow ar = padre.GetDatos().Perfil.ACCIONES.NewACCIONESRow();
                    ar.idAccion = idx;
                    ar.Nombre = st[i];
                    ar.Comandos = new ushort[1 + st[i].Length + 1 + 3];
                    //'texto x52
                    ar.Comandos[0] = (byte)TipoComando.TipoComando_MfdTextoIni + (3 << 8); //línea
                    byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(st[i]));
                    for (byte j = 0; j < texto.Length; j++)
                    {
                        ar.Comandos[1 + j] = (ushort)((byte)TipoComando.TipoComando_MfdTexto + (texto[j] << 8));
                    }
                    ar.Comandos[1 + texto.Length] = (byte)TipoComando.TipoComando_MfdTextoFin;
                    //Resto
                    int v = (((((4 - NumericUpDown1.Value) * 8) + i) << 3) + (NumericUpDownJ.Value - 1)) << 8;
                    ar.Comandos[1 + texto.Length + 1] = (ushort)((byte)TipoComando.TipoComando_DxSeta + (ushort)v);
                    ar.Comandos[1 + texto.Length + 2] = (byte)TipoComando.TipoComando_Hold;
                    ar.Comandos[1 + texto.Length + 3] = (ushort)((byte)(TipoComando.TipoComando_DxSeta | TipoComando.TipoComando_Soltar) + (ushort)v);

                    padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
                }

                padre.GetDatos().Perfil.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, (byte)(i + (pov * 8))).TamIndices = 0;
                padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, (uint)(i + (pov * 8)), 0).idAccion = idx;
                padre.GetDatos().Perfil.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, (uint)(i + (pov * 8)), 1).idAccion = 0;
            }
        }
    }
}
