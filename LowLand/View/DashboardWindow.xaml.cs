using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class DashboardWindow : Window
    {

        public DashboardWindow()
        {
            this.InitializeComponent();
            SystemBackdrop = new DesktopAcrylicBackdrop();
            MainFrame.Navigate(typeof(DashboardPage));
        }
    }
}
