using System;
using LowLand.Model.Product;
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

        private void addNewProductButton_Click(object sender, RoutedEventArgs e)
        {

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
                    // Handle delete product
                    if (ViewModel.RemoveProduct(product.Id))
                    {
                        // Show success message
                        ContentDialog successDialog = new ContentDialog
                        {
                            Title = "Xóa sản phẩm thành công",
                            Content = "Sản phẩm đã được xóa khỏi hệ thống.",
                            CloseButtonText = "Đóng",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await successDialog.ShowAsync();
                    }
                    else
                    {
                        // Show error message
                        ContentDialog errorDialog = new ContentDialog
                        {
                            Title = "Xóa sản phẩm thất bại",
                            Content = "Đã xảy ra lỗi trong quá trình xóa sản phẩm.",
                            CloseButtonText = "Đóng",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                }
            }
        }
    }
}