using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Model.Customer
{
    public class Customer
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        public string? Phone { get; set; }
        required public int Point { get; set; }
        required public DateTime RegistrationDate { get; set; }

        // Rank Details
        required public int RankId { get; set; }
        public string? RankName { get; set; }
        public int? PromotionPoint { get; set; }
        public int? DiscountPercentage { get; set; }
    }
}
