using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LowLand.Model.Product
{
<<<<<<< Updated upstream
    public class Product : INotifyPropertyChanged
    {
        required public int Id { get; set; }
        required public int ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }
        public string? CategoryName { get; set; }
=======
    public abstract class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }
>>>>>>> Stashed changes
        required public string Name { get; set; }
        public int SalePrice { get; set; } = 39000;
        public int CostPrice { get; set; } = 20000;
        public string Image { get; set; } = "product_default.jpg";
<<<<<<< Updated upstream
        public string Size { get; set; } = "X";

        public event PropertyChangedEventHandler? PropertyChanged;
=======

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class SingleProduct : Product
    {
        required public Category Category { get; set; }
        required public ProductType ProductType { get; set; }

    }

    public class ComboProduct : Product
    {
        public List<int> ProductIds { get; set; } = new List<int>(); 
>>>>>>> Stashed changes
    }
}

