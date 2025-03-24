using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LowLand.Model.Product;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductInfoPage : Page
    {
        ProductInfoViewModel ViewModel = new();
        public ProductInfoPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LoadProduct(e.Parameter.ToString()!);
            // Add categories to ProductBasicInfo StackPanel

            if (ViewModel.Product is SingleProduct product)
            {
                var category = new ComboBox
                {
                    Name = "ProductCategory",
                    Header = "Danh mục sản phẩm",
                    SelectedValuePath = "Id",
                    ItemsSource = ViewModel.Categories,
                    ItemTemplate = (DataTemplate)Resources["ComboBoxTemplate1"],
                    VerticalAlignment = VerticalAlignment.Center,
                };
                category.SelectionChanged += Category_SelectionChanged;

                var productType = new ComboBox
                {
                    Name = "ProdProductType",
                    Header = "Loại sản phẩm",
                    SelectedValuePath = "Id",
                    ItemTemplate = (DataTemplate)Resources["ComboBoxTemplate2"],
                    VerticalAlignment = VerticalAlignment.Center,
                };

                // Apply 2-way binding
                productType.SetBinding(ComboBox.ItemsSourceProperty, new Binding
                {
                    Path = new PropertyPath("FilteredProductTypes"),
                    Source = ViewModel,
                    Mode = BindingMode.OneWay
                });

                productType.SelectionChanged += ProductType_SelectionChanged;

                category.SelectedValue = product?.Category?.Id;
                Grid.SetRow(category, 2);
                Grid.SetColumn(category, 0);
                productType.SelectedValue = product?.ProductType?.Id;
                Grid.SetRow(productType, 2);
                Grid.SetColumn(category, 1);

                ProductBasicInfo.Children.Add(category);
                ProductBasicInfo.Children.Add(productType);

                OptionContainerTitle.Text = "Tùy chọn của sản phẩm";
                NewItemButtonText.Text = "Thêm tùy chọn";
                OptionListView.ItemsSource = ViewModel.ProductOptions;
                OptionListView.ItemTemplate = (DataTemplate)Resources["ProductOptionTemplate"];
            }

            base.OnNavigatedTo(e);
        }

        private void ProductType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var type = (sender as ComboBox)!.SelectedItem as ProductType;
            ViewModel.OnProductTypeChange(type!);
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var category = (sender as ComboBox)!.SelectedItem as Category;
            ViewModel.OnCategoryChange(category!);
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.UpdateProduct())
            {
                // Success dialog
                var dialog = new ContentDialog
                {
                    Title = "Cập nhật sản phẩm",
                    Content = "Cập nhật sản phẩm thành công",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProductsPage));
        }

        private async void EditProductOption_Click(object sender, RoutedEventArgs e)
        {
            var selectedOption = (ProductOption)((MenuFlyoutItem)sender).DataContext;
            var editDialog = new ProductOptionDialog(ViewModel, selectedOption.OptionId);
            editDialog.XamlRoot = Content.XamlRoot;
            await editDialog.ShowAsync();
        }
        private async void AddNewOptionButton_Click(object sender, RoutedEventArgs e)
        {
            var addDialog = new ProductOptionDialog(ViewModel, -1);
            addDialog.XamlRoot = Content.XamlRoot;
            await addDialog.ShowAsync();
        }

        private async void DeleteProductOption_Click(object sender, RoutedEventArgs e)
        {
            var selectedOption = (ProductOption)((MenuFlyoutItem)sender).DataContext;

            // Show confirmation dialog
            ContentDialog contentDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa tùy chọn này?",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            var result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.DeleteProductOption(selectedOption.OptionId);
            }       
        }
    }
}
