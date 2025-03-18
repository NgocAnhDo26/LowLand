using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Product
{
    public class Product
    {
        required public int Id { get; set; }
        required public int ProductTypeId { get; set; }
        required public string Name { get; set; }
        public int SalePrice { get; set; } = 39000;
        public int CostPrice { get; set; } = 20000;
        public string Image { get; set; } = "product_default.jpg";
        public string Size { get; set; } = "X";
    }
}
