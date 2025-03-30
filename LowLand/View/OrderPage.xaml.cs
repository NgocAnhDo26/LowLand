using System;
using System.Diagnostics;
using LowLand.Model.Order;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrderPage : Page
    {
        public OrderViewModel ViewModel { get; set; } = new OrderViewModel();

        public OrderPage()
        {
            this.InitializeComponent();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            // Tìm kiếm đơn hàng theo từ khóa
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddOrderPage));
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Order selectedOrder)
                {
                    ContentDialog deleteDialog = new ContentDialog
                    {
                        Title = "Xác nhận xóa",
                        Content = $"Bạn có chắc muốn xóa đơn hàng {selectedOrder.Id}?",
                        PrimaryButtonText = "Xóa",
                        CloseButtonText = "Hủy",
                        XamlRoot = this.XamlRoot
                    };

                    ContentDialogResult result = await deleteDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        ViewModel.Remove(selectedOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("aaa", ex.Message);
            }
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Order selectedOrder)
            {
                Frame.Navigate(typeof(UpdateOrderPage), selectedOrder);
            }
        }

        private void viewDetailButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Order selectedOrder)
                {
                    Debug.WriteLine(selectedOrder.Id);
                    Frame.Navigate(typeof(OrderDetailPage), selectedOrder);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private string FormatCustomerName(string customerId, string customerName, string customerPhone)
        {
            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(customerName) && string.IsNullOrEmpty(customerPhone))
            {
                return "Khách hàng chưa đăng ký thành viên";
            }
            return customerName;
        }

        private Visibility ShowInactiveMember(string customerId, string customerName, string customerPhone)
        {
            if (string.IsNullOrEmpty(customerId) && !string.IsNullOrEmpty(customerName) && !string.IsNullOrEmpty(customerPhone))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

    }
}
