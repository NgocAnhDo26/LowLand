using LowLand.Model.Order;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class InvoicePrintPage : Page
    {
        public Order Order { get; set; }

        public InvoicePrintPage(Order order)
        {
            this.InitializeComponent();
            Order = order ?? new Order();
            this.DataContext = this;
        }
    }
}
