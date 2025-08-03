using System;
using System.Windows;
using System.IO.Pipes;
using System.Threading.Tasks;


namespace Launcher
{
	internal class CService(object main) : IDisposable
	{
		private System.Threading.CancellationTokenSource closePipe = new();
		private System.Threading.CancellationTokenSource closePipeSvc = new();
		private System.IO.BinaryWriter outputPipeSvc = null;
		private readonly object main = main;

		public event EventHandler<ResolveEventArgs> ExitEvt;
		public enum MsgType : byte { RawMode, CalibrationMode, Calibration, Antivibration, Map, Macros };

		#region IDisposable Support
		private bool disposedValue = false; // Para detectar llamadas redundantes

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					closePipe?.Cancel();
					while (closePipe != null) { System.Threading.Thread.Sleep(100); }
					closePipeSvc?.Cancel();
					while (closePipeSvc != null) { System.Threading.Thread.Sleep(100); }
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

		public bool Init()
		{
			Task.Run(() =>
				{
					while (!closePipe.Token.IsCancellationRequested)
					{
						using NamedPipeServerStream pipeServer = new("LauncherPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough);
						try { pipeServer.WaitForConnectionAsync(closePipe.Token).Wait(closePipe.Token); } catch { break; }
						if (closePipe.Token.IsCancellationRequested)
						{
							break;
						}
						using System.IO.StreamReader r = new(pipeServer);
						MessageIn(r.ReadToEnd());
					}
					closePipe.Dispose();
					closePipe = null;
				});
			Task.Run(() =>
			{
				using (NamedPipeServerStream pipeServerSvc = new("LauncherPipeSvc", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.WriteThrough))
				{
					while (!closePipeSvc.Token.IsCancellationRequested)
					{
						try { pipeServerSvc.WaitForConnectionAsync(closePipeSvc.Token).Wait(closePipeSvc.Token); } catch { closePipeSvc.Cancel(); }
						if (!closePipeSvc.Token.IsCancellationRequested)
						{
							try
							{
								using System.IO.StreamReader r = new(pipeServerSvc);
								using System.IO.BinaryWriter w = new(pipeServerSvc);
								while (!closePipeSvc.Token.IsCancellationRequested)
								{
									if (r.ReadLine() == "OK")
									{
										outputPipeSvc = w;
										LoadCalibration();
										closePipeSvc.Token.WaitHandle.WaitOne();
									}
									else
									{
										break;
									}
								}
							}
							catch
							{
								closePipeSvc.Cancel();
							}
							if (outputPipeSvc != null)
							{
								outputPipeSvc.Close();
								outputPipeSvc = null;
							}
						}
					}
				}
				closePipeSvc.Dispose();
				closePipeSvc = null;
			}).ContinueWith((ret) => ExitEvt.Invoke(null, null));

			return true;
		}

		private void LoadCalibration()
		{
			CCalibration.Load(outputPipeSvc);
		}

		private void MessageIn(string msj)
		{
			if (msj.StartsWith("CCAL"))
			{
				LoadCalibration();
			}
			else if (msj.StartsWith("CAL:"))
			{
				if (outputPipeSvc != null)
				{
					byte[] buff = [(byte)MsgType.CalibrationMode, msj.Contains("True") ? (byte)1 : (byte)0];
					outputPipeSvc.Write(buff, 0, 2);
					outputPipeSvc.Flush();
				}
			}
			else if (msj.StartsWith("RAW:"))
			{
				if (outputPipeSvc != null)
				{
					byte[] buff = [(byte)MsgType.RawMode, msj.Contains("True") ? (byte)1 : (byte)0];
					outputPipeSvc.Write(buff, 0, 2);
					outputPipeSvc.Flush();
				}
			}
			else if (msj.StartsWith("DEF:"))
			{
				LoadProfile(null);
			}
			else
			{
				LoadProfile(msj);
			}
		}

		public void LoadProfile(string file)
		{
			bool ret;
			lock (this)
			{

				ret = new CProfile(main).Load(file, outputPipeSvc);
			}

			if (ret && (file != null))
			{
				string name = System.IO.Path.GetFileNameWithoutExtension(file);
				((CMain)main).MessageBox(CTranslate.Get("profile loaded ok"), name, MessageBoxImage.Information);
			}
		}
	}
}
