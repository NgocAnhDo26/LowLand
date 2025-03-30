using System;
using System.Diagnostics;
using System.IO;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace LowLand.View.Converter
{
    public partial class AbsolutePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null) return new BitmapImage(new Uri("ms-appx:///Assets/product_default.jpg"));

                string filename = value.ToString()!;
                // If the path is already absolute, use it directly
                if (Path.IsPathRooted(filename))
                {
                    Debug.WriteLine("Rooted: " + filename);
                    if (File.Exists(filename))
                    {
                        return new BitmapImage(new Uri(filename));
                    }
                }
                else
                {
                    // Try to get the image file from the local folder
                    string localFolder = ApplicationData.Current.LocalFolder.Path;
                    var path = Path.Combine(localFolder, filename);

                    if (File.Exists(path))
                    {
                        return new BitmapImage(new Uri(path));
                    }
                }

                // Fall back to the default image
                return new BitmapImage(new Uri("ms-appx:///Assets/product_default.jpg"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new BitmapImage(new Uri("ms-appx:///Assets/product_default.jpg"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
