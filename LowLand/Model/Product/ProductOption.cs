using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Product
{
    public class ProductOption : INotifyPropertyChanged
    {
        public int OptionId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int CostPrice { get; set; }
        public int SalePrice { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
