using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LowLand.Model.Product;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductsPage : Page
    {
        ProductsViewModel ViewModel { get; set; }

        public ProductsPage()
        {
            this.InitializeComponent();
            ViewModel = new ProductsViewModel();
            DataContext = ViewModel;
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle search button click
        }

        private void addNewProductButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle add new product button click
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var product = menuFlyoutItem?.DataContext as Product;
            if (product != null)
            {
                Frame.Navigate(typeof(ProductInfoPage), product.Id);
            }
        }
    }
}
