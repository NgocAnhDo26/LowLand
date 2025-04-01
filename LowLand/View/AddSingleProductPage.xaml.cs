using System;
using LowLand.Model.Product;
using LowLand.Utils;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddSingleProductPage : Page
    {
        private AddSingleProductViewModel ViewModel;
        public AddSingleProductPage()
        {
            ViewModel = new AddSingleProductViewModel();
            DataContext = ViewModel;
            this.InitializeComponent();
        }

        private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProductsPage));
        }

        private async void ApplyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var responseCode = ViewModel.AddProduct();

            // Show appropriate messages based on response code
            string message = responseCode switch
            {
                ResponseCode.Success => "Thêm sản phẩm thành công!",
                ResponseCode.EmptyName => "Tên sản phẩm không được để trống!",
                ResponseCode.NegativeValueNotAllowed => "Giá bán và giá vốn phải lớn hơn 0!",
                ResponseCode.InvalidValue => "Giá bán không được thấp hơn giá vốn!",
                ResponseCode.CategoryEmpty => "Vui lòng chọn danh mục cho sản phẩm!",
                ResponseCode.Error => "Lỗi khi thêm sản phẩm!",
                _ => "Lỗi không xác định!"
            };

            var infoDialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await infoDialog.ShowAsync();

            // Navigate back to the products page
            if (responseCode == ResponseCode.Success)
            {
                Frame.Navigate(typeof(ProductsPage));
            }
        }

        private async void PickProductImage(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

        private void Category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category category = (sender as ComboBox).SelectedItem as Category;
            ViewModel.ChangeProductCategory(category!);
        }
    }
}
