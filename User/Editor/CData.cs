using System;
using System.Windows;
using System.Collections.Generic;
using Shared;
using System.IO.Packaging;
//using System.Windows.Media.Imaging;
using System.Data.Common;

namespace Profiler
{
	class CDatos : IDisposable
	{
		public ProfileModel Profile { get; private set; } = new();
		public bool Modified { get; set; } = false;

		#region IDisposable Support
		private bool disposedValue = false; // Para detectar llamadas redundantes

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Profile.AxesMap.Clear();
					Profile.ButtonsMap.Clear();
					Profile.Macros.Clear();
				}

				disposedValue = true;
			}
		}
		void IDisposable.Dispose()
		{
			Dispose(true);
			 GC.SuppressFinalize(this);
		}
		#endregion

		public void New()
		{
			Profile = new();

			Profile.Macros.Add(new() { Id = 0, Name = Translate.Get("profile none") });

			Modified = true;
		}

		public async System.Threading.Tasks.Task<bool> Load(string filename)
		{
			try
			{
				string json = System.IO.File.ReadAllText(filename);
				Profile = System.Text.Json.JsonSerializer.Deserialize<Shared.ProfileModel>(json);
			}
			catch (Exception ex)
			{
				try
				{
					using OldProfile.DSPerfil old = new();
					old.ReadXml(filename);
					ConvertOld(old);
					Modified = false;
					return true;
				}
				catch { }
				await MessageBox.Show(ex.Message, Translate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			Modified = false;
			return true;
		}

		public async System.Threading.Tasks.Task<bool> Save(string filename)
		{
			//Reorder action ids
			ushort id = 0;
			foreach (ProfileModel.MacroModel ar in Profile.Macros)
			{
				if (ar.Id != id)
				{
					foreach (KeyValuePair<uint, ProfileModel.AxisMapModel> amm in Profile.AxesMap)
					{
						foreach (KeyValuePair<byte, ProfileModel.AxisMapModel.ModeModel> mm in amm.Value.Modes)
						{
							foreach (KeyValuePair<byte, ProfileModel.AxisMapModel.ModeModel.AxisModel> am in mm.Value.Axes)
							{
								for (int i = 0; i < am.Value.Actions.Count; i++)
								{
									if (am.Value.Actions[i] == ar.Id) { am.Value.Actions[i] = id; }
								}
							}
						}
					}
					foreach (KeyValuePair<uint, ProfileModel.ButtonMapModel> bmm in Profile.ButtonsMap)
					{
						foreach (KeyValuePair<byte, ProfileModel.ButtonMapModel.ModeModel> mm in bmm.Value.Modes)
						{
							foreach (KeyValuePair<byte, ProfileModel.ButtonMapModel.ModeModel.ButtonModel> bm in mm.Value.Buttons)
							{
								for (int i = 0; i < bm.Value.Actions.Count; i++)
								{
									if (bm.Value.Actions[i] == ar.Id) { bm.Value.Actions[i] = id; }
								}
							}
						}
					}
				}
				id++;
			}

			try
			{
				System.IO.File.WriteAllText(filename, System.Text.Json.JsonSerializer.Serialize(Profile));
			}
			catch (Exception ex)
			{
				await MessageBox.Show(ex.Message, Translate.Get("error"), MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			Modified = false;
			return true;
		}

		private void ConvertOld(OldProfile.DSPerfil old)
		{
			Profile = new()
			{
				MouseTick = old.GENERAL[0].TickRaton
			};

			foreach (OldProfile.DSPerfil.ACCIONESRow ar in old.ACCIONES)
			{
				ProfileModel.MacroModel macro = new()
				{
					Id = ar.idAccion,
					Name = ar.Nombre
				};
				if (!ar.IsComandosNull())
				{
					foreach (ushort action in ar.Comandos)
					{
						if (((action & 0x7f) == (byte)CTypes.CommandType.DxButton) || ((action & 0x7f) == (byte)CTypes.CommandType.DxHat))
						{
							byte joy = ((byte)((action >> 8) & 0b0111));
							byte bt = (byte)((action >> 8) >> 3);
							macro.Commands.Add((uint)((joy << 16) | (bt << 8) | (action & 0xff)));
						}
					}
				}
				Profile.Macros.Add(macro);
			}

			foreach (OldProfile.DSPerfil.MAPABOTONESRow br in old.MAPABOTONES)
			{
				byte size = br.TamIndices;
				uint joyId = GetId((OldProfile.CTipos.TipoJoy)br.idJoy);
				for (byte i = 0; i < size; i++)
				{
					OldProfile.DSPerfil.INDICESBOTONESRow ibr = old.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(br.idJoy, br.idPinkie, br.idModo, br.idBoton, i);
					if (ibr != null)
					{
						if (!Profile.ButtonsMap.TryGetValue(joyId, out ProfileModel.ButtonMapModel bmm))
						{
							bmm = new();
							Profile.ButtonsMap.Add(joyId, bmm);
						}
						if (!bmm.Modes.TryGetValue((byte)((ibr.idPinkie << 4) | ibr.idModo), out ProfileModel.ButtonMapModel.ModeModel mm))
						{
							mm = new();
							bmm.Modes.Add((byte)((ibr.idPinkie << 4) | ibr.idModo), mm);
						}
						if (!mm.Buttons.TryGetValue((byte)ibr.idBoton, out ProfileModel.ButtonMapModel.ModeModel.ButtonModel bm))
						{
							bm = new();
							mm.Buttons.Add((byte)ibr.idBoton, bm);
						}
						bm.Actions.Add(ibr.idAccion);
					}
				}
			}

			foreach (OldProfile.DSPerfil.MAPAEJESRow er in old.MAPAEJES)
			{
				uint joyId = GetId((OldProfile.CTipos.TipoJoy)er.idJoy);
				if (!Profile.AxesMap.TryGetValue(joyId, out ProfileModel.AxisMapModel amm))
				{
					amm = new();
					Profile.AxesMap.Add(joyId, amm);
				}
				if (!amm.Modes.TryGetValue((byte)((er.idPinkie << 4) | er.idModo), out ProfileModel.AxisMapModel.ModeModel mm))
				{
					mm = new();
					amm.Modes.Add((byte)((er.idPinkie << 4) | er.idModo), mm);
				}
				if (!mm.Axes.TryGetValue(er.idEje, out ProfileModel.AxisMapModel.ModeModel.AxisModel am))
				{
					am = new();
					mm.Axes.Add(er.idEje, am);
				}
				am.Mouse = er.Mouse;
				am.IdJoyOutput = er.IsJoySalidaNull() ? (byte)0 : er.JoySalida;
				am.Type = er.TipoEje;
				am.OutputAxis = er.Eje;
				er.Sensibilidad.CopyTo(am.Sensibility, 0);
				am.IsSensibilityForSlider = er.Slider == 1;
				am.Bands = [.. er.Bandas];
				am.Toughness = (er.ResistenciaInc, er.ResistenciaDec);

				for (byte i= 0; i <= byte.MaxValue; i++)
				{
					OldProfile.DSPerfil.INDICESEJESRow ier = old.INDICESEJES.FindByidJoyidPinkieidModoidEjeid(er.idJoy, er.idPinkie, er.idModo, er.idEje, i);
					if (ier == null)
					{
						break;
					}
					else
					{
						am.Actions.Add(ier.idAccion);
					}
				}
			}

			//foreach (OldProfile.DSPerfil.MAPASETASRow br in old.MAPASETAS)
			//{
			//	byte size = br.TamIndices;
			//	uint joyId = GetId((OldProfile.CTipos.TipoJoy)br.idJoy);
			//	for (byte i = 0; i < size; i++)
			//	{
			//		OldProfile.DSPerfil.INDICESBOTONESRow ibr = old.INDICESBOTONES.FindByidJoyidPinkieidModoidBotonid(br.idJoy, br.idPinkie, br.idModo, br.idBoton, i);
			//		if (ibr != null)
			//		{
			//			if (!Profile.ButtonsMap.TryGetValue(joyId, out ProfileModel.ButtonMapModel bmm))
			//			{
			//				bmm = new();
			//				Profile.ButtonsMap.Add(joyId, bmm);
			//			}
			//			if (!bmm.Modes.TryGetValue((byte)((ibr.idPinkie << 4) | ibr.idModo), out ProfileModel.ButtonMapModel.ModeModel mm))
			//			{
			//				mm = new();
			//				bmm.Modes.Add((byte)((ibr.idPinkie << 4) | ibr.idModo), mm);
			//			}
			//			if (!mm.Buttons.TryGetValue((byte)ibr.idBoton, out ProfileModel.ButtonMapModel.ModeModel.ButtonModel bm))
			//			{
			//				bm = new();
			//				mm.Buttons.Add((byte)ibr.idBoton, bm);
			//			}
			//			bm.Actions.Add(ibr.idAccion);
			//		}
			//	}
			//}

		}

		private static uint GetId(OldProfile.CTipos.TipoJoy joy)
		{
			return joy switch
			{
				OldProfile.CTipos.TipoJoy.NXT => 0x231d0200,
				OldProfile.CTipos.TipoJoy.Pedales => 0x06a30763,
				//_ => 0x063a0255 x52
				_ => 0x231d012d
			};
		}
	}
}
