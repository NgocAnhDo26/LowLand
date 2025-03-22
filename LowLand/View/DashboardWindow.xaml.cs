using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LowLand.View;
using Microsoft.UI.Composition.SystemBackdrops;
using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()
using LowLand.Utils;


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
        private WindowsSystemDispatcherQueueHelper? _wsdqHelper;

        // See separate sample below for implementation
        private DesktopAcrylicController? _acrylicController;

        private SystemBackdropConfiguration? _configurationSource;

        public DashboardWindow()
        {
            this.InitializeComponent();
            var result = TrySetAcrylicBackdrop();
            MainFrame.Navigate(typeof(DashboardPage));
        }

        private bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported() is true)
            {
                _wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                _wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hooking up the policy object
                _configurationSource = new SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                _configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                _acrylicController = new DesktopAcrylicController();

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                _acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_configurationSource);
                return true; // succeeded
            }

            return false; // Acrylic is not supported on this system
        }
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_configurationSource is not null)
            {
                _configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (_acrylicController is not null)
            {
                _acrylicController.Dispose();
                _acrylicController = null;
            }

            this.Activated -= Window_Activated;
            _configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (_configurationSource is not null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            if (_configurationSource is not null)
            {
                switch (((FrameworkElement)this.Content).ActualTheme)
                {
                    case ElementTheme.Dark:
                        _configurationSource.Theme = SystemBackdropTheme.Dark;
                        break;
                    case ElementTheme.Light:
                        _configurationSource.Theme = SystemBackdropTheme.Light;
                        break;
                    case ElementTheme.Default:
                        _configurationSource.Theme = SystemBackdropTheme.Default;
                        break;
                }
            }
        }
    }
}
