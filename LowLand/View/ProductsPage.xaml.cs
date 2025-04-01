using System;
using LowLand.Model.Product;
using LowLand.Utils;
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
    public sealed partial class ProductsPage : Page
    {
        ProductsViewModel ViewModel { get; set; }

        public ProductsPage()
        {
            this.InitializeComponent();
            ViewModel = new ProductsViewModel();
            DataContext = ViewModel;
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle search button click
        }

        private void AddNewSingleProduct(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame.Navigate(typeof(AddSingleProductPage));
            }
            catch (Exception ex)
            {
                // ContentDialog to show error message
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                errorDialog.ShowAsync();
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var product = menuFlyoutItem?.DataContext as Product;
            if (product != null)
            {
                Frame.Navigate(typeof(ProductInfoPage), product.Id);
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var product = menuFlyoutItem?.DataContext as Product;
            if (product != null)
            {
                // Popup confirmation dialog
                ContentDialog deleteConfirmationDialog = new ContentDialog
                {
                    Title = "Xác nhận xóa",
                    Content = "Bạn có chắc chắn muốn xóa sản phẩm này?",
                    PrimaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                ContentDialogResult result = await deleteConfirmationDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var responseCode = ViewModel.RemoveProduct(product.Id);

                    // Show appropriate messages based on response code
                    string message = responseCode switch
                    {
                        ResponseCode.Success => "Xóa sản phẩm thành công!",
                        ResponseCode.Error => "Đã xảy ra lỗi khi xóa sản phẩm!",
                        _ => "Lỗi không xác định!"
                    };

                    ContentDialog infoDialog = new ContentDialog
                    {
                        Title = "Thông báo",
                        Content = message,
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };

                    await infoDialog.ShowAsync();
                }
            }
        }
    }
}