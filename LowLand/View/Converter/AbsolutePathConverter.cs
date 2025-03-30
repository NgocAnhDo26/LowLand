using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace LowLand.View.Converter
{
    public partial class AbsolutePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null) return "";

                string filename = value.ToString()!;
                string folder = "C:/LowLand/Images";
                //string folder = AppDomain.CurrentDomain.BaseDirectory;
                var path = $"{folder}/{filename}";

                if (!System.IO.File.Exists(path))
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/product_default.jpg"));
                }

                return new BitmapImage(new Uri("file:///" + path));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
