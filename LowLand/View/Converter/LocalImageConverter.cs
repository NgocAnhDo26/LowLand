using System;
using System.IO;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LowLand.View.Converter
{
    internal class LocalImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new BitmapImage(new Uri("ms-appx:///Assets/product_default.jpg"));

            string path = value.ToString()!;

            if (!File.Exists(path))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/product_default.jpg"));
            }

            return new BitmapImage(new Uri("file:///" + path));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
