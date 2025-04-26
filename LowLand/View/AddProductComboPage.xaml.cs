using System;
using System.Collections.Generic;
using LowLand.Model.Product;
using LowLand.Utils;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddProductComboPage : Page
    {
        private AddProductComboViewModel ViewModel;
        public AddProductComboPage()
        {
            ViewModel = new AddProductComboViewModel();
            DataContext = ViewModel;
            this.InitializeComponent();
        }

        /// <summary>
        /// Event handler when user searching for a product in the AutoSuggestBox, used in combo creation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

            var selectedItemString = args.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedItemString) || selectedItemString == "Không tìm thấy sản phẩm nào...")
            {

                return;
            }

            // Extract the product ID from the selected item string
            var productId = selectedItemString.Split('-')[0].Trim();
            if (int.TryParse(productId, out int parsedProductId))
            {
                ViewModel.AddProductToCombo(parsedProductId);
            }

            // Clear the AutoSuggestBox
            sender.Text = string.Empty;
        }

        private void RemoveChildProductButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Get the product to remove from the button's DataContext
            var item = (ComboItem)((Button)sender).DataContext;
            ViewModel.RemoveItemFromCombo(item);
        }
        private void ProductOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set the selected option for the product
            var item = (ComboItem)((ComboBox)sender).DataContext;
            var selectedOption = (ProductOption)((ComboBox)sender).SelectedItem;

            if (selectedOption != null)
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
        private async void PickProductImage(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Safely cast the sender to a Button and check for null
            if (sender is Button senderButton)
            {
                // Disable the button to avoid double-clicking
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

                // Re-enable the button
                senderButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProductsPage));
        }

        private async void ApplyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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

            var response = ViewModel.AddCombo();

            // Show appropriate messages based on response code
            string message = response switch
            {
                ResponseCode.Success => "Thêm combo thành công!",
                ResponseCode.EmptyName => "Tên combo không được để trống!",
                ResponseCode.InvalidValue => "Giá bán của combo không được thấp hơn tổng giá vốn!",
                ResponseCode.NoChildProduct => "Combo phải có ít nhất 1 sản phẩm con!",
                ResponseCode.Error => "Đã xảy ra lỗi khi thêm combo!",
                _ => "Lỗi không xác định!"
            };

            // Popup message dialog
            ContentDialog infoDialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await infoDialog.ShowAsync();

            // Navigate back to the products page
            if (response == ResponseCode.Success)
            {
                Frame.Navigate(typeof(ProductsPage));
            }
        }
    }
}
