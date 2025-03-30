using System.Diagnostics;
using System.Linq;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderDetailPage : Page
    {
        OrderViewModel ViewModel { get; set; } = new OrderViewModel();
        public OrderDetailPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!ValidateAndLoadOrder(e.Parameter))
            {
                Debug.WriteLine("⚠️ Không thể tải đơn hàng!");
            }
        }

        private bool ValidateAndLoadOrder(object parameter)
        {
            if (parameter == null)
            {
                Debug.WriteLine("⚠️ e.Parameter is NULL!");
                return false;
            }

            if (parameter is not int orderId)
            {
                Debug.WriteLine($"⚠️ Unexpected parameter type: {parameter.GetType()}");
                return false;
            }

            Debug.WriteLine($"🔍 Loading Order ID: {orderId}");

            if (ViewModel.Orders == null || !ViewModel.Orders.Any())
            {
                Debug.WriteLine("⚠️ ViewModel.Orders is NULL or Empty!");
                return false;
            }

            var order = ViewModel.Orders.FirstOrDefault(c => c.Id == orderId);
            if (order == null)
            {
                Debug.WriteLine("⚠️ No order found with given Id!");
                return false;
            }

            ViewModel.EditorAddOrder = order;
            Debug.WriteLine($"✅ Order Loaded: {order.Id}");

            return true;
        }


        private void searchButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
