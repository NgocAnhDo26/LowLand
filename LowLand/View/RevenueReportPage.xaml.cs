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

        private void AverageTransactionValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.LoadAverageChartData();
        }

        private void TotalRevenue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.LoadTotalChartData();
        }
    }
}
