using System;
using LowLand.Model.Discount;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class PromotionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Promotion promotion)
            {
                // Assuming PromotionType is an enum or string in the Promotion class
                switch (promotion.Type)
                {
                    case PromotionType.Percentage:
                        return $"{promotion.Amount}%";
                    case PromotionType.FixedAmount:
                        return promotion.Amount.ToString("C0", new System.Globalization.CultureInfo("vi-VN")); // Adjust culture as needed
                    default:
                        return promotion.Amount.ToString();
                }
            }
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
