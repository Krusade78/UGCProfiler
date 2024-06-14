using System;
using System.Windows.Controls;
using System.Data;
using Comunes;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlListar.xaml
    /// </summary>
    internal partial class CtlListar : UserControl
    {
        private readonly DataTable datos = new DataTable();
        private readonly DSPerfil padre;


        public CtlListar()
        {
            InitializeComponent();
            padre = ((MainWindow)App.Current.MainWindow).GetData().Profile;
            Iniciar();
            ListViewX52J.DataContext = new DataView(datos) { RowFilter = "(idJ = 1)" };
            ListViewX52T.DataContext = new DataView(datos) { RowFilter = "(idJ = 2)" };
            ListViewPedal.DataContext = new DataView(datos) { RowFilter = "(idJ = 0)" };
            ListViewNXT.DataContext = new DataView(datos) { RowFilter = "(idJ = 3)" };
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            datos.Dispose();
        }

        private void Iniciar()
        {
            datos.Columns.Add("idJ", typeof(byte));
            datos.Columns.Add("tipo", typeof(byte));
            datos.Columns.Add("Elementos");
            datos.Columns.Add("m1");
            datos.Columns.Add("m1p");
            datos.Columns.Add("m2");
            datos.Columns.Add("m2p");
            datos.Columns.Add("m3");
            datos.Columns.Add("m3p");

            datos.Rows.Add(new object[] { 1, 0, "Eje X", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 0, "Eje Y", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 0, "Eje R", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Gatillo 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Gatillo 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Botón Lanzar", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Botón A", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Botón B", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Botón C", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Pnkie", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 1 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 2, "Seta 2 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Toggle 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Toggle 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Toggle 3", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Toggle 4", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Toggle 5", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Toggle 6", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Modo 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Modo 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, 1, "Modo 3", "", "", "", "", "", "" });

            datos.Rows.Add(new object[] { 2, 0, "Eje Z", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 0, "Slider", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 0, "Eje Rx", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 0, "Eje Ry", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 0, "Ministick X", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 0, "Ministick Y", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón D", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón I", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón Ratón", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 3 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 2, "Seta 4 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Rueda Botón", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Rueda Arriba", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Rueda Abajo", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón MFD 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón MFD 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, 1, "Botón MFD 3", "", "", "", "", "", "" });

            datos.Rows.Add(new object[] { 0, 0, "Eje R", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 0, 0, "Freno Izquierdo", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 0, 0, "Freno Derecho", "", "", "", "", "", "" });

            datos.Rows.Add(new object[] { 3, 0, "Eje X", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 0, "Eje Y", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 0, "Eje Z", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 0, "Eje Rx", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Gatillo P.1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Gatillo P.2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Gatillo S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Botón Lanzar", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Botón 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Pinkie", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Base 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Base 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Base 3", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Enc. 1 Arriba", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Enc. 1 Abajo", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Enc. 2 Arriba", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Enc. 2 Abajo", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 1, "Seta 1 Centro", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 1 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 0, "Ministick X", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 0, "Ministick Y", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 2 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 2 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 2 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 2 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 3 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 3 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 3 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 3 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 4 Centro", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 4 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 4 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 4 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, 2, "Seta 4 O", "", "", "", "", "", "" });

            byte idx = 0; byte t = 1;
            foreach (DataRow r in datos.Rows)
            {
                if ((byte)r[0] != t)
                {
                    idx = 0;
                    t = (byte)r[0];
                }
                for (byte m = 0; m < 3; m++)
                {
                    CogerDatos(r, (byte)r[0], 0, m, idx, (byte)r[1]);
                    CogerDatos(r, (byte)r[0], 1, m, idx, (byte)r[1]);
                }
                idx++;
            }
        }

        private void CogerDatos(DataRow r, byte idJ, byte p, byte m, byte idc, byte tipo)
        {
            byte id = Mapear(idJ, idc);
            if (id == 255)
                return;

            if (tipo == 2)
                r[3 + (m * 2) + p] = Boton(idJ, p, m, id, true);
            else if (tipo == 1)
                r[3 + (m * 2) + p] = Boton(idJ, p, m, id, false);
            else
                r[3 + (m * 2) + p] = Eje(idJ, p, m, id);
        }

        private byte Mapear(byte idJ, byte idc)
        {
            byte[] m;
            if (idJ == 1)
                m = new byte[] { 0, 1, 3, 0, 1, 2, 8, 9, 4, 3, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 ,10, 11, 12, 13, 14, 15, 10, 11, 12, 13, 14, 15, 5, 6, 7 };
            else if (idJ == 2)
                m = new byte[] { 2, 5, 3, 4, 6, 7, 0, 1, 5, 6, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 7, 8, 9, 2, 3, 4 };
            else if (idJ == 0)
                m = new byte[] { 3, 6, 7 };
            else
                m = new byte[] { 0, 1, 2, 3, 9, 8, 14, 11, 15, 10, 4, 5, 6, 0, 1, 2, 3, 12, 0, 1, 2, 3, 4, 5, 6, 7, 6, 7, 8, 10, 12, 14, 16, 18, 20, 22, 13, 24, 26, 28, 30 };
            return m[idc];
        }

        private string Boton(byte idJ, byte p, byte m, byte b, bool seta)
        {
            String n = "";
            if (seta)
            {
                DSPerfil.INDICESSETASRow ri;
                DSPerfil.MAPASETASRow rm = padre.MAPASETAS.FindByidJoyidPinkieidModoIdSeta(idJ, p, m, b);
                if (rm.TamIndices > 0)
                {
                    for (byte i = 0; i < rm.TamIndices; i++)
                    {
                        ri = padre.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, b, i);
                        n = n + ", " + ri.ACCIONESRow.Nombre;
                    }
                }
                else
                {
                    ri = padre.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, b, 0);
                    if (ri.idAccion > 0)
                        n = ri.ACCIONESRow.Nombre;
                    ri = padre.INDICESSETAS.FindByidJoyidPinkieidModoidSetaid(idJ, p, m, b, 1);
                    if (ri.idAccion > 0)
                        n = n + " / " + ri.ACCIONESRow.Nombre;
                }
            }
            else
            {
                DSPerfil.INDICESBOTONESRow ri;
                DSPerfil.MAPABOTONESRow rm = padre.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(idJ, p, m, b);
                if (rm.TamIndices > 0)
                {
                    for (byte i = 0; i < rm.TamIndices; i++)
                    {
                        ri = padre.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, b, i);
                        n = n + ", " + ri.ACCIONESRow.Nombre;
                    }
                }
                else
                {
                    ri = padre.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, b, 0);
                    if (ri.idAccion > 0)
                        n = ri.ACCIONESRow.Nombre;
                    ri = padre.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(idJ, p, m, b, 1);
                    if (ri.idAccion > 0)
                        n = n + " / " + ri.ACCIONESRow.Nombre;
                }
            }
            return n;
        }

        private string Eje(byte idJ, byte p, byte m, byte e)
        {
            string n = "";
            byte neje = padre.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).Eje;
            byte tipoEje = padre.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).TipoEje;
            if ((tipoEje & 0b10) == 0b10) { n = "-"; }
            if (neje == 6) { neje = 7; }
            else if (neje == 7) { neje = 6; }
            neje = (byte)(((tipoEje & 0b1) == 0) ? 0 : neje + 1);
            if ((tipoEje & 0b1000) == 0b1000) //ratón
            {
                neje += 9;
            }
            string[] nombreEje = new String[] { "[Ninguno]", "[X]", "[Y]", "[Z]", "[Rx]", "[Ry]", "[Rz]", "[Sl 1]", "[Sl 2]", "[Ratón X]", "[Ratón Y]" };
            n += ((neje == 0) ? "" : $"Joy {idJ} ") + nombreEje[neje] + " ";


            if ((tipoEje & 0b10000) == 0b10000) //incremental
            {
                DSPerfil.INDICESEJESRow ri = padre.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, e, 0);
                if (ri.idAccion > 0) n += "+" + ri.ACCIONESRow.Nombre; else n += " ";
                ri = padre.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p, m, e, 1);
                if (ri.idAccion > 0) n += "/-" + ri.ACCIONESRow.Nombre; else n += "/ ";
            }
            else if((tipoEje & 0b100000) == 0b100000) //bandas
            {
                byte[] bandas = padre.MAPAEJES.FindByidJoyidPinkieidModoidEje(idJ, p, m, e).Bandas;
                byte nBandas = 0;
                foreach (byte b in bandas)
                {
                    nBandas++;
                    if (b == 0)
                    {
                        break;
                    }
                }
                n += " | ";
                for (byte i = 0; i < nBandas; i++)
                {
                    DSPerfil.INDICESEJESRow ri = padre.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(idJ, p,m, e, i);
                    n += ", " + ri.ACCIONESRow.Nombre;
                }
                n = n.Replace("| ,", "| ");
            }

            return n;
        }
    }
}
