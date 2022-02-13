using System.Data;
using static Comunes.CTipos;

namespace Editor
{
    partial class DataSetMacros
    {
        partial class MACROSDataTable
        {
            public override void BeginInit()
            {
                this.RowChanged += MACROSDataTable_RowChanged;
                base.BeginInit();
            }

            private void MACROSDataTable_RowChanged(object sender, DataRowChangeEventArgs e)
            {
                if (e.Action == DataRowAction.Add)
                {
                    ((DataSetMacros.MACROSRow)e.Row).nombre = GetNombre(((DataSetMacros.MACROSRow)e.Row).comando);
                }
            }

            private string GetNombre(ushort[] comando)
            {
                byte tipo = (byte)(comando[0] & 0x7f);
                byte dato = (byte)(comando[0] >> 8);
                bool soltar = ((comando[0] & 0xff) & (byte)TipoComando.TipoComando_Soltar) == (byte)TipoComando.TipoComando_Soltar;

                if (tipo == (byte)TipoComando.TipoComando_Tecla)
                {
                    string tecla = ((TECLASDataTable)this.DataSet.Tables["TECLAS"])[dato].nombre;
                    if (soltar)
                    {
                        return "Soltar " + tecla.Remove(0, tecla.IndexOf(" "));
                    }
                    else
                    {
                        return "Presionar " + tecla.Remove(0, tecla.IndexOf(" "));
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonBt1)
                {
                    if (soltar)
                    {
                        return "Ratón->Botón 1 Off";
                    }
                    else
                    {
                        return "Ratón->Botón 1 On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonBt2)
                {
                    if (soltar)
                    {
                        return "Ratón->Botón 2 Off";
                    }
                    else
                    {
                        return "Ratón->Botón 2 On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonBt3)
                {
                    if (soltar)
                    {
                        return "Ratón->Botón 3 Off";
                    }
                    else
                    {
                        return "Ratón->Botón 3 On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonIzq)
                {
                    if (soltar)
                    {
                        return "Ratón->Izquierda 0";
                    }
                    else
                    {
                        return "Ratón->Izquierda " + dato;
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonDer)
                {
                    if (soltar)
                    {
                        return "Ratón->Derecha 0";
                    }
                    else
                    {
                        return "Ratón->Derecha " + dato;
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonAba)
                {
                    if (soltar)
                    {
                        return "Ratón->Abajo 0";
                    }
                    else
                    {
                        return "Ratón->Abajo " + dato;
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonArr)
                {
                    if (soltar)
                    {
                        return "Ratón->Arriba 0";
                    }
                    else
                    {
                        return "Ratón->Arriba " + dato;
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonWhArr)
                {
                    if (soltar)
                    {
                        return "Ratón->Rueda arriba Off";
                    }
                    else
                    {
                        return "Ratón->Rueda arriba On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_RatonWhAba)
                {
                    if (soltar)
                    {
                        return "Ratón->Rueba abajo Off";
                    }
                    else
                    {
                        return "Ratón->Rueda abajo On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_Delay)
                {
                    return "Pausa " + dato;
                }
                else if (tipo == (byte)TipoComando.TipoComando_Hold)
                {
                    return "Mantener";
                }
                else if (tipo == (byte)TipoComando.TipoComando_Repeat)
                {
                    if (soltar)
                        return "/-- Repetir Fin";
                    else
                        return "/-- Repetir Inicio";
                }
                else if (tipo == (byte)TipoComando.TipoComando_RepeatN)
                {
                    if (soltar)
                        return "/-- Repetir N Fin";
                    else
                        return "/-- Repetir N[" + dato + "] Inicio";
                }
                else if (tipo == (byte)TipoComando.TipoComando_ModoPreciso)
                {
                    string[] ejes = new string[] { "X", "Y", "Z", "Rx", "Ry", "Rz", "Sl1", "Sl2" };
                    if (!soltar)
                        return $"Modo preciso J{((dato & 31) / 8) + 1} {ejes[(dato & 31) % 8]} s{(dato >> 5) + 1} On";
                    else
                        return $"Modo preciso J{((dato & 31) / 8) + 1} {ejes[(dato & 31) % 8]} Off";
                }
                else if (tipo == (byte)TipoComando.TipoComando_Modo)
                {
                    return "Modo " + (dato + 1);
                }
                else if (tipo == (byte)TipoComando.TipoComando_Pinkie)
                {
                    if (dato == 0)
                        return "Pinkie Off";
                    else
                        return "Pinkie On";
                }
                else if (tipo == (byte)TipoComando.TipoComando_DxBoton)
                {
                    if (soltar)
                    {
                        return "Botón DX " + ((dato & 7) + 1) + " - " + ((dato >> 3) + 1) + " Off";
                    }
                    else
                    {
                        return "Botón DX " + ((dato & 7) + 1) + " - " + ((dato >> 3) + 1) + " On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_DxSeta)
                {
                    if (soltar)
                    {
                        return "Seta DX " + ((dato & 7) + 1) + " - " + (4 - ((dato >> 3)/ 8)) + "@" + (((dato >> 3) % 8) + 1) + " Off";
                    }
                    else
                    {
                        return "Seta DX " + ((dato & 7) + 1) + " - " + (4 - ((dato >> 3) / 8)) + "@" + (((dato >> 3) % 8) + 1) + " On";
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdLuz)
                {
                    return "Luz MFD " + dato;
                }
                else if (tipo == (byte)TipoComando.TipoComando_Luz)
                {
                    return "Luz Botones " + dato;
                }
                else if (tipo == (byte)TipoComando.TipoComando_InfoLuz)
                {
                    if (dato == 0)
                        return "Luz Info Off";
                    else
                        return "Luz Info On";
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdPinkie)
                {
                    if (dato == 0)
                        return "MFD Pinkie Off";
                    else
                        return "MFD Pinkie On";
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdTextoIni)
                {
                    string texto = "Línea de texto " + dato;
                    byte[] ascii = new byte[16];
                    byte i = 1;
                    while ((byte)(comando[i] & 0x7f) != (byte)TipoComando.TipoComando_MfdTextoFin)
                    {
                        ascii[i - 1] = (byte)(comando[i] >> 8);
                        i++;
                    }
                    string uni = System.Text.Encoding.Unicode.GetString(System.Text.Encoding.Convert(System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode, ascii));
                    uni = uni.Replace('ø', 'ñ').Replace('Ó', 'á').Replace('ß', 'í').Replace('Ô', 'ó').Replace('Ò', 'ú').Replace('£', 'Ñ').Replace('Ø', 'ª').Replace('×', 'º').Replace('ƒ', '¿').Replace('Ú', '¡');
                    return texto + " " + uni;
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdHora)
                {
                    if (dato == 1)
                    {
                        return "MFD Hora " + dato + " (AM/PM) " + (comando[1] >> 8) + ":" + (comando[2] >> 8);
                    }
                    else
                    {
                        return "MFD Hora " + dato + " (AM/PM) " + ((((comando[1] >> 8) * 256) + (comando[2] >> 8)) / 60) + ":" + ((((comando[1] >> 8) * 256) + (comando[2] >> 8)) % 60);
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdHora24)
                {
                    if (dato == 1)
                    {
                        return "MFD Hora " + dato + " (24H) " + (comando[1] >> 8) + ":" + (comando[2] >> 8);
                    }
                    else
                    {
                        return "MFD Hora " + dato + " (24H) " + ((((comando[1] >> 8) * 256) + (comando[2] >> 8)) / 60) + ":" + ((((comando[1] >> 8) * 256) + (comando[2] >> 8)) % 60);
                    }
                }
                else if (tipo == (byte)TipoComando.TipoComando_MfdFecha)
                {
                    return "MFD Fecha " + dato + ": " + (comando[1] >> 8);
                }
                else if (tipo == (byte)TipoComando.TipoComando_NxtLeds)
                {
                    string sled = (dato == 0) ? "B1" : ((dato == 11) ? "J1" : "J2");
                    string[] sorden = new string[] { "Off", "Cte", "It1", "It2", "It3", "Fl" };
                    string[] smodo = new string[] { "c1", "c2", "c1c2", "c2c1", "c1+c2", "c1+", "c2+" };

                    byte cmd = (byte)(comando[1] >> 8);
                    string rgb1 = (cmd & 0x7).ToString();
                    rgb1 += ((cmd >> 3) & 0x7).ToString();
                    rgb1 += ((cmd >> 6) | (((comando[2] >> 8) & 0x1) << 2)).ToString();

                    cmd = (byte)(comando[2] >> 8);
                    string rgb2 = ((cmd >> 1) & 0x7).ToString();
                    rgb2 += ((cmd >> 4) & 0x7).ToString();
                    rgb2 += ((((comando[3] >> 8) << 1) & 0x6) | (cmd >> 7)).ToString();

                    cmd = (byte)(comando[3] >> 8);
                    return $"NXT Led {sled}: {sorden[(cmd >> 2) & 0x7]} {smodo[(cmd >> 5) & 0x7]} {rgb1} {rgb2}";
                }

                return "";
            }
        }
    }
}

