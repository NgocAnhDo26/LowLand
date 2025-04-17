using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class EditButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.WriteLine($"EditButtonVisibilityConverter: Value = {value}, Type = {value?.GetType()}, TargetType = {targetType}");
            try
            {
                string status = value?.ToString();
                bool isCompleted = string.Equals(status, "Hoàn thành", StringComparison.OrdinalIgnoreCase);
                var result = isCompleted ? Visibility.Collapsed : Visibility.Visible;
                Debug.WriteLine($"EditButtonVisibilityConverter: Status = {status}, Result = {result}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EditButtonVisibilityConverter: Error = {ex.Message}");
                return Visibility.Visible; // Mặc định hiển thị nếu có lỗi
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}