using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Product
{
    public class ProductType
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        required public int CategoryId { get; set; }
    }
}
