using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LowLand.Model.Product
{
    public abstract class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }

        required public string Name { get; set; }
        public int SalePrice { get; set; } = 39000;
        public int CostPrice { get; set; } = 20000;
        public int Profit => SalePrice - CostPrice;
        public string Image { get; set; } = "product_default.jpg";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class SingleProduct : Product
    {
        public Category? Category { get; set; }
        public List<ProductOption> Options { get; set; } = new List<ProductOption>();
    }

    public class ComboProduct : Product
    {
        public List<ComboItem> ChildProducts { get; set; } = new List<ComboItem>();

        private int _originalSalePrice;
        public int OriginalSalePrice
        {
            get => ChildProducts.Sum(item => item.Option != null ? item.Option.SalePrice * item.Quantity : item.Product.SalePrice * item.Quantity);
            set => _originalSalePrice = value;
        }

    }
    public class ComboItem : INotifyPropertyChanged
    {
        public int ItemId { get; set; }
        public int ComboId { get; set; }
        public required SingleProduct Product { get; set; }
        public ProductOption? Option { get; set; }
        public int Quantity { get; set; } = 1;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

