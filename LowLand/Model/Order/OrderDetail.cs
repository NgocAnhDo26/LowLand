using System.Collections.ObjectModel;
using System.ComponentModel;
using LowLand.Model.Product;

namespace LowLand.Model.Order
{
    public class OrderDetail : INotifyPropertyChanged
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public int quantity { get; set; }
        public int? ProductId { get; set; }
        public int ProductPrice { get; set; }
        public int Price { get; set; }
        public string? ProductName { get; set; }
        public int? OptionId { get; set; }
        public string? OptionName { get; set; }
        public ObservableCollection<ProductOption> ProductOptions { get; set; } = new ObservableCollection<ProductOption>();

        public event PropertyChangedEventHandler? PropertyChanged;


    }
}