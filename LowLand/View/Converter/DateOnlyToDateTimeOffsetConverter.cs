using System;
using Microsoft.UI.Xaml.Data;

namespace LowLand.View.Converter
{
    public class DateOnlyToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateOnly dateOnly)
            {
                // Convert DateOnly to DateTimeOffset
                return new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset dateTimeOffset)
            {
                // Convert DateTimeOffset back to DateOnly
                return DateOnly.FromDateTime(dateTimeOffset.DateTime);
            }
            return null;
        }
    }
}
