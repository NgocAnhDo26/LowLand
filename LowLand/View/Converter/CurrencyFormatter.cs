using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace LowLand.View.Converter
{
    public class CurrencyFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            int amount = (int)value;
            CultureInfo culture = CultureInfo.GetCultureInfo("vi-VN");  // en-US /en-UK
            string formatted = string.Format(culture, "{0:c}", amount);
            return formatted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return 0;

            string stringValue = value.ToString().Replace("₫", "").Replace(",", "").Trim();
            if (int.TryParse(stringValue, NumberStyles.Any, new CultureInfo("vi-VN"), out int result))
            {
                return result;
            }
            return 0;
        }
    }
}
