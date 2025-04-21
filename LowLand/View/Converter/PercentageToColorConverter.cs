using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace LowLand.View.Converter
{
    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double percentage)
            {
                return new SolidColorBrush(
                    percentage < 0 ? Colors.Red :
                    percentage > 0 ? Colors.LimeGreen : Colors.Orange);
            }
            else if (value is int intPercentage)
            {
                return new SolidColorBrush(
                    intPercentage < 0 ? Colors.Red :
                    intPercentage > 0 ? Colors.LimeGreen : Colors.Orange);
            }

            return new SolidColorBrush(Colors.Orange); // Fallback color
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
