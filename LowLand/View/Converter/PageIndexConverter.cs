using System;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class PageIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((int)value) - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((int)value) + 1;
        }
    }
}