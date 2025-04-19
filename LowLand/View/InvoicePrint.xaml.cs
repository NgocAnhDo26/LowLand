using LowLand.Model.Order;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    public sealed partial class InvoicePrint : UserControl
    {
        public Order Order { get; set; }
        public InvoicePrint(Order order)
        {
            this.InitializeComponent();
            Order = order ?? new Order();
            this.DataContext = this;
        }
    }
}
