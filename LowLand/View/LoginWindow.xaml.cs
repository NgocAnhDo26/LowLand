using System.Diagnostics;
using DemoListBinding.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow : Window
    {

        //  private PostgreDAO _dao;
        public LoginViewModel ViewModel { get; set; }
        public LoginWindow()
        {
            ViewModel = new LoginViewModel();
            this.InitializeComponent();
            SystemBackdrop = new MicaBackdrop();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ViewModel.Username + " " + ViewModel.Password);

            if (ViewModel.CanLogin())
            {
                bool success = ViewModel.Login();

                if (success)
                {
                    ViewModel.SaveCredentials(); // Lưu Remember Me nếu cần

                    var dashboard = new DashboardWindow();
                    App.m_window = dashboard;
                    App.MainWindow = dashboard;
                    dashboard.Activate();

                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(dashboard);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                    var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
                    var workArea = displayArea.WorkArea;

                    int targetWidth = (int)(workArea.Width * 0.9);
                    int targetHeight = (int)(workArea.Height * 0.9);

                    appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = targetWidth, Height = targetHeight });

                    var centerX = (workArea.Width - targetWidth) / 2;
                    var centerY = (workArea.Height - targetHeight) / 2;

                    appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });

                    this.Close();
                }
            }
        }

    }
}
