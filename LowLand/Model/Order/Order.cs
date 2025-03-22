using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Order
{
    public class Order
    {
        public int Id { get; set; }
        required public int CustomerId { get; set; }
        required public int PromotionId { get; set; }
        required public int TotalPrice { get; set; }
        public int DiscountedApplied => TotalPrice - TotalAfterDiscount;
        required public int TotalAfterDiscount { get; set; }
        required public int Status { get; set; }
        public string Name_Option { get; set; } = "Size: X";

    }
}
