using System;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;
namespace LowLand.View
{
    /// <summary>
    /// A page for product-related reports.
    /// </summary>
    public sealed partial class ProductReportPage : Page
    {
        private ProductReportViewModel ViewModel = new ProductReportViewModel();
        public ProductReportPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        private async void ApplyTimeRangeFilterButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Apply the time range filter
            try
            {
                await ViewModel.ApplyTimeRangeFilter();
            }
            catch (Exception ex)
            {
                // Show error message if the filter fails
                var errorDialog = new ContentDialog
                {
                    Title = "Bộ lọc không hợp lệ!",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                await errorDialog.ShowAsync();
            }
        }

        private async void RetrainModelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Open a dialog to confirm the retraining
            var confirmDialog = new ContentDialog
            {
                Title = "Xác nhận",
                Content = "Bạn có chắc chắn muốn huấn luyện lại mô hình không?",
                PrimaryButtonText = "Có",
                IsPrimaryButtonEnabled = true,
                CloseButtonText = "Không",
                XamlRoot = this.Content.XamlRoot
            };

            // Show the dialog and wait for the user's response
            var result = await confirmDialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // Start the retraining process
            try
            {
                await ViewModel.RetrainModel();
            }
            catch (Exception ex)
            {
                // Show error message if the retraining fails
                var errorDialog = new ContentDialog
                {
                    Title = "Huấn luyện lại mô hình thất bại!",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
            finally
            {
                // Repredict the products after retraining
                await ViewModel.PredictTopProducts();

                var successDialog = new ContentDialog
                {
                    Title = "Huấn luyện lại mô hình thành công!",
                    Content = "Mô hình đã được huấn luyện lại thành công.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }
    }
}
