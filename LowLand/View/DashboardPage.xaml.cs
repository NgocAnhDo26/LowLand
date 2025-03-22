using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            this.InitializeComponent();
        }

        private void navigation_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                // = "Settings clicked";
                return;
            }

            var item = (NavigationViewItem)sender.SelectedItem;

            if (item.Tag != null)
            {
                string tag = (string)item.Tag;
                Debug.WriteLine(tag);
                Debug.WriteLine($"{GetType().Namespace}.{tag}");
                container.Navigate(Type.GetType($"{GetType().Namespace}.{tag}"));
            }
        }

        private void navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }
    }
}
