using System;
using LowLand.Model.Product;
using LowLand.Utils;
using LowLand.View.Converter;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;

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
            SalePriceBox.NumberFormatter = new NumberFormatter();
            CostPriceBox.NumberFormatter = new NumberFormatter();
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

                category.SelectedValue = product?.Category?.Id;
                Grid.SetRow(category, 2);
                Grid.SetColumn(category, 0);

                ProductBasicInfo.Children.Add(category);

                OptionContainerTitle.Text = "Tùy chọn của sản phẩm";
                NewItemButtonText.Text = "Thêm tùy chọn";
                OptionListView.ItemsSource = ViewModel.ProductOptions;
                OptionListView.ItemTemplate = (DataTemplate)Resources["ProductOptionTemplate"];
            }

            base.OnNavigatedTo(e);
        }

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var category = (sender as ComboBox)!.SelectedItem as Category;
            ViewModel.OnCategoryChange(category!);
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalePriceBox.Text == null || SalePriceBox.Text == "" || CostPriceBox.Text == null || CostPriceBox.Text == "")
            {
                var dialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = "Giá bán và giá vốn không được để trống!",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var responseCode = ViewModel.UpdateProduct();

            // Show appropriate messages based on response code
            string message = responseCode switch
            {
                ResponseCode.Success => "Cập nhật sản phẩm thành công!",
                ResponseCode.EmptyName => "Tên sản phẩm không được để trống!",
                ResponseCode.NegativeValueNotAllowed => "Giá bán và giá vốn phải lớn hơn 0!",
                ResponseCode.InvalidValue => "Giá bán không được thấp hơn giá vốn!",
                ResponseCode.Error => "Đã xảy ra lỗi khi cập nhật sản phẩm!",
                _ => "Lỗi không xác định!"
            };

            var infoDialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };

            await infoDialog.ShowAsync();
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

        private async void ChooseNewImageFile(object sender, RoutedEventArgs e)
        {
            //disable the button to avoid double-clicking
            var senderButton = sender as Button;
            senderButton.IsEnabled = false;

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // Get the current window
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                // Update the image in the view model
                ViewModel.UpdateProductImage(file.Path, file.Name);
            }

            //re-enable the button
            senderButton.IsEnabled = true;
        }
    }
}
