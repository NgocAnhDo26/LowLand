using System;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class NumberToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Check if the value is a number
            if (value is double number)
            {
                // Convert the number to a percentage string
                return number > 0 ? $"{number:N0}% ▲" : number < 0 ? $"{number:N0}% ▼" : $"{number:N0}%";
            }
            else if (value is int intValue)
            {
                // Convert the integer to a percentage string
                return intValue > 0 ? $"{intValue:N0}% ▲" : intValue < 0 ? $"{intValue:N0}% ▼" : $"{intValue:N0}%";
            }

            // If the value is not a number, return an empty string
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
