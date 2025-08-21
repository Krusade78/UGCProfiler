using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

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
    public sealed partial class MessageBox : Page
    {
        public MessageBox()
        {
            this.InitializeComponent();
        }

        public async static System.Threading.Tasks.Task<ContentDialogResult> Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            ContentDialog dialog = new()
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = ((App)Application.Current).GetRoot(),
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
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
                    ((MessageBox)dialog.Content).imgError.Visibility = Visibility.Visible;
                    break;
                case MessageBoxImage.Question:
                    ((MessageBox)dialog.Content).imgQuestion.Visibility = Visibility.Visible;
                    break;
                //case System.Windows.MessageBoxImage.Exclamation:
                case MessageBoxImage.Warning:
                    ((MessageBox)dialog.Content).imgWarning.Visibility = Visibility.Visible;
                    break;
                //case System.Windows.MessageBoxImage.Asterisk:
                case MessageBoxImage.Information:
                    ((MessageBox)dialog.Content).imgInfo.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            return await dialog.ShowAsync();
        }
    }
}
