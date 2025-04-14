using System;
using System.ComponentModel;

namespace LowLand.Model.Discount
{
    public class Promotion : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public PromotionType Type { get; set; }
        public double Amount { get; set; }
        public int MinimumOrderValue { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public enum PromotionType : int
    {
        Percentage,
        FixedAmount
    }
}
