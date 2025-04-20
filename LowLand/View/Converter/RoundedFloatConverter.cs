using System;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class RoundedFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double floatValue = (double)value;

            // Round to 2 decimal places
            floatValue = Math.Round(floatValue, 2);
            return floatValue.ToString("F2");

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
