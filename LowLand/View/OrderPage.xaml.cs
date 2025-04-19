using System;
using System.Diagnostics;
using LowLand.Model.Order;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LowLand.View
{
    public sealed partial class OrderPage : Page
    {
        public OrderViewModel ViewModel { get; set; } = new OrderViewModel();

        public OrderPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.Dispose();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            var keyword = searchBar?.Text ?? string.Empty;
            Debug.WriteLine($"Search button clicked, keyword: {keyword}");
            ViewModel.Paging.SearchKeyword = keyword;
            ViewModel.Paging.Refresh();
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
                        Content = $"Bạn có chắc muốn xóa đơn hàng #{selectedOrder.Id}?",
                        PrimaryButtonText = "Xóa",
                        CloseButtonText = "Hủy",
                        DefaultButton = ContentDialogButton.Primary,
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
                Debug.WriteLine(ex.Message);
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

        private void completeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Order selectedOrder)
            {
                selectedOrder.Status = "Hoàn thành";
                selectedOrder.TableId = null;
                ViewModel.Update(selectedOrder);
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Paging.GoToPreviousPage();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Paging.GoToNextPage();
        }

        private void PageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
            {
                ViewModel.Paging.CurrentPage = comboBox.SelectedIndex + 1;
            }
        }

        private async void PrintInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Order selectedOrder)
            {
                await ViewModel.PrintInvoice(selectedOrder, this.XamlRoot);
            }
        }
    }
}