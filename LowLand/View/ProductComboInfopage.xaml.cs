using System;
using System.Collections.Generic;
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
    public sealed partial class ProductComboInfopage : Page
    {
        ProductComboInfoViewModel ViewModel = new();

        public ProductComboInfopage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;

            ComboSalePriceBox.NumberFormatter = new NumberFormatter();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.LoadProduct(e.Parameter.ToString()!);
            base.OnNavigatedTo(e);
        }

        private void FindProductBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Since selecting an item will also change the text,
            // only listen to changes caused by user entering text.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var prompt = sender.Text.ToLower().Trim();

                List<string> suitableItems = ViewModel.SearchForProducts(prompt);
                sender.ItemsSource = suitableItems;
            }
        }

        private void FindProductBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Add the selected item to the list of selected products
            var selectedItemString = args.SelectedItem.ToString();

            if (selectedItemString == "Không tìm thấy sản phẩm nào...")
            {
                // If no product is found, do nothing
                return;
            }

            // Extract the product ID from the selected item string
            var productId = selectedItemString.Split('-')[0].Trim();
            ViewModel.AddProductToCombo(int.Parse(productId));

            // Clear the AutoSuggestBox
            sender.Text = string.Empty;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComboSalePriceBox.Text == null || ComboSalePriceBox.Text == "")
            {
                var dialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = "Giá bán không được để trống!",
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

        private void ProductOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set the selected option for the product
            var item = (ComboItem)((ComboBox)sender).DataContext;
            var selectedOption = (ProductOption)((ComboBox)sender).SelectedItem;

            if (selectedOption != null && item != null)
            {
                ViewModel.SetProductOption(item, selectedOption);
            }
        }
        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var item = (ComboItem)sender.DataContext;
            if (item != null)
            {
                item.Quantity = (int)sender.Value;
                ViewModel.CalculatePrice();
            }
        }

        private void RemoveChildProductButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (ComboItem)((Button)sender).DataContext;
            if (item != null)
            {
                ViewModel.RemoveItemFromCombo(item);
            }
        }
    }
}
