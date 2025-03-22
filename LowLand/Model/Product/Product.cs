using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LowLand.Model.Product
{
    public abstract class Product : INotifyPropertyChanged
    {
        required public int Id { get; set; }
        required public string Name { get; set; }
        public int SalePrice { get; set; } = 39000;
        public int CostPrice { get; set; } = 20000;
        public string Image { get; set; } = "product_default.jpg";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class SingleProduct : Product
    {
        required public Category Category { get; set; }
        required public ProductType ProductType { get; set; }

    }

    public partial class ComboProduct : Product
    {
        public List<int> ProductIds { get; set; } = new List<int>();
    }
}