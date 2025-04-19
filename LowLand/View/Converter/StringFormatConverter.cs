using System;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                if (parameter is string format)
                {
                    return string.Format(format, "N/A");
                }
                return "N/A";
            }

            if (parameter is string formatString)
            {
                if (value is DateTime dateTime)
                {
                    return string.Format(formatString, dateTime.ToString("dd/MM/yyyy"));
                }
                return string.Format(formatString, value);
            }

            return value?.ToString() ?? "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}