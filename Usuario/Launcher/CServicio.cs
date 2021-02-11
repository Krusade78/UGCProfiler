using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Launcher
{
	class CServicio : IDisposable
	{
		private Comunes.DataSetConfiguracion dsc = new Comunes.DataSetConfiguracion();
		private System.Threading.CancellationTokenSource cerrarPipe = new System.Threading.CancellationTokenSource();
		private System.Threading.CancellationTokenSource cerrarPipeSvc = new System.Threading.CancellationTokenSource();
		private System.IO.BinaryWriter salidaPipeSvc = null;
		public enum TipoMsj : byte { ModoRaw, ModoCalibrado, Calibrado, Antiv, Mapa, Comandos };

		public CServicio()
		{
		}

		#region IDisposable Support
		private bool disposedValue = false; // Para detectar llamadas redundantes

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					cerrarPipe.Cancel();
					while (cerrarPipe != null) { System.Threading.Thread.Sleep(100); }
					dsc.Dispose(); dsc = null;
					cerrarPipeSvc.Cancel();
					while (cerrarPipeSvc != null) { System.Threading.Thread.Sleep(100); }
				}
				disposedValue = true;
			}
		}
		public void Dispose()
		{
			 Dispose(true);
			 GC.SuppressFinalize(this);
		}
		#endregion
		public bool Iniciar()
		{
			Task.Run(() =>
				{
					while (!cerrarPipe.Token.IsCancellationRequested)
					{
						using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("LauncherPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough))
						{
							try { pipeServer.WaitForConnectionAsync(cerrarPipe.Token).Wait(cerrarPipe.Token); } catch { break; }
							if (cerrarPipe.Token.IsCancellationRequested)
								break;
							using (System.IO.StreamReader r = new System.IO.StreamReader(pipeServer))
							{
								MensajeIn(r.ReadToEnd());
							}
						}
					}
					cerrarPipe.Dispose();
					cerrarPipe = null;
				});
			Task.Run(() =>
			{
				using (NamedPipeServerStream pipeServerSvc = new NamedPipeServerStream("LauncherPipeSvc", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough))
				{
					using (System.Diagnostics.Process p = new System.Diagnostics.Process())
					{
						p.StartInfo.FileName = "xhotas_svc.dat";
						p.StartInfo.UseShellExecute = false;
						try
						{
							p.Start();
						}
						catch { }
					}
					while (!cerrarPipeSvc.Token.IsCancellationRequested)
					{
						try { pipeServerSvc.WaitForConnectionAsync(cerrarPipeSvc.Token).Wait(cerrarPipeSvc.Token); } catch { cerrarPipeSvc.Cancel(); }
						if (!cerrarPipeSvc.Token.IsCancellationRequested)
						{
							using (System.IO.StreamReader r = new System.IO.StreamReader(pipeServerSvc))
							{
								using (System.IO.BinaryWriter w = new System.IO.BinaryWriter(pipeServerSvc))
								{
									while (!cerrarPipeSvc.Token.IsCancellationRequested)
									{
										if (r.ReadLine() == "OK")
										{
											salidaPipeSvc = w;
											CargarCalibrado();
											cerrarPipeSvc.Token.WaitHandle.WaitOne();
										}
										else
										{
											break;
										}
									}
								}
							}
							
							if (salidaPipeSvc != null)
							{
								salidaPipeSvc.Close();
								salidaPipeSvc = null;
							}
						}
					}
				}
				cerrarPipeSvc.Dispose();
				cerrarPipeSvc = null;
			});

			return true;
		}

		private void CargarCalibrado()
		{
			dsc.Clear();
			try
			{
				dsc.ReadXml("configuracion.dat");
			}
			catch {}

			short[,] mapaRangos = {
				{ 0,0,0,255,0,0,63,63 },
				{ 1023,1023,0,511,0,0,0,0 },
				{ 0,0,127,127,127,127,7,7 },
				{ 32767,32767,32767,32767,32767,32767,127,127 }
			};

			Comunes.CTipos.STJITTER[,] jitter = new Comunes.CTipos.STJITTER[4,8];
			Comunes.CTipos.STLIMITES[,] limites = new Comunes.CTipos.STLIMITES[4,8];
			if ((dsc.CALIBRADO_LIMITES.Count == 32) && (dsc.CALIBRADO_JITTER.Count == 32))
			{
				for (byte j = 0; j < 4; j++)
				{
					for (int i = 0; i < 8; i++)
					{
						if ((dsc.CALIBRADO_LIMITES.Count == 32) && (dsc.CALIBRADO_JITTER.Count == 32))
						{
							limites[j, i].Cen = dsc.CALIBRADO_LIMITES[(j * 8) + i].Cen;
							limites[j, i].Izq = dsc.CALIBRADO_LIMITES[(j * 8) + i].Izq;
							limites[j, i].Der = dsc.CALIBRADO_LIMITES[(j * 8) + i].Der;
							limites[j, i].Nulo = dsc.CALIBRADO_LIMITES[(j * 8) + i].Nulo;
							limites[j, i].Cal = dsc.CALIBRADO_LIMITES[(j * 8) + i].Cal;
							jitter[j, i].Margen = dsc.CALIBRADO_JITTER[(j * 8) + i].Margen;
							jitter[j, i].Resistencia = dsc.CALIBRADO_JITTER[(j * 8) + i].Resistencia;
							jitter[j, i].Antiv = dsc.CALIBRADO_JITTER[(j * 8) + i].Antiv;
						}
						else
                        {
							limites[j, i].Cal = 0;
							limites[j, i].Cen = 0;
							limites[j, i].Izq = (short)-mapaRangos[j, i];
							limites[j, i].Der = mapaRangos[j, i]; ;
							jitter[j, i].Antiv = 0;
							jitter[j, i].Margen = 0;
							jitter[j, i].Resistencia = 0;
						}
					}
				}
			}

			if (salidaPipeSvc != null)
			{
				byte[] bufCal = new byte[1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)) * 8 * 4)];
				byte[] bufJit = new byte[1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)) * 8 * 4)];
				bufCal[0] = (byte)TipoMsj.Calibrado;
				bufJit[0] = (byte)TipoMsj.Antiv;
				for (byte j = 0; j < 4; j++)
				{
					for (int i = 0; i < 8; i++)
					{
						IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)));
						Marshal.StructureToPtr(limites[j, i], ptr, true);
						Marshal.Copy(ptr, bufCal, 1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)) * ((j * 8) + i)), Marshal.SizeOf(typeof(Comunes.CTipos.STLIMITES)));
						Marshal.FreeHGlobal(ptr);
						ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)));
						Marshal.StructureToPtr(jitter[j, i], ptr, true);
						Marshal.Copy(ptr, bufJit, 1 + (Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)) * ((j * 8) + i)), Marshal.SizeOf(typeof(Comunes.CTipos.STJITTER)));
						Marshal.FreeHGlobal(ptr);
					}
				}
				salidaPipeSvc.Write(bufCal, 0, bufCal.Length);
				salidaPipeSvc.Flush();
				salidaPipeSvc.Write(bufJit, 0, bufJit.Length);
				salidaPipeSvc.Flush();
            }
		}

		private void MensajeIn(string msj)
		{
			if (msj.StartsWith("CCAL"))
			{
				CargarCalibrado();
			}
			else if (msj.StartsWith("CAL:"))
			{
				if (salidaPipeSvc != null)
				{
					byte[] buff = { (byte)TipoMsj.Calibrado, (msj.IndexOf("True") != -1) ? (byte)1 : (byte)0 };
					salidaPipeSvc.Write(buff, 0, 2);
					salidaPipeSvc.Flush();
				}
			}
			else if (msj.StartsWith("RAW:"))
			{
				if (salidaPipeSvc != null)
				{
					byte[] buff = { (byte)TipoMsj.ModoRaw, (msj.IndexOf("True") != -1) ? (byte)1 : (byte)0 };
					salidaPipeSvc.Write(buff, 0, 2);
					salidaPipeSvc.Flush();
				}
			}
			else
			{
				CargarPerfil(msj);
			}
		}

		public void CargarPerfil(string archivo)
		{
			byte ret = 0;
			lock (this)
			{
				ret = CPerfil.CargarMapa(archivo, salidaPipeSvc);
			}

			if (ret == 0)
			{
				String nombre = System.IO.Path.GetFileNameWithoutExtension(archivo);
				CMain.MessageBox("Perfil cargado correctamente.", nombre, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
	}
}
