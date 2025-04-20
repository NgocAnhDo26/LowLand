using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// Trang báo cáo tổng quan
    /// </summary>
    public sealed partial class OverviewReportPage : Page
    {
        private OverviewReportViewModel ViewModel = new OverviewReportViewModel();
        public OverviewReportPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
