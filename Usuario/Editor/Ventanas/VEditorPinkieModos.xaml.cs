using System;
using System.Windows;
using Comunes;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal partial class VEditorPinkieModos : Window
    {
        private readonly bool modos;

        public VEditorPinkieModos(bool modos)
        {
            InitializeComponent();
            if (modos)
            {
                this.Title = "Asignar a modos";
                txt.Text = "Esto asignará los botones de modo por defecto como selectores de modo. Se añadirá cualquier macro que sea necesaria y se sobreescribirá la configuración de los 3 botones.";
            }
            this.modos = modos;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            if (modos)
                GuardarModos();
            else
                GuardarPinkie();

            this.DialogResult = true;
            this.Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GuardarPinkie()
        {
            ushort idAccion = 0;
            MainWindow padre = (MainWindow)this.Owner;
            byte joy = 0, md = 0, pk = 0;
            padre.GetModos(ref joy, ref pk, ref md);

            //'on
            foreach (DSPerfil.ACCIONESRow ar in padre.GetDatos().Perfil.ACCIONES.Rows)
            {
                if (ar.Nombre == "<Pinkie On>")
                {
                    idAccion = ar.idAccion;
                    break;
                }
            }

            if (idAccion == 0)
            {
                foreach (DSPerfil.ACCIONESRow aar in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if (aar.idAccion > idAccion)
                        idAccion = aar.idAccion;
                }
                idAccion++;

                DSPerfil.ACCIONESRow ar = padre.GetDatos().Perfil.ACCIONES.NewACCIONESRow();
                ar.idAccion = idAccion;
                ar.Nombre = "<Pinkie On>";
                ar.Comandos = new ushort[] { (byte)CTipos.TipoComando.TipoComando_Pinkie + (1 << 8),
                                            (byte)CTipos.TipoComando.TipoComando_MfdPinkie + (1 << 8), 
                                            0x0b32, 0x0732, 0x0032, 0x0432};
                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
            }
            for (byte m = 0; m < 3; m++)
            {
                padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(joy, 0, m, (byte)((joy == 1) ? 3 : 10)).TamIndices = 0;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(joy, 0, m, (byte)((joy == 1) ? 3 : 10), 0).idAccion = idAccion;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(joy, 0, m, (byte)((joy == 1) ? 3 : 10), 1).idAccion = 0;
            }

            //'off
            idAccion = 0;
            foreach (DSPerfil.ACCIONESRow ar in padre.GetDatos().Perfil.ACCIONES.Rows)
            {
                if (ar.Nombre == "<Pinkie Off>")
                {
                    idAccion = ar.idAccion;
                    break;
                }
            }

            if (idAccion == 0)
            {
                foreach (DSPerfil.ACCIONESRow aar in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if (aar.idAccion > idAccion)
                        idAccion = aar.idAccion;
                }
                idAccion++;

                DSPerfil.ACCIONESRow ar = padre.GetDatos().Perfil.ACCIONES.NewACCIONESRow();
                ar.idAccion = idAccion;
                ar.Nombre = "<Pinkie Off>";
                ar.Comandos = new ushort[] { (byte)CTipos.TipoComando.TipoComando_Pinkie,
                                             (byte)CTipos.TipoComando.TipoComando_MfdPinkie,
                                             0x0b32, 0x0732, 0x0032, 0x0032};
                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
            }
            for (byte m = 0; m < 3; m++)
            {
                padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(joy, 1, m, (byte)((joy == 1) ? 3 : 10)).TamIndices = 0;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(joy, 1, m, (byte)((joy == 1) ? 3 : 10), 0).idAccion = 0;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(joy, 1, m, (byte)((joy == 1) ? 3 : 10), 1).idAccion = idAccion;
            }
        }

        private void GuardarModos()
        {
            MainWindow padre = (MainWindow)this.Owner;
            byte joy = 0, md = 0, pk = 0;
            padre.GetModos(ref joy, ref pk, ref md);

            for (byte modo = 1; modo <= 3; modo++)
            {
                ushort idAccion = 0;

                foreach (DSPerfil.ACCIONESRow ar in padre.GetDatos().Perfil.ACCIONES.Rows)
                {
                    if (ar.Nombre == "<Modo " + modo.ToString() + ">")
                    {
                        idAccion = ar.idAccion;
                        break;
                    }
                }

                if (idAccion == 0)
                {
                    foreach (DSPerfil.ACCIONESRow aar in padre.GetDatos().Perfil.ACCIONES.Rows)
                    {
                        if (aar.idAccion > idAccion)
                            idAccion = aar.idAccion;
                    }
                    idAccion++;

                    DSPerfil.ACCIONESRow ar = padre.GetDatos().Perfil.ACCIONES.NewACCIONESRow();
                    ar.idAccion = idAccion;
                    ar.Nombre = "<Modo " + modo.ToString() + ">";
                    if (modo == 1)
                    {
                        ar.Comandos = new ushort[] { (ushort)((byte)CTipos.TipoComando.TipoComando_Modo + ((modo - 1) << 8)), 0x0a32, 0x3832, 0x0032, 0x0432, 0x0032, 0xc032, 0x0f32, 0x2432 };
                    }
                    else if (modo == 2)
                    {
                        ar.Comandos = new ushort[] { (ushort)((byte)CTipos.TipoComando.TipoComando_Modo + ((modo - 1) << 8)), 0x0a32, 0x3f32, 0x0032, 0x0432, 0x0032, 0xc032, 0x0f32, 0x0432 };
                    }
                    else
                    {
                        ar.Comandos = new ushort[] { (ushort)((byte)CTipos.TipoComando.TipoComando_Modo + ((modo - 1) << 8)), 0x0a32, 0xf832, 0x0132, 0x0432, 0x0032, 0xc032, 0x0f32, 0x8432 };
                    }
                    padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
                }
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        byte btBase = (byte)((joy == 1) ? 4 : 3);
                        padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(joy, p, m, (byte)(btBase + modo)).TamIndices = 0;
                        padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(joy, p, m, (uint)(btBase + modo), 0).idAccion = idAccion;
                        padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(joy, p, m, (uint)(btBase + modo), 1).idAccion = 0;
                    }
                }
            }
        }
    }
}
