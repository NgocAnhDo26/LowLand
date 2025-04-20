using System.Globalization;
using Windows.Globalization.NumberFormatting;

namespace LowLand.View.Converter
{
    // Custom NumberFormatter for Vietnamese currency (đ)
    public class NumberFormatter : INumberFormatter2, INumberParser
    {
        private readonly CultureInfo _cultureInfo;

        public NumberFormatter()
        {
            _cultureInfo = new CultureInfo("vi-VN");
        }

        // Format double as currency (e.g., 12345.0 -> "12.345 đ")
        public string FormatDouble(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return "";
            }
            return string.Format(_cultureInfo, "{0:N0} đ", value);
        }

        // Format long as currency (e.g., 12345 -> "12.345 đ")
        public string FormatInt(long value)
        {
            return string.Format(_cultureInfo, "{0:N0} đ", value);
        }

        // Format ulong as currency (e.g., 12345 -> "12.345 đ")
        public string FormatUInt(ulong value)
        {
            return string.Format(_cultureInfo, "{0:N0} đ", value);
        }

        // Parse double from text (e.g., "12345" or "12.345" -> 12345.0)
        public double? ParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Clean input: remove "đ" and dots
            text = text.Replace("đ", "").Replace(".", "").Trim();

            if (double.TryParse(text, NumberStyles.Any, _cultureInfo, out double result))
            {
                return result;
            }

            return null;
        }

        // Parse long from text (e.g., "12345" -> 12345)
        public long? ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            text = text.Replace("đ", "").Replace(".", "").Trim();

            if (long.TryParse(text, NumberStyles.Any, _cultureInfo, out long result))
            {
                return result;
            }

            return null;
        }

        // Parse ulong from text (e.g., "12345" -> 12345)
        public ulong? ParseUInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            text = text.Replace("đ", "").Replace(".", "").Trim();

            if (ulong.TryParse(text, NumberStyles.Any, _cultureInfo, out ulong result))
            {
                return result;
            }

            return null;
        }
    }
}
