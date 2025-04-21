using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LowLand.Model.Order
{
    public class Order : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerStatus { get; set; }
        public int? PromotionId { get; set; }
        public int TotalPrice { get; set; }
        public int DiscountedApplied => TotalPrice - TotalAfterDiscount;
        public int TotalAfterDiscount { get; set; }
        public string? Status { get; set; }
        public DateTime Date { get; set; }

        public decimal TotalCostPrice { get; set; }
        public ObservableCollection<OrderDetail>? Details { get; set; } = new ObservableCollection<OrderDetail>();
        public event PropertyChangedEventHandler? PropertyChanged;

    }
}
