using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class CurrencyFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            double amount;
            if (value is int intValue)
            {
                amount = intValue;
            }
            else if (value is double doubleValue)
            {
                amount = doubleValue;
            }
            else
            {
                throw new ArgumentException("Value must be of type int or double.");
            }

            CultureInfo culture = CultureInfo.GetCultureInfo("vi-VN");  // en-US /en-UK
            string formatted = string.Format(culture, "{0:c}", amount);
            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
