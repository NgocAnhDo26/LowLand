using System;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class PromotionStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isActive = (bool)value;
            return isActive ? "Đang diễn ra" : "Tạm ngưng";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
