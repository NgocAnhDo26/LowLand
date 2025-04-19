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
                ViewModel.ApplyTimeRangeFilter();
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
    }
}
