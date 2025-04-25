using System;
using System.Diagnostics;
using LowLand.Model.Customer;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class CustomerPage : Page
    {
        public CustomerViewModel ViewModel { get; set; } = new CustomerViewModel();

        public CustomerPage()
        {
            this.InitializeComponent();
        }

        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            var keyword = searchBar?.Text?.Trim() ?? string.Empty;
            Debug.WriteLine($"Search button clicked, keyword: '{keyword}'");
            ViewModel.Paging.SearchKeyword = keyword;
            await ViewModel.Paging.RefreshAsync();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddCustomerPage));
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Customer selectedCustomer)
                {
                    ContentDialog deleteDialog = new ContentDialog
                    {
                        Title = "Xác nhận xóa",
                        Content = $"Bạn có chắc muốn xóa khách hàng {selectedCustomer.Name}?",
                        PrimaryButtonText = "Xóa",
                        CloseButtonText = "Hủy",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = this.XamlRoot
                    };

                    ContentDialogResult result = await deleteDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        ViewModel.Remove(selectedCustomer);
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
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Customer selectedCustomer)
            {
                Frame.Navigate(typeof(UpdateCustomerPage), selectedCustomer.Id);
            }
        }

        private async void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.Paging.GoToPreviousPageAsync();
        }

        private async void NextPage_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.Paging.GoToNextPageAsync();
        }

        private void PageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
            {
                Debug.WriteLine($"PageSelector changed, SelectedIndex: {comboBox.SelectedIndex}, Setting CurrentPage: {comboBox.SelectedIndex + 1}");
                ViewModel.Paging.CurrentPage = comboBox.SelectedIndex + 1;
            }
        }
    }
}