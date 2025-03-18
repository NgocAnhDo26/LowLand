using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowLand.Model.Product;

namespace LowLand.Model.Order
{
    public class OrderDetail
    {
        public int Id { get; set; }
        required public int OrderId { get; set; }
        required public Tuple<Product, int> Products { get; set; }
    }
}
