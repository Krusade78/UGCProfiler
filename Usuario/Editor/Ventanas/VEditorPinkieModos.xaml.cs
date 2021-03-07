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
                ar.Comandos = new ushort[] { (byte)CTipos.TipoComando.TipoComando_Pinkie + (1 << 8), (byte)CTipos.TipoComando.TipoComando_MfdPinkie + (1 << 8) };
                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
            }
            for (byte m = 0; m < 3; m++)
            {
                padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(1, 0, m, 3).TamIndices = 0;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(1, 0, m, 3, 0).idAccion = idAccion;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(1, 0, m, 3, 1).idAccion = 0;
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
                ar.Comandos = new ushort[] { (byte)CTipos.TipoComando.TipoComando_Pinkie, (byte)CTipos.TipoComando.TipoComando_MfdPinkie };
                padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
            }
            for (byte m = 0; m < 3; m++)
            {
                padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(1, 1, m, 3).TamIndices = 0;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(1, 1, m, 3, 0).idAccion = 0;
                padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(1, 1, m, 3, 1).idAccion = idAccion;
            }
        }

        private void GuardarModos()
        {
            MainWindow padre = (MainWindow)this.Owner;

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
                    ar.Comandos = new ushort[] { (ushort)((byte)CTipos.TipoComando.TipoComando_Modo + ((modo - 1) << 8)) };
                    padre.GetDatos().Perfil.ACCIONES.AddACCIONESRow(ar);
                }
                for (byte p = 0; p < 2; p++)
                {
                    for (byte m = 0; m < 3; m++)
                    {
                        padre.GetDatos().Perfil.MAPABOTONES.FindByidJoyidPinkieidModoidBoton(1, p, m, (byte)(4 + modo)).TamIndices = 0;
                        padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(1, p, m, (uint)(4 + modo), 0).idAccion = idAccion;
                        padre.GetDatos().Perfil.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(1, p, m, (uint)(4 + modo), 1).idAccion = 0;
                    }
                }
            }
        }
    }
}
