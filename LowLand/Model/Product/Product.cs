using System.Collections.Generic;
using System.ComponentModel;

namespace LowLand.Model.Product
{
    public abstract class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }

        required public string Name { get; set; }
        public int SalePrice { get; set; } = 39000;
        public int CostPrice { get; set; } = 20000;
        public string Image { get; set; } = "product_default.jpg";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class SingleProduct : Product
    {
        public Category? Category { get; set; }
    }

    public class ComboProduct : Product
    {
        public List<int> ProductIds { get; set; } = new List<int>();
    }
}

