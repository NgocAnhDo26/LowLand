using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Product
{
    public class ProductType : INotifyPropertyChanged
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        required public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
