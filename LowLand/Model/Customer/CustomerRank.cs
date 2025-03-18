using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Customer
{
    public class CustomerRank
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        required public int PromotionPoint { get; set; }
        required public int DiscountPercentage { get; set; }
    }
}
