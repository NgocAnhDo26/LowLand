using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class RevenueReportPage : Page
    {
        public RevenueReportViewModel ViewModel { get; } = new RevenueReportViewModel();

        public RevenueReportPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
        }

        private async void AverageTransactionValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ViewModel.LoadAverageChartDataAsync();
        }

        private async void TotalRevenue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ViewModel.LoadTotalChartDataAsync();
        }
    }
}
