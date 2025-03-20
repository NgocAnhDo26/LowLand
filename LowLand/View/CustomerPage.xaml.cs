using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using LowLand.View.ViewModel;
using LowLand.Model.Product;
using LowLand.Model.Customer;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerPage : Page
    {
        public CustomerViewModel ViewModel { get; set; } = new CustomerViewModel();

        public CustomerPage()
        {
            this.InitializeComponent();
        }

        

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {

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
                Console.WriteLine(ex.Message);
            }

        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Customer selectedCustomer)
            {
                Frame.Navigate(typeof(UpdateCustomerPage), selectedCustomer.Id);
            }
        }

    }
}

