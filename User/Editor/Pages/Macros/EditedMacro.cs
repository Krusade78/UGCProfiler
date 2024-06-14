using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Linq;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    internal class EditedMacro
    {
        private Microsoft.UI.Xaml.Controls.ListBox ListBox1;
        private readonly List<GroupedCommand> gMacros = [];
        private readonly Shared.ProfileModel profile = ((App)Application.Current).GetMainWindow().GetData().Profile;

        public void SetListBox(Microsoft.UI.Xaml.Controls.ListBox lb) => ListBox1 = lb;

        public void LoadData(ushort macroId, ref bool MFDChecked)
        {
            gMacros.Clear();
            //dsMacros.MACROS.DefaultView.Sort = "id";
            //ListBox1.DataContext = dsMacros.MACROS.DefaultView;

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
                bool nombreOk = true;
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
                        ushort comando = (ushort)((byte)CommandType.X52MfdText + (stb[i] << 8));
                        if (comando != macros[i + 1])
                        {
                            nombreOk = false;
                            break;
                        }
                    }
                    if ((macros[st.Length + 1] == (byte)CommandType.X52MfdTextEnd) && nombreOk)
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

            List<uint> bloque = [];
            for (int i = 0; i < macros.Count; i++)
            {
                byte tipo = (byte)(macros[i] & 0x7f);

                if (tipo == (byte)CommandType.X52MfdTextIni)
                {
                    bloque.Add(macros[i]);
                }
                else if (tipo == (byte)CommandType.X52MfdText)
                {
                    bloque.Add(macros[i]);
                }
                else if (tipo == (byte)CommandType.X52MfdTextEnd)
                {
                    bloque.Add(macros[i]);
                    gMacros.Add(new() { Id = mtr++, Commands = [.. bloque] });
                    bloque.Clear();

                }
                else if (tipo == (byte)CommandType.X52MfdHour)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1], macros[i + 2]] });
                    i += 2;
                }
                else if (tipo == (byte)CommandType.X52MfdHour24)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1], macros[i + 2]] });
                    i += 2;
                }
                else if (tipo == (byte)CommandType.x52MfdDate)
                {
                    gMacros.Add(new() { Id = mtr++, Commands = [macros[i], macros[i + 1]] });
                    i++;
                }
                else if (tipo == (byte)CommandType.VkbGladiatorNxtLeds)
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
        public int GetCuenta()
        {
            int n = 0;
            foreach (GroupedCommand gc in gMacros)
            {
                n += gc.Commands.Length;
            }
            return n;
        }

        public void Clear()
        {
            gMacros.Clear();
            RefrestListBox();
        }

        /// <summary>
        /// Se inserta por encima de la selección
        /// </summary>
        /// <returns>Devuelve la posición de inserción</returns>
        private byte GetIndice()
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

        public void Insertar(uint[] block, bool separated)
        {
            byte idInicio = GetIndice();

            //hacer hueco
            for (int i = gMacros.Count - 1; i >= 0; i--)
            {
                GroupedCommand rvBusca = gMacros[i];
                if (rvBusca.Id >= idInicio)
                {
                    if (separated)
                    {
                        rvBusca.Id += (byte)block.Length;
                    }
                    else
                    {
                        rvBusca.Id++;
                    }
                }
                else
                    break;
            }

            if (separated)
            {
                byte c = 0;
                foreach (uint comando in block)
                {
                    gMacros.Add(new() { Id = (byte)(idInicio + c++), Commands = [comando] });
                }
            }
            else
            {
                gMacros.Add(new() { Id = idInicio, Commands = block });
            }
            RefrestListBox();
        }

        //private void BorrarMacroLista()
        //{
        //	if (ListBox1.SelectedIndex == -1)
        //		return;

        //	if (RadioButtonAvanzado.IsChecked != true)
        //	{
        //		if (RadioButtonBasico.IsChecked == true)
        //		{
        //			CurrentMacro.MACROS.Clear();
        //		}
        //	}
        //	else
        //	{
        //		DataSetMacros.MACROSRow rsel = (DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row;
        //		byte tipo = (byte)(rsel.comando[0] & 0x7F);
        //		if (tipo == (byte)CommandType.Repeat)
        //		{
        //			if (((byte)(rsel.comando[0] & 0xff) & (byte)CommandType.Release) == 0) //inicio repeat
        //			{
        //				foreach (DataSetMacros.MACROSRow rBusca in CurrentMacro.MACROS)
        //				{
        //					if ((rBusca.id > rsel.id) &&  
        //						((byte)(rBusca.comando[0] & 0x7F) == (byte)CommandType.Repeat))
        //					{
        //						rBusca.Delete();
        //						break;
        //					}
        //				}
        //				rsel.Delete();
        //			}
        //			else //fin repeat
        //			{
        //				foreach (DataSetMacros.MACROSRow rBusca in CurrentMacro.MACROS)
        //				{
        //					if ((rBusca.id < rsel.id) && //hace falta porque en la tabla puede estar desordenado
        //						((byte)(rBusca.comando[0] & 0x7F) == (byte)CommandType.Repeat))
        //					{
        //						rBusca.Delete();
        //						break;
        //					}
        //				}
        //				rsel.Delete();
        //			}
        //		}
        //		else if (tipo == (byte)CommandType.RepeatN)
        //		{
        //			if ((byte)(rsel.comando[0] & 0xff & (byte)CommandType.Release) == 0)
        //			{
        //				byte anidado = 1;
        //				foreach (System.Data.DataRowView rvBusca in CurrentMacro.MACROS.DefaultView)
        //				{
        //					DataSetMacros.MACROSRow rBusca = (DataSetMacros.MACROSRow)rvBusca.Row;
        //					if (rBusca.id > rsel.id)
        //					{
        //						if ((byte)(rBusca.comando[0] & 0x7F) == (byte)CommandType.RepeatN)
        //						{
        //							if ((rBusca.comando[0] & 0xff & (byte)CommandType.Release) != 0)
        //								anidado--;
        //							else
        //								anidado++;
        //						}
        //						if ((anidado == 0) &&
        //								((byte)(rBusca.comando[0] & 0x7F) == (byte)CommandType.RepeatN) &&
        //								((byte)(rBusca.comando[0] & (byte)CommandType.Release) != 0)) //fin repeatn
        //						{
        //							rBusca.Delete();
        //							break;
        //						}
        //					}
        //				}
        //				rsel.Delete();
        //			}
        //			else
        //			{
        //				byte anidado = 1;
        //				DataSetMacros.MACROSRow rUltima = null;
        //				for (int i = rsel.id - 1; i >= 0; i--)
        //				{
        //					System.Data.DataRowView rvBusca = CurrentMacro.MACROS.DefaultView[i];
        //					DataSetMacros.MACROSRow rBusca = (DataSetMacros.MACROSRow)rvBusca.Row;
        //					{
        //						if ((byte)(rBusca.comando[0] & 0x7F) == (byte)CommandType.RepeatN)
        //						{
        //							if ((rBusca.comando[0] & 0xff & (byte)CommandType.Release) != 0)
        //								anidado++;
        //							else
        //								anidado--;
        //						}
        //						if ((anidado == 0) &&
        //							((byte)(rBusca.comando[0] & 0x7F) == (byte)CommandType.RepeatN) &&
        //							((rBusca.comando[0] & 0xff & (byte)CommandType.Release) == 0))
        //						{
        //							rUltima = rBusca;
        //							break;
        //						}
        //					}
        //				}
        //				rUltima.Delete();
        //				rsel.Delete();
        //			}
        //		}
        //		else
        //		{
        //			rsel.Delete();
        //		}

        //		//reordenar
        //		byte id = 0;
        //		foreach (System.Data.DataRowView rvBusca in CurrentMacro.MACROS.DefaultView)
        //		{
        //			DataSetMacros.MACROSRow rBusca = (DataSetMacros.MACROSRow)rvBusca.Row;
        //			if (rBusca.id != id)
        //			{
        //				rBusca.id = id;
        //			}
        //			id++;
        //		}
        //	}
        //}

        //private void SubirMacroLista()
        //{
        //	if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == 0))
        //		return;

        //	DataSetMacros.MACROSRow rSel = (DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row;
        //	DataSetMacros.MACROSRow rAnterior = CurrentMacro.MACROS.FindByid((byte)(rSel.id - 1));
        //	byte tipoSel = (byte)(rSel.comando[0] & 0x7f);
        //	byte tipoAnterior = (byte)(rAnterior.comando[0] & 0x7f);

        //	if ((tipoSel  == (byte)CommandType.Hold) && 
        //		(tipoAnterior == (byte)CommandType.RepeatN) && ((byte)(rAnterior.comando[0] & (byte)CommandType.Release) != 0)) //fin repeatn
        //	{
        //		return;
        //	}
        //	if ((tipoSel == (byte)CommandType.Repeat) &&   //cualquier repeat
        //		((tipoAnterior == (byte)CommandType.RepeatN) || (tipoAnterior == (byte)CommandType.Repeat)))
        //	{
        //		return;
        //	}
        //	if ((tipoSel == (byte)CommandType.RepeatN) && //inicio repeat
        //		((tipoAnterior == (byte)CommandType.RepeatN) || (tipoAnterior == (byte)CommandType.Repeat)))
        //	{
        //		return;
        //	}


        //	int sel = ListBox1.SelectedIndex;

        //	byte idAnterior = rAnterior.id;
        //	rAnterior.id = 255;
        //	rSel.id = idAnterior;
        //	rAnterior.id = (byte)(idAnterior + 1);

        //	ListBox1.SelectedIndex = sel - 1;
        //}

        //private void BajarMacroLista()
        //{
        //	if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == (ListBox1.Items.Count - 1)))
        //		return;

        //	DataSetMacros.MACROSRow rSel = (DataSetMacros.MACROSRow)((System.Data.DataRowView)ListBox1.SelectedItem).Row;
        //	DataSetMacros.MACROSRow rSiguiente = CurrentMacro.MACROS.FindByid((byte)(rSel.id + 1));
        //	byte tipoSel = (byte)(rSel.comando[0] & 0x7f);
        //	byte tipoSiguiente = (byte)(rSiguiente.comando[0] & 0x7f);

        //	if ((tipoSel == (byte)CommandType.Hold) && (tipoSiguiente == (byte)CommandType.RepeatN))
        //	{
        //		return;
        //	}
        //	if ((tipoSel == (byte)CommandType.Repeat) &&
        //		((tipoSiguiente == (byte)CommandType.RepeatN) || (tipoSiguiente == (byte)CommandType.Repeat)))
        //	{
        //		return;
        //	}
        //	if ((tipoSel == (byte)CommandType.RepeatN) &&
        //		((tipoSiguiente == (byte)CommandType.RepeatN) || (tipoSiguiente == (byte)CommandType.Repeat)))
        //	{
        //		return;
        //	}

        //	int sel = ListBox1.SelectedIndex;

        //	byte idSiguiente = rSiguiente.id;
        //	rSiguiente.id = 255;
        //	rSel.id = idSiguiente;
        //	rSiguiente.id = (byte)(idSiguiente - 1);

        //	ListBox1.SelectedIndex = sel + 1;
        //}
        #endregion

        //private void Guardar()
        //{
        //	if (TextBoxNombre.Text.Trim() == "")
        //		return;
        //	else
        //	{
        //		foreach (Comunes.DSPerfil.ACCIONESRow r in parent.GetData().Profile.ACCIONES.Rows)
        //		{
        //			if ((r.Nombre == TextBoxNombre.Text.Trim()) && (r.idAccion != indicep))
        //			{
        //				_ = MessageBox.Show("El de nombre de la macro está repetido.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //				return;
        //			}
        //		}
        //	}

        //	List<ushort> macro = [];

        //	if (CheckBox1.IsChecked == true) //texto x52
        //	{
        //		string st = TextBoxNombre.Text.Trim();
        //		if (st.Length > 16)
        //		{
        //			st = st[..16];
        //		}
        //		st = st.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
        //		byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));

        //		macro.Add((byte)CommandType.X52MfdTextIni + (3 << 8));
        //		for (int i = 0; i < texto.Length; i++)
        //		{
        //			macro.Add((ushort)((byte)CommandType.X52MfdText + (texto[i] << 8)));
        //		}
        //		macro.Add((byte)CommandType.X52MfdTextEnd);
        //	}

        //	foreach (System.Data.DataRowView rv in CurrentMacro.MACROS.DefaultView)
        //	{
        //		foreach (ushort c in ((DataSetMacros.MACROSRow)rv.Row).comando)
        //		{
        //			macro.Add(c);
        //		}
        //	}

        //	if (indicep == -1)
        //	{
        //		ushort idnuevo = 0;
        //		foreach (Comunes.DSPerfil.ACCIONESRow r in parent.GetData().Profile.ACCIONES.Rows)
        //		{
        //			if (r.idAccion > idnuevo)
        //				idnuevo = r.idAccion;
        //		}
        //		idnuevo++;

        //		parent.GetData().Profile.ACCIONES.AddACCIONESRow(idnuevo, TextBoxNombre.Text.Trim(), [.. macro]);
        //		this.DialogResult = null;
        //	}
        //	else
        //	{
        //		Comunes.DSPerfil.ACCIONESRow nr = parent.GetData().Profile.ACCIONES.FindByidAccion((ushort)indicep);
        //		nr.Nombre = TextBoxNombre.Text.Trim();
        //		nr.Comandos = [.. macro];
        //		this.DialogResult = true;
        //	}
        //}
    }
}
