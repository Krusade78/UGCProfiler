using FluentAvalonia.UI.Controls;

namespace Profiler
{
	public enum MessageBoxButton
	{
		OK,
		OKCancel,
		YesNoCancel,
		YesNo,
	}

	public enum MessageBoxImage
	{
		Error,
		Question,
		Warning,
		Information
	}

	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public partial class MessageBox : Frame
	{
		private MessageBox()
		{
			this.InitializeComponent();
		}

        public async static System.Threading.Tasks.Task<ContentDialogResult> Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
		{
			ContentDialog dialog = new()
			{ 
				Title = title
			};
			switch (button)
			{
				case MessageBoxButton.YesNo:
					dialog.PrimaryButtonText = Translate.Get("yes");
					dialog.SecondaryButtonText = Translate.Get("no");
					dialog.DefaultButton = ContentDialogButton.Primary;
					break;
				case MessageBoxButton.OKCancel:
					dialog.PrimaryButtonText = Translate.Get("ok");
					dialog.CloseButtonText = Translate.Get("cancel");
					dialog.DefaultButton = ContentDialogButton.Primary;
					break;
				case MessageBoxButton.YesNoCancel:
					dialog.PrimaryButtonText = Translate.Get("yes");
					dialog.SecondaryButtonText = Translate.Get("no");
					dialog.CloseButtonText = Translate.Get("cancel");
					break;
				default:
					dialog.PrimaryButtonText = Translate.Get("ok");
					break;
			}
			dialog.Content = new MessageBox();

			((MessageBox)dialog.Content).txt.Text = message;
			switch (image)
			{
				//case System.Windows.MessageBoxImage.Hand:
				//case System.Windows.MessageBoxImage.Stop:
				case MessageBoxImage.Error:
					((MessageBox)dialog.Content).imgError.IsVisible = true;
					break;
				case MessageBoxImage.Question:
					((MessageBox)dialog.Content).imgQuestion.IsVisible = true;
					break;
				//case System.Windows.MessageBoxImage.Exclamation:
				case MessageBoxImage.Warning:
					((MessageBox)dialog.Content).imgWarning.IsVisible = true;
					break;
				//case System.Windows.MessageBoxImage.Asterisk:
				case MessageBoxImage.Information:
					((MessageBox)dialog.Content).imgInfo.IsVisible = true;
					break;
				default:
					break;
			}

			return await dialog.ShowAsync();
		}
	}
}
