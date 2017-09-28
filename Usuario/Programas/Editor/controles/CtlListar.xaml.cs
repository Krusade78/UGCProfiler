using System;
using System.Windows.Controls;
using System.Data;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlListar.xaml
    /// </summary>
    internal partial class CtlListar : UserControl
    {
        private DataTable datos = new DataTable();
        private DSPerfil padre;


        public CtlListar()
        {
            InitializeComponent();
            padre = ((MainWindow)App.Current.MainWindow).GetDatos().Perfil;
            Iniciar();
            ListView1.DataContext = new DataView(datos) { RowFilter = "(id < 36) And (id > 0)" };
            ListView2.DataContext = new DataView(datos) { RowFilter = "id > 36" };
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            datos.Dispose();
        }

        private void Iniciar()
        {
            datos.Columns.Add("id", typeof(UInt16));
            datos.Columns.Add("Elementos");
            datos.Columns.Add("m1");
            datos.Columns.Add("m1p");
            datos.Columns.Add("m2");
            datos.Columns.Add("m2p");
            datos.Columns.Add("m3");
            datos.Columns.Add("m3p");

            datos.Rows.Add(new object[] { 0, "Joystick", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 1, "Eje X", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 2, "Eje Y", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 3, "Eje R", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 4, "Gatillo 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 5, "Gatillo 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 6, "Botón Lanzar", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 7, "Botón A", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 8, "Botón B", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 9, "Botón C", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 10, "Pnkie", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 11, "Seta 1 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 12, "Seta 1 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 13, "Seta 1 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 14, "Seta 1 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 15, "Seta 1 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 16, "Seta 1 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 17, "Seta 1 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 18, "Seta 1 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 19, "Seta 2 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 20, "Seta 2 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 21, "Seta 2 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 22, "Seta 2 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 23, "Seta 2 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 24, "Seta 2 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 25, "Seta 2 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 26, "Seta 2 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 27, "Toggle 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 28, "Toggle 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 29, "Toggle 3", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 30, "Toggle 4", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 31, "Toggle 5", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 32, "Toggle 6", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 33, "Modo 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 34, "Modo 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 35, "Modo 3", "", "", "", "", "", "" });

            datos.Rows.Add(new object[] { 36, "Acelerador", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 37, "Eje Z", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 38, "Slider", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 39, "Eje Rx", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 40, "Eje Ry", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 41, "Ministick X", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 42, "Ministick Y", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 43, "Botón D", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 44, "Botón E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 45, "Botón I", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 46, "Botón Ratón", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 47, "Seta 3 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 48, "Seta 3 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 49, "Seta 3 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 50, "Seta 3 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 51, "Seta 3 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 52, "Seta 3 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 53, "Seta 3 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 54, "Seta 3 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 55, "Seta 4 N", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 56, "Seta 4 NE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 57, "Seta 4 E", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 58, "Seta 4 SE", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 59, "Seta 4 S", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 60, "Seta 4 SO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 61, "Seta 4 O", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 62, "Seta 4 NO", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 63, "Rueda Botón", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 64, "Rueda Arriba", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 65, "Rueda Abajo", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 66, "Botón MFD 1", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 67, "Botón MFD 2", "", "", "", "", "", "" });
            datos.Rows.Add(new object[] { 68, "Botón MFD 3", "", "", "", "", "", "" });

            int col = 1;
            for (int i = 1; i < datos.Rows.Count; i++)
            {
                for (byte m = 0; m < 3; m++)
                {
                    CogerDatos(col, 0, m, i);
                    col++;
                    CogerDatos(col, 1, m, i);
                    col++;
                }
            }
        }

        private void CogerDatos(int col, byte p, byte m, int idc)
        {
            byte id = Mapear(idc);
            if (id == 255)
                return;

            if (id > 99)
                datos.Rows[idc][2 + (m * 2) + p] = Boton(p, m, (byte)(id - 100), true);
            else if (id < 64)
                datos.Rows[idc][2 + (m * 2) + p] = Boton(p, m, id, false);
            else if (id < 71)
                datos.Rows[idc][2 + (m * 2) + p] = Eje(p, m, (byte)(id - 64));
            else
                datos.Rows[idc][2 + (m * 2) + p] = MiniStick(p, m, (byte)(id - 71));
        }

        private byte Mapear(int idc)
        {
            byte[] m = new byte[] { 255, 64, 65, 66, 22, 0, 3, 1, 2, 7, 6, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 16, 17, 18, 19, 20, 21, 8, 9, 10, 255, 67, 70, 68, 69, 71, 72, 4, 15, 14, 5, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 23, 24, 25, 11, 12, 13 };
            return m[idc];
        }

        private String Boton(byte p, byte m, byte b, bool seta)
        {
            String n = "";
            if (seta)
            {
                DSPerfil.INDICESSETASRow ri;
                DSPerfil.MAPASETASRow rm = padre.MAPASETAS.FindByidSetaidModoidPinkie(b, m, p);
                if (rm.Estado > 0)
                {
                    for (byte i = 0; i < rm.Estado; i++)
                    {
                        ri = padre.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | b), i);
                        n = n + ", " + ri.ACCIONESRow.Nombre;
                    }
                }
                else
                {
                    ri = padre.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | b), 0);
                    if (ri.Indice > 0)
                        n = ri.ACCIONESRow.Nombre;
                    ri = padre.INDICESSETAS.FindByidSetaid((UInt32)((p << 16) | (m << 8) | b), 1);
                    if (ri.Indice > 0)
                        n = n + " / " + ri.ACCIONESRow.Nombre;
                }
            }
            else
            {
                DSPerfil.INDICESBOTONESRow ri;
                DSPerfil.MAPABOTONESRow rm = padre.MAPABOTONES.FindByidBotonidModoidPinkie(b, m, p);
                if (rm.Estado > 0)
                {
                    for (byte i = 0; i < rm.Estado; i++)
                    {
                        ri = padre.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | b), i);
                        n = n + ", " + ri.ACCIONESRow.Nombre;
                    }
                }
                else
                {
                    ri = padre.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | b), 0);
                    if (ri.Indice > 0)
                        n = ri.ACCIONESRow.Nombre;
                    ri = padre.INDICESBOTONES.FindByidBotonid((UInt32)((p << 16) | (m << 8) | b), 1);
                    if (ri.Indice > 0)
                        n = n + " / " + ri.ACCIONESRow.Nombre;
                }
            }
            return n;
        }

        private String Eje(byte p, byte m, byte e)
        {
            String n = "";
            bool peque = (e > 3);
            if (peque) e -= 4;
            byte neje = (!peque) ? padre.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).nEje : padre.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).nEje;
            bool incremental = ((neje & 128) == 128);
            neje &= 127;
            if (neje > 19) { n = "-"; neje -= 20; }

            String[] nombreEje = new String[] { "[Ninguno]", "[X]", "[Y]", "[R]", "[Z]", "[Rx]", "[Ry]", "[Sl 1]", "[Sl 2]", "[MiniStick X]", "[MiniStick Y]", "[Ratón X]", "[Ratón Y]" };
            n = n + nombreEje[neje] + " ";

            if (incremental)
            {
                if (peque)
                {
                    DSPerfil.INDICESEJESPEQUERow ri = padre.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 0);
                    if (ri.Indice > 0) n = n + "+" + ri.ACCIONESRow.Nombre; else n = n + " ";
                    ri = padre.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 1);
                    if (ri.Indice > 0) n = n + "/-" + ri.ACCIONESRow.Nombre; else n = n + "/ ";
                }
                else
                {
                    DSPerfil.INDICESEJESRow ri = padre.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 0);
                    if (ri.Indice > 0) n = n + "+" + ri.ACCIONESRow.Nombre; else n = n + " ";
                    ri = padre.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), 1);
                    if (ri.Indice > 0) n = n + "/-" + ri.ACCIONESRow.Nombre; else n = n + "/ ";
                }
            }
            else
            {
                if (peque)
                {
                    byte[] bandas = padre.MAPAEJESPEQUE.FindByidEjeidModoidPinkie(e, m, p).Bandas;
                    byte nBandas = 0;
                    foreach (byte b in bandas)
                    {
                        if (b == 0)
                            break;
                        else
                            nBandas++;
                    }
                    for (byte i = 0; i < nBandas; i++)
                    {
                        DSPerfil.INDICESEJESPEQUERow ri = padre.INDICESEJESPEQUE.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), i);
                        n = n + ", " + ri.ACCIONESRow.Nombre;
                    }
                }
                else
                {
                    byte[] bandas = padre.MAPAEJES.FindByidEjeidModoidPinkie(e, m, p).Bandas;
                    byte nBandas = 0;
                    foreach (byte b in bandas)
                    {
                        if (b == 0)
                            break;
                        else
                            nBandas++;
                    }
                    for (byte i = 0; i < nBandas; i++)
                    {
                        DSPerfil.INDICESEJESRow ri = padre.INDICESEJES.FindByidEjeid((UInt32)((p << 16) | (m << 8) | e), i);
                        n = n + ", " + ri.ACCIONESRow.Nombre;
                    }
                }
            }

            return n;
        }

        private String MiniStick(byte p, byte m, byte id)
        {
            String n = "";
            byte neje = padre.MAPAEJESMINI.FindByidEjeidModoidPinkie(id, m, p).nEje;
            if (neje > 19) { n = "-"; neje -= 20; }

            String[] nombreEje = new String[] { "[Ninguno]", "[X]", "[Y]", "[R]", "[Z]", "[Rx]", "[Ry]", "[Sl 1]", "[Sl 2]", "[MiniStick X]", "[MiniStick Y]", "[Ratón X]", "[Ratón Y]" };
            return n + nombreEje[neje];
        }
    }
}
