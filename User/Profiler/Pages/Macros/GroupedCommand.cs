using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public class GroupedCommand
    {
        public byte Id { get; set; }
        public string Name { get => GetName(Commands); }
        public uint[] Commands { get; set; } = [];

        public static System.Collections.Generic.List<string> Keys { get; set; } = []; //key names ordered by key code

        private static string GetName(uint[] comands)
        {
            byte cmdType = (byte)(comands[0] & 0x7f);
            byte data1 = (byte)((comands[0] >> 8) & 0xff);
            byte data2 = (byte)((comands[0] >> 16) & 0xff);
            byte extra = (byte)((comands[0] >> 24) & 0xf);
            byte joy = (byte)(comands[0] >> 28);
            bool released = (comands[0] & 0xff & (byte)CommandType.Release) == (byte)CommandType.Release;

            if (cmdType == (byte)CommandType.Key)
            {
                string key = Keys[data1];
                if (released)
                {
                    return $"{Translate.Get("release")} {key[5..]}";
                }
                else
                {
                    return $"{Translate.Get("press")} {key[5..]}";
                }
            }
            else if (cmdType == (byte)CommandType.MouseBt1)
            {
                if (released)
                {
                    return Translate.Get("mouse_button1_off");
                }
                else
                {
                    return Translate.Get("mouse_button1_on");
                }
            }
            else if (cmdType == (byte)CommandType.MouseBt2)
            {
                if (released)
                {
                    return Translate.Get("mouse_button2_off");
                }
                else
                {
                    return Translate.Get("mouse_button2_on");
                }
            }
            else if (cmdType == (byte)CommandType.MouseBt3)
            {
                if (released)
                {
                    return Translate.Get("mouse_button3_off");
                }
                else
                {
                    return Translate.Get("mouse_button3_on");
                }
            }
            else if (cmdType == (byte)CommandType.MouseLeft)
            {
                if (released)
                {
                    return Translate.Get("mouse_left_0");
                }
                else
                {
                    return $"{Translate.Get("mouse_left")} {data1}";
                }
            }
            else if (cmdType == (byte)CommandType.MouseRight)
            {
                if (released)
                {
                    return Translate.Get("mouse_right_0");
                }
                else
                {
                    return $"{Translate.Get("mouse_right")} {data1}";
                }
            }
            else if (cmdType == (byte)CommandType.MouseDown)
            {
                if (released)
                {
                    return Translate.Get("mouse_down_0");
                }
                else
                {
                    return $"{Translate.Get("mouse_down")} {data1}";
                }
            }
            else if (cmdType == (byte)CommandType.MouseUp)
            {
                if (released)
                {
                    return Translate.Get("mouse_up_0");
                }
                else
                {
                    return $"{Translate.Get("mouse_up")} {data1}";
                }
            }
            else if (cmdType == (byte)CommandType.MouseWhUp)
            {
                if (released)
                {
                    return Translate.Get("mouse_wheel_up_off");
                }
                else
                {
                    return Translate.Get("mouse_wheel_up_on");
                }
            }
            else if (cmdType == (byte)CommandType.MouseWhDown)
            {
                if (released)
                {
                    return Translate.Get("mouse_wheel_down_off");
                }
                else
                {
                    return Translate.Get("mouse_wheel_down_on");
                }
            }
            else if (cmdType == (byte)CommandType.Delay)
            {
                return $"{Translate.Get("pause")} {data1}";
            }
            else if (cmdType == (byte)CommandType.Hold)
            {
                return Translate.Get("hold");
            }
            else if (cmdType == (byte)CommandType.Repeat)
            {
                if (released)
                    return Translate.Get("repeat_end");
                else
                    return Translate.Get("repeat_begin");
            }
            else if (cmdType == (byte)CommandType.RepeatN)
            {
                if (released)
                    return $"{Translate.Get("repeat_n(slash)")} {Translate.Get("end")}";
                else
                    return $"{Translate.Get("repeat_n(slash)")}[{data1}] {Translate.Get("begin")}";
            }
            else if (cmdType == (byte)CommandType.PrecisionMode)
            {
                string[] ejes = ["X", "Y", "Z", "Rx", "Ry", "Rz", "Sl1", "Sl2"];
                if (!released)
                    return $"[{Translate.Get("precision_mode")}] J{((data1 & 31) / 8) + 1} {ejes[(data1 & 31) % 8]} s{(data1 >> 5) + 1} On";
                else
                    return $"[{Translate.Get("precision_mode")}] J{((data1 & 31) / 8) + 1} {ejes[(data1 & 31) % 8]} Off";
            }
            else if (cmdType == (byte)CommandType.Mode)
            {
                return $"{Translate.Get("mode")} {data1 + 1}";
            }
            else if (cmdType == (byte)CommandType.SubMode)
            {
                return $"{Translate.Get("submode")} {data1 + 1}";
            }
            else if (cmdType == (byte)CommandType.DxButton)
            {
                if (released)
                {
                    return $"[{Translate.Get("button")}] vJoy {joy + 1} @ {data1 + 1} Off";
                }
                else
                {
                    return $"[{Translate.Get("button")}] vJoy {joy + 1} @ {data1 + 1} On";
                }
            }
            else if (cmdType == (byte)CommandType.DxHat)
            {
                if (released)
                {
                    return $"[{Translate.Get("hat")}] vJoy {joy + 1} - {data2 + 1} @ {(data1 % 8) + 1} Off";
                }
                else
                {
                    return $"[{Translate.Get("hat")}] vJoy {joy + 1} - {data2 + 1} @ {(data1 % 8) + 1} On";
                }
            }
            else if (cmdType == (byte)CommandType.DxAxis)
            {
                ushort move = (ushort)((data2 << 8) + data1);
                move *= (ushort)((extra & 1) == 1 ? -1 : 1);
                string[] axes = ["X", "Y", "Z", "Rx", "Ry", "Rz", "Sl1", "Sl2"];
                return $"[{Translate.Get("axis")}] vJoy {joy + 1} - {axes[extra >> 1]} {(move > 0 ? '+' : "")}{move}";
            }
            else if (cmdType == (byte)CommandType.X52MfdLight)
            {
                return $"[X52] {Translate.Get("mfd_light")} {data1}";
            }
            else if (cmdType == (byte)CommandType.X52Light)
            {
                return $"[X52] {Translate.Get("buttons_light")} {data1}";
            }
            else if (cmdType == (byte)CommandType.X52InfoLight)
            {
                if (data1 == 0)
                    return $"[X52] {Translate.Get("info_light")} Off";
                else
                    return $"[X52] {Translate.Get("info_light")} On";
            }
            else if (cmdType == (byte)CommandType.X52MfdPinkie)
            {
                if (data1 == 0)
                    return "[X52] MFD Pinkie Off";
                else
                    return "[X52] MFD Pinkie On";
            }
            else if (cmdType == (byte)CommandType.X52MfdTextIni)
            {
                string texto = $"[X52] {Translate.Get("text_line")} {data1}";
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
                if (data1 == 1)
                {
                    return $"[X52] {Translate.Get("mfd_hour")} {data1} (AM/PM) " + (comands[1] >> 8) + ":" + (comands[2] >> 8);
                }
                else
                {
                    return $"[X52] {Translate.Get("mfd_hour")} {data1} (AM/PM) " + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) / 60) + ":" + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) % 60);
                }
            }
            else if (cmdType == (byte)CommandType.X52MfdHour24)
            {
                if (data1 == 1)
                {
                    return $"[X52] {Translate.Get("mfd_hour")} {data1} (24H) " + (comands[1] >> 8) + ":" + (comands[2] >> 8);
                }
                else
                {
                    return $"[X52] {Translate.Get("mfd_hour")} {data1} (24H) " + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) / 60) + ":" + ((((comands[1] >> 8) * 256) + (comands[2] >> 8)) % 60);
                }
            }
            else if (cmdType == (byte)CommandType.x52MfdDate)
            {
                return $"[X52] {Translate.Get("mfd_date")} {data1}: {(comands[1] >> 8)}";
            }
            else if (cmdType == (byte)CommandType.VkbGladiatorNxtLeds)
            {
                string sled = (data1 == 0) ? "B1" : ((data1 == 11) ? "J1" : "J2");
                string[] sorden = ["Off", "Ct", "It1", "It2", "It3", "Fl"];
                string[] smodo = ["c1", "c2", "c1c2", "c2c1", "c1+c2", "c1+", "c2+"];

                byte cmd = (byte)(comands[1] >> 8);
                string rgb1 = (cmd & 0x7).ToString();
                rgb1 += ((cmd >> 3) & 0x7).ToString();
                rgb1 += ((cmd >> 6) | (byte)(((comands[2] >> 8) & 0x1) << 2)).ToString();

                cmd = (byte)(comands[2] >> 8);
                string rgb2 = ((cmd >> 1) & 0x7).ToString();
                rgb2 += ((cmd >> 4) & 0x7).ToString();
                rgb2 += ((byte)(((comands[3] >> 8) << 1) & 0x6) | (cmd >> 7)).ToString();

                cmd = (byte)(comands[3] >> 8);
                return $"NXT Led {sled}: {sorden[(cmd >> 2) & 0x7]} {smodo[(cmd >> 5) & 0x7]} {rgb1} {rgb2}";
            }

            return "";
        }
    }
}