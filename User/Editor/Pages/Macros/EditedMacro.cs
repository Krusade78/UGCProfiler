using System.Collections.Generic;
using System.Linq;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    internal class EditedMacro
    {
        private Avalonia.Controls.ListBox ListBox1;
        private readonly List<GroupedCommand> gMacros = [];
        private readonly Shared.ProfileModel profile = ((App)Avalonia.Application.Current).GetMainWindow().GetData().Profile;
        private ushort currentMacroId;

        public void SetListBox(Avalonia.Controls.ListBox lb) => ListBox1 = lb;

        public bool BasicMode { get; set; } = true;

        public void LoadData(ushort macroId, ref bool MFDChecked)
        {
            currentMacroId = macroId;
            gMacros.Clear();

            Shared.ProfileModel.MacroModel r = profile.Macros.Find(x => x.Id == macroId);

            List<uint> macros = [.. r.Commands];

            // Check X52 MFD name
            bool idc = false;
            if (macros.Count >= 2)
            {
                if (macros[0] == ((byte)CommandType.X52MfdTextIni + (3 << 8)))
                    idc = true;
            }
            if (idc)
            {
                bool nameOk = true;
                string st = r.Name;
                if (st.Length > 16)
                {
                    st = st[..16];
                }
                st = st.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));

                if (macros.Count >= (st.Length + 2))
                {
                    for (int i = 0; i < st.Length; i++)
                    {
                        ushort command = (ushort)((byte)CommandType.X52MfdText + (stb[i] << 8));
                        if (command != macros[i + 1])
                        {
                            nameOk = false;
                            break;
                        }
                    }
                    if ((macros[st.Length + 1] == (byte)CommandType.X52MfdTextEnd) && nameOk)
                    {
                        for (byte i = 0; i <= (st.Length + 1); i++)
                            macros.RemoveAt(0);

                        MFDChecked = true;
                    }
                }
            }
            LoadList(ref macros);

            RefrestListBox();
        }

        private void LoadList(ref List<uint> macros)
        {
            byte mtr = 0;

            List<uint> block = [];
            for (int i = 0; i < macros.Count; i++)
            {
                byte type = (byte)(macros[i] & 0x7f);

                if (type == (byte)CommandType.X52MfdTextIni)
                {
                    block.Add(macros[i]);
                }
                else if (type == (byte)CommandType.X52MfdText)
                {
                    block.Add(macros[i]);
                }
                else if (type == (byte)CommandType.X52MfdTextEnd)
                {
                    block.Add(macros[i]);
                    gMacros.Add(new() { Id = mtr++, Commands = [.. block] });
                    block.Clear();

                }
                else if (type == (byte)CommandType.X52MfdHour)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1], macros[i + 2]] });
                    i += 2;
                }
                else if (type == (byte)CommandType.X52MfdHour24)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1], macros[i + 2]] });
                    i += 2;
                }
                else if (type == (byte)CommandType.x52MfdDate)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1]] });
                    i++;
                }
                else if (type == (byte)CommandType.VkbGladiatorNxtLeds)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1], macros[i + 2], macros[i + 3]] });
                    i += 3;
                }
                else
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i]] });
                }
            }
        }

        private void RefrestListBox()
        {
            ListBox1.DataContext = gMacros.OrderBy(x => x.Id);
        }

        #region
        public bool LimitReached(byte nAdded)
        {
            int n = 0;
            foreach (GroupedCommand gc in gMacros)
            {
                n += gc.Commands.Length;
            }
            return n + nAdded + 18 > 255; //18 = x52 mfd txt + begin + end
        }

        public void Clear()
        {
            gMacros.Clear();
            RefrestListBox();
        }

        /// <summary>
        /// Insert over the selection
        /// </summary>
        /// <returns>returns insertion position</returns>
        private byte GetIndex()
        {
            if (gMacros.Count == 0)
            {
                return 0;
            }
            if (ListBox1.SelectedIndex == -1) //al final
            {
                return (byte)(gMacros[^1].Id + 1);
            }
            else
            {
                return ((GroupedCommand)ListBox1.SelectedItem).Id;
            }
        }

        public void Insert(uint[] block, bool blockTogether = false)
        {
            byte idBegin = GetIndex();

            //make room
            for (int i = gMacros.Count - 1; i >= 0; i--)
            {
                GroupedCommand rvSearch = gMacros[i];
                if (rvSearch.Id >= idBegin)
                {
                    if (!blockTogether)
                    {
                        rvSearch.Id += (byte)block.Length;
                    }
                    else
                    {
                        rvSearch.Id++;
                    }
                }
                else
                    break;
            }

            if (blockTogether)
            {
                gMacros.Add(new() { Id = idBegin, Commands = block });
            }
            else
            {
                byte c = 0;
                foreach (uint command in block)
                {
                    gMacros.Add(new() { Id = (byte)(idBegin + c++), Commands = [command] });
                }
            }
            RefrestListBox();
        }

        public void DeleteCommand()
        {
            if (ListBox1.SelectedIndex == -1)
                return;

            if (BasicMode)
            {
                Clear();
            }
            else
            {
                GroupedCommand rSel = (GroupedCommand)ListBox1.SelectedItem;
                byte cmdType = (byte)(rSel.Commands[0] & 0x7F);
                if (cmdType == (byte)CommandType.Repeat)
                {
                    if (((byte)(rSel.Commands[0] & 0xff) & (byte)CommandType.Release) == 0) //begin repeat
                    {
                        foreach (GroupedCommand rFind in gMacros)
                        {
                            if ((rFind.Id > rSel.Id) && ((byte)(rFind.Commands[0] & 0x7F) == (byte)CommandType.Repeat))
                            {
                                gMacros.Remove(rFind);
                                break;
                            }
                        }
                        gMacros.Remove(rSel);
                    }
                    else //end repeat
                    {
                        foreach (GroupedCommand rFind in gMacros)
                        {
                            if ((rFind.Id < rSel.Id) && //necessary because can be unordered in list
                                ((byte)(rFind.Commands[0] & 0x7F) == (byte)CommandType.Repeat))
                            {
                                gMacros.Remove(rFind);
                                break;
                            }
                        }
                        gMacros.Remove(rSel);
                    }
                }
                else if (cmdType == (byte)CommandType.RepeatN)
                {
                    if ((byte)(rSel.Commands[0] & 0xff & (byte)CommandType.Release) == 0)
                    {
                        byte nested = 1;
                        foreach (GroupedCommand rFind in gMacros)
                        {
                            if (rFind.Id > rSel.Id)
                            {
                                if ((byte)(rFind.Commands[0] & 0x7F) == (byte)CommandType.RepeatN)
                                {
                                    if ((rFind.Commands[0] & 0xff & (byte)CommandType.Release) != 0)
                                        nested--;
                                    else
                                        nested++;
                                }
                                if ((nested == 0) &&
                                        ((byte)(rFind.Commands[0] & 0x7F) == (byte)CommandType.RepeatN) &&
                                        ((byte)(rFind.Commands[0] & (byte)CommandType.Release) != 0)) //end repeatn
                                {
                                    gMacros.Remove(rFind);
                                    break;
                                }
                            }
                        }
                        gMacros.Remove(rSel);
                    }
                    else
                    {
                        byte nested = 1;
                        GroupedCommand rLast = null;
                        for (int i = rSel.Id - 1; i >= 0; i--)
                        {
                            GroupedCommand rFind = gMacros[i];
                            {
                                if ((byte)(rFind.Commands[0] & 0x7F) == (byte)CommandType.RepeatN)
                                {
                                    if ((rFind.Commands[0] & 0xff & (byte)CommandType.Release) != 0)
                                    {
                                        nested++;
                                    }
                                    else
                                    {
                                        nested--;
                                    }
                                }
                                if ((nested == 0) && ((byte)(rFind.Commands[0] & 0x7F) == (byte)CommandType.RepeatN) && ((rFind.Commands[0] & 0xff & (byte)CommandType.Release) == 0))
                                {
                                    rLast = rFind;
                                    break;
                                }
                            }
                        }
                        gMacros.Remove(rLast);
                        gMacros.Remove(rSel);
                    }
                }
                else
                {
                    gMacros.Remove(rSel);
                }

                //reorder
                byte id = 0;
                foreach (GroupedCommand gc in gMacros)
                {
                    if (gc.Id != id)
                    {
                        gc.Id = id;
                    }
                    id++;
                }
            }
            RefrestListBox();
        }

        public void MoveUpCommand()
        {
            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == 0))
                return;

            GroupedCommand rSel = (GroupedCommand)ListBox1.SelectedItem;
            GroupedCommand rBefore = gMacros.Find(x => x.Id == (byte)(rSel.Id - 1));
            byte selType = (byte)(rSel.Commands[0] & 0x7f);
            byte beforeType = (byte)(rBefore.Commands[0] & 0x7f);

            if ((selType == (byte)CommandType.Hold) &&
                (beforeType == (byte)CommandType.RepeatN) && ((byte)(rBefore.Commands[0] & (byte)CommandType.Release) != 0)) //end repeatn
            {
                return;
            }
            if ((selType == (byte)CommandType.Repeat) &&   //any repeat
                ((beforeType == (byte)CommandType.RepeatN) || (beforeType == (byte)CommandType.Repeat)))
            {
                return;
            }
            if ((selType == (byte)CommandType.RepeatN) && //begin repeat
                ((beforeType == (byte)CommandType.RepeatN) || (beforeType == (byte)CommandType.Repeat)))
            {
                return;
            }


            int sel = ListBox1.SelectedIndex;

            byte idBefore = rBefore.Id;
            rBefore.Id = 255;
            rSel.Id = idBefore;
            rBefore.Id = (byte)(idBefore + 1);

            RefrestListBox();
            ListBox1.SelectedIndex = sel - 1;
        }

        public void MoveDownCommand()
        {
            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == (ListBox1.Items.Count - 1)))
                return;

            GroupedCommand rSel = (GroupedCommand)ListBox1.SelectedItem;
            GroupedCommand rNext = gMacros.Find(x => x.Id == (byte)(rSel.Id + 1));
            byte selType = (byte)(rSel.Commands[0] & 0x7f);
            byte nextType = (byte)(rNext.Commands[0] & 0x7f);

            if ((selType == (byte)CommandType.Hold) && (nextType == (byte)CommandType.RepeatN))
            {
                return;
            }
            if ((selType == (byte)CommandType.Repeat) &&
                ((nextType == (byte)CommandType.RepeatN) || (nextType == (byte)CommandType.Repeat)))
            {
                return;
            }
            if ((selType == (byte)CommandType.RepeatN) &&
                ((nextType == (byte)CommandType.RepeatN) || (nextType == (byte)CommandType.Repeat)))
            {
                return;
            }

            int sel = ListBox1.SelectedIndex;

            byte idNext = rNext.Id;
            rNext.Id = 255;
            rSel.Id = idNext;
            rNext.Id = (byte)(idNext - 1);

            RefrestListBox();
            ListBox1.SelectedIndex = sel + 1;
        }
        #endregion

        public async System.Threading.Tasks.Task<bool> CheckHoldWithRepeat()
        {
            foreach (GroupedCommand r in gMacros)
            {
                byte type = (byte)(r.Commands[0] & 0x7f);
                if ((type == (byte)CommandType.Repeat) || (type == (byte)CommandType.Hold))
                {
                    await MessageBox.Show(Translate.Get("repeat_and_hold_must_be_unique"), Translate.Get("warning"), MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }

            if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == 0))
            {
                return true;
            }

            int reps = 0;
            foreach (GroupedCommand rv in ListBox1.Items)
            {
                if (rv == (GroupedCommand)ListBox1.SelectedItem)
                {
                    break;
                }
                byte tipo = (byte)(rv.Commands[0] & 0xff);
                if ((CommandType)(tipo & 0x7f) == CommandType.RepeatN)
                {
                    if ((tipo & (byte)CommandType.Release) != 0)
                        reps--;
                    else
                        reps++;
                }
            }
            if (reps != 0)
            {
                await MessageBox.Show(Translate.Get("repeat_and_hold_cannot_be_inside_repeat_n"), Translate.Get("warning"), MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        public async void Save(bool nameOnMFD, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            else
            {
                foreach (Shared.ProfileModel.MacroModel r in profile.Macros)
                {
                    if ((r.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) && (r.Id != currentMacroId))
                    {
                        _ = await MessageBox.Show(Translate.Get("macro_name_repeated"), Translate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            List<uint> macro = [];

            if (nameOnMFD) //x52 text
            {
                string st = name;
                if (st.Length > 16)
                {
                    st = st[..16];
                }
                st = st.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
                byte[] text = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));

                macro.Add((byte)CommandType.X52MfdTextIni + (3 << 8));
                for (byte i = 0; i < text.Length; i++)
                {
                    macro.Add((uint)((byte)CommandType.X52MfdText + (text[i] << 8)));
                }
                macro.Add((byte)CommandType.X52MfdTextEnd);
            }

            foreach (GroupedCommand gc in gMacros.OrderBy(x => x.Id))
            {
                foreach (uint c in gc.Commands)
                {
                    macro.Add(c);
                }
            }
           
            Shared.ProfileModel.MacroModel nr = profile.Macros.Find(x => x.Id == currentMacroId);
            nr.Name = name;
            nr.Commands = [.. macro];
        }
    }
}
