using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class DiscountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.WriteLine($"DiscountConverter: Value = {value}, Type = {value?.GetType()}");
            try
            {
                if (value != null)
                {
                    if (int.TryParse(value.ToString(), out int discount) && discount > 0)
                    {
                        var result = $"- {discount.ToString("C0", new System.Globalization.CultureInfo("vi-VN"))}";
                        Debug.WriteLine($"DiscountConverter: Result = {result}");
                        return result;
                    }
                    return $"- {0.ToString("C0", new System.Globalization.CultureInfo("vi-VN"))}";
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DiscountConverter: Error = {ex.Message}");
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}