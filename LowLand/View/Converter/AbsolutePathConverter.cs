using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.View.Converter
{
    public partial class AbsolutePathConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null) return "";

                string filename = value.ToString()!;
                string folder = AppDomain.CurrentDomain.BaseDirectory;
                string path = $"Assets/{filename}";

                // Find if the file exists
                if (!System.IO.File.Exists(path))
                {
                    path = $"Assets/product_default.jpg";
                }

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}