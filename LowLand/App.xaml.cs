using System;
using System.Diagnostics;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LowLand.Services;
using LowLand.View;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        ///
        public static Window MainWindow { get; set; } = null!;

        public App()
        {
            this.InitializeComponent();
            Services.Services.AddKeyedSingleton<IDao, PostgreDao>();
            UnhandledException += App_UnhandledException;
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "vi-VN";

            LiveCharts.Configure(config =>
                config
                // you can override the theme 
                .AddDarkTheme()
            );
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception: {e.Exception}");
            e.Handled = true;
            // ContentDialog to show error message
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Lỗi",
                Content = e.Exception.Message,
                CloseButtonText = "OK",
                XamlRoot = m_window?.Content.XamlRoot
            };

            await errorDialog.ShowAsync();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            App.m_window = new LoginWindow();
            MainWindow = m_window;
            m_window.Activate();

            // Retrieve the display area of the current window
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
                appWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

            if (displayArea != null)
            {
                // Lấy kích thước làm việc của màn hình
                var workArea = displayArea.WorkArea;

                // Lấy kích thước cửa sổ
                var windowSize = appWindow.Size;

                // Tính tọa độ căn giữa
                int centerX = workArea.X + (workArea.Width - windowSize.Width) / 2;
                int centerY = workArea.Y + (workArea.Height - windowSize.Height) / 2;



                appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });
            }
        }

        public static Window? m_window;
    }
}
