using System;
using System.Windows;

namespace Calibrator
{
    internal class ImagenDisabled : System.Windows.Controls.Image
    {
        static ImagenDisabled()
        {
            IsEnabledProperty.OverrideMetadata(typeof(ImagenDisabled), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnAutoGreyScaleImageIsEnabledPropertyChanged)));
        }

        private static void OnAutoGreyScaleImageIsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ImagenDisabled autoGreyScaleImg = (ImagenDisabled)source;
            Boolean isEnable = (Boolean)args.NewValue;
            if (autoGreyScaleImg.Source != null)
            {
                if (!isEnable)
                {
                    System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(autoGreyScaleImg.Source.ToString()));
                    autoGreyScaleImg.Source = new System.Windows.Media.Imaging.FormatConvertedBitmap(bitmapImage, System.Windows.Media.PixelFormats.Gray32Float, null, 0);
                    autoGreyScaleImg.OpacityMask = new System.Windows.Media.ImageBrush(bitmapImage);
                }
                else
                {
                    autoGreyScaleImg.Source = ((System.Windows.Media.Imaging.FormatConvertedBitmap)(autoGreyScaleImg.Source)).Source;
                    autoGreyScaleImg.OpacityMask = null;
                }
            }
        }
    }
}
