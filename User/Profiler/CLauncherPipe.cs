using System;

namespace Profiler
{
	internal static class CLauncherPipe
	{
		public static async System.Threading.Tasks.Task<bool> SetRawMode(bool on, bool onClose = false)
		{
			return await SendAsync("RAW:" + on.ToString(), onClose);
		}

		//public static async System.Threading.Tasks.Task<bool> SetCalibrationMode(bool on, bool onClose = false)
		//{
		//	return await SendAsync("CAL:" + on.ToString(), onClose);
		//}

		public static async System.Threading.Tasks.Task<bool> Reset()
		{
			return await SendAsync("DEF:");
		}

		public static async System.Threading.Tasks.Task<bool> LaunchProfileAsync(string file)
		{
			return await SendAsync(file);
		}

		private static async System.Threading.Tasks.Task<bool> SendAsync(string msj, bool onClose = false)
		{
			using System.IO.Pipes.NamedPipeClientStream pipeClient = new(".", "LauncherPipe", System.IO.Pipes.PipeDirection.Out);
			try
			{
				await pipeClient.ConnectAsync(200);
				pipeClient.Write(System.Text.Encoding.UTF8.GetBytes(msj));
				pipeClient.Flush();
			}
			catch (Exception ex)
			{
				if (!onClose)
				{
					await MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				return false;
			}

			return true;
		}
	}
}
