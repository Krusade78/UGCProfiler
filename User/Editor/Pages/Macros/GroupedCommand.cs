using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public class GroupedCommand
    {
        public byte Id { get; set; }
        public string Name { get => GetName(Commands); }
        public uint[] Commands { get; set; }

        public static System.Collections.Generic.List<string> Keys { get; set; } = []; //key names ordered by key code

        private static string GetName(uint[] comands)
        {
            byte cmdType = (byte)(comands[0] & 0x7f);
            byte data = (byte)((comands[0] >> 8) & 0xff);
            byte joy = (byte)((comands[0] >> 16) & 0xff);
            bool released = (comands[0] & 0xff & (byte)CommandType.Release) == (byte)CommandType.Release;

            if (cmdType == (byte)CommandType.Key)
            {
                string key = Keys[data];
                if (released)
                {
                    return "Soltar " + key.Remove(0, 5);
                }
                else
                {
                    return "Presionar " + key.Remove(0, 5);
                }
            }
            else if (cmdType == (byte)CommandType.MouseBt1)
            {
                if (released)
                {
                    return "Ratón->Botón 1 Off";
                }
                else
                {
                    return "Ratón->Botón 1 On";
                }
            }
            else if (cmdType == (byte)CommandType.MouseBt2)
            {
                if (released)
                {
                    return "Ratón->Botón 2 Off";
                }
                else
                {
                    return "Ratón->Botón 2 On";
                }
            }
            else if (cmdType == (byte)CommandType.MouseBt3)
            {
                if (released)
                {
                    return "Ratón->Botón 3 Off";
                }
                else
                {
                    return "Ratón->Botón 3 On";
                }
            }
            else if (cmdType == (byte)CommandType.MouseLeft)
            {
                if (released)
                {
                    return "Ratón->Izquierda 0";
                }
                else
                {
                    return "Ratón->Izquierda " + data;
                }
            }
            else if (cmdType == (byte)CommandType.MouseRight)
            {
                if (released)
                {
                    return "Ratón->Derecha 0";
                }
                else
                {
                    return "Ratón->Derecha " + data;
                }
            }
            else if (cmdType == (byte)CommandType.MouseDown)
            {
                if (released)
                {
                    return "Ratón->Abajo 0";
                }
                else
                {
                    return "Ratón->Abajo " + data;
                }
            }
            else if (cmdType == (byte)CommandType.MouseUp)
            {
                if (released)
                {
                    return "Ratón->Arriba 0";
                }
                else
                {
                    return "Ratón->Arriba " + data;
                }
            }
            else if (cmdType == (byte)CommandType.MouseWhUp)
            {
                if (released)
                {
                    return "Ratón->Rueda arriba Off";
                }
                else
                {
                    return "Ratón->Rueda arriba On";
                }
            }
            else if (cmdType == (byte)CommandType.MouseWhDown)
            {
                if (released)
                {
                    return "Ratón->Rueba abajo Off";
                }
                else
                {
                    return "Ratón->Rueda abajo On";
                }
            }
            else if (cmdType == (byte)CommandType.Delay)
            {
                return "Pausa " + data;
            }
            else if (cmdType == (byte)CommandType.Hold)
            {
                return "Mantener";
            }
            else if (cmdType == (byte)CommandType.Repeat)
            {
                if (released)
                    return "/-- Repetir Fin";
                else
                    return "/-- Repetir Inicio";
            }
            else if (cmdType == (byte)CommandType.RepeatN)
            {
                if (released)
                    return "/-- Repetir N Fin";
                else
                    return "/-- Repetir N[" + data + "] Inicio";
            }
            else if (cmdType == (byte)CommandType.PrecisionMode)
            {
                string[] ejes = ["X", "Y", "Z", "Rx", "Ry", "Rz", "Sl1", "Sl2"];
                if (!released)
                    return $"Modo preciso J{((data & 31) / 8) + 1} {ejes[(data & 31) % 8]} s{(data >> 5) + 1} On";
                else
                    return $"Modo preciso J{((data & 31) / 8) + 1} {ejes[(data & 31) % 8]} Off";
            }
            else if (cmdType == (byte)CommandType.Mode)
            {
                return "Modo " + (data + 1);
            }
            else if (cmdType == (byte)CommandType.SubMode)
            {
                if (data == 0)
                    return "Pinkie Off";
                else
                    return "Pinkie On";
            }
            else if (cmdType == (byte)CommandType.DxButton)
            {
                if (released)
                {
                    return $"Botón vJoy. {joy + 1} -  DX {data + 1} Off";
                }
                else
                {
                    return $"Botón vJoy. {joy + 1} - DX {data + 1} On";
                }
            }
            else if (cmdType == (byte)CommandType.DxHat)
            {
                if (released)
                {
                    return $"Seta vJoy. {joy + 1} - DX {4 - (data / 8)} @ {(data % 8) + 1} Off";
                }
                else
                {
                    return $"Seta vJoy. {joy + 1} - DX {4 - (data / 8)} @ {(data % 8) + 1} On";
                }
            }
            else if (cmdType == (byte)CommandType.X52MfdLight)
            {
                return "Luz MFD " + data;
            }
            else if (cmdType == (byte)CommandType.X52Light)
            {
                return "Luz Botones " + data;
            }
            else if (cmdType == (byte)CommandType.X52InfoLight)
            {
                if (data == 0)
                    return "Luz Info Off";
                else
                    return "Luz Info On";
            }
            else if (cmdType == (byte)CommandType.X52MfdPinkie)
            {
                if (data == 0)
                    return "MFD Pinkie Off";
                else
                    return "MFD Pinkie On";
            }
            else if (cmdType == (byte)CommandType.X52MfdTextIni)
            {
                string texto = "Línea de texto " + data;
                byte[] ascii = new byte[16];
                byte i = 1;
                while ((byte)(comands[i] & 0x7f) != (byte)CommandType.X52MfdTextEnd)
                {
                    ascii[i - 1] = (byte)(comands[i] >> 8);
                    i++;
                }
                string uni = System.Text.Encoding.Unicode.GetString(System.Text.Encoding.Convert(System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode, ascii));
                uni = uni.Replace('ø', 'ñ').Replace('Ó', 'á').Replace('ß', 'í').Replace('Ô', 'ó').Replace('Ò', 'ú').Replace('£', 'Ñ').Replace('Ø', 'ª').Replace('×', 'º').Replace('ƒ', '¿').Replace('Ú', '¡');
                return texto + " " + uni;
            }
            else if (cmdType == (byte)CommandType.X52MfdHour)
            {
                if (data == 1)
                {
                    return "MFD Hora " + data + " (AM/PM) " + (comands[1] >> 8) + ":" + (comands[2] >> 8);
                }
                else
                {
                    return "MFD Hora " + data + " (AM/PM) " + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) / 60) + ":" + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) % 60);
                }
            }
            else if (cmdType == (byte)CommandType.X52MfdHour24)
            {
                if (data == 1)
                {
                    return "MFD Hora " + data + " (24H) " + (comands[1] >> 8) + ":" + (comands[2] >> 8);
                }
                else
                {
                    return "MFD Hora " + data + " (24H) " + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) / 60) + ":" + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) % 60);
                }
            }
            else if (cmdType == (byte)CommandType.x52MfdDate)
            {
                return "MFD Fecha " + data + ": " + (comands[1] >> 8);
            }
            else if (cmdType == (byte)CommandType.VkbGladiatorNxtLeds)
            {
                string sled = (data == 0) ? "B1" : ((data == 11) ? "J1" : "J2");
                string[] sorden = ["Off", "Cte", "It1", "It2", "It3", "Fl"];
                string[] smodo = ["c1", "c2", "c1c2", "c2c1", "c1+c2", "c1+", "c2+"];

                byte cmd = (byte)(comands[1] >> 8);
                string rgb1 = (cmd & 0x7).ToString();
                rgb1 += ((cmd >> 3) & 0x7).ToString();
                rgb1 += ((cmd >> 6) | (((comands[2] >> 8) & 0x1) << 2)).ToString();

                cmd = (byte)(comands[2] >> 8);
                string rgb2 = ((cmd >> 1) & 0x7).ToString();
                rgb2 += ((cmd >> 4) & 0x7).ToString();
                rgb2 += ((((comands[3] >> 8) << 1) & 0x6) | (cmd >> 7)).ToString();

                cmd = (byte)(comands[3] >> 8);
                return $"NXT Led {sled}: {sorden[(cmd >> 2) & 0x7]} {smodo[(cmd >> 5) & 0x7]} {rgb1} {rgb2}";
            }

            return "";
        }
    }
}