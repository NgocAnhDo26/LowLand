using System;
using LowLand.Model.Discount;
using LowLand.Utils;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PromotionsPage : Page
    {
        public PromotionsViewModel ViewModel { get; set; } = new PromotionsViewModel();
        public PromotionsPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        private void addButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddPromotionPage), new Promotion()
            {
                Id = -1,
                Name = "KMMOI123",
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now),
            });
        }

        private void updateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Convert the sender to Promotion
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var selectedPromotion = menuFlyoutItem?.DataContext as Promotion;

            // Navigate to the AddPromotionPage with the selected promotion
            Frame.Navigate(typeof(AddPromotionPage), selectedPromotion);
        }

        private void deleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Convert the sender to Promotion
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var selectedPromotion = menuFlyoutItem?.DataContext as Promotion;

            // Show confirmation dialog
            var dialog = new ContentDialog
            {
                Title = "Xóa khuyến mãi",
                Content = $"Bạn có chắc chắn muốn xóa khuyến mãi {selectedPromotion.Name} không?",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy"
            };

            dialog.XamlRoot = this.XamlRoot;
            var result = dialog.ShowAsync();
            result.Completed = (asyncOperation, asyncStatus) =>
            {
                if (result.Status == Windows.Foundation.AsyncStatus.Completed)
                {
                    // If the user confirms, delete the promotion
                    if (result.GetResults() == ContentDialogResult.Primary)
                    {
                        var response = ViewModel.DeletePromotion(selectedPromotion);

                        // Show appropriate messages based on response code
                        string message = response switch
                        {
                            ResponseCode.Success => "Xóa khuyến mãi thành công!",
                            ResponseCode.Error => "Đã xảy ra lỗi khi xóa khuyến mãi!",
                            _ => "Có lỗi xảy ra!"
                        };

                        var messageDialog = new ContentDialog
                        {
                            Title = "Thông báo",
                            Content = message,
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };

                        messageDialog.ShowAsync().Completed = (asyncOperation, asyncStatus) =>
                        {
                            if (messageDialog != null)
                            {
                                messageDialog.Hide();
                            }
                        };
                    }
                }
            };
        }
    }
}
