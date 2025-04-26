using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Discount;
using LowLand.Model.Order;
using LowLand.Model.Product;
using LowLand.Model.Table;
using LowLand.Utils;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class AddOrderPage : Page
    {
        public AddOrderViewModel ViewModel { get; set; } = new AddOrderViewModel();

        public AddOrderPage()
        {
            this.InitializeComponent();
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.EditorAddOrder.Details == null || ViewModel.EditorAddOrder.Details.Count == 0)
            {
                ShowMessage("Vui lòng chọn sản phẩm cho đơn hàng!");
                return;
            }
            if (NameBox.Visibility == Visibility.Visible && string.IsNullOrEmpty(ViewModel.EditorAddOrder.CustomerName))
            {
                ShowMessage("Vui lòng nhập tên khách hàng!");
                return;
            }
            if (ViewModel.EditorAddOrder.CustomerId == 0)
            {
                ResponseCode result = ViewModel.addWithNewCustomer();
                if (result == ResponseCode.ExistsCustomer)
                {
                    ShowMessage("Khách hàng đã tồn tại!");
                    return;
                }
                if (result == ResponseCode.NotFound)
                {
                    ShowMessage("Không tìm thấy khách hàng!");
                    return;
                }
                if (result == ResponseCode.InvalidValue)
                {
                    ShowMessage("Số điện thoại không hợp lệ!");
                    return;
                }
            }

            Debug.WriteLine($"Creating order: TotalPrice = {ViewModel.EditorAddOrder.TotalPrice}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, RankDiscount = {ViewModel.RankDiscountAmount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
            ViewModel.Add();
            ShowMessage("Tạo đơn hàng thành công!");
            Frame.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void ShowMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // Handle when the user clears the content (presses the "X" button)
                if (string.IsNullOrEmpty(sender.Text))
                {
                    ViewModel.UpdateCustomerInfo(null, "", "");
                    NameBox.Visibility = Visibility.Collapsed;

                    ViewModel.SelectedTable = ViewModel.AvailableTables.FirstOrDefault(t => t.Id == -1)!;

                    Debug.WriteLine("AutoSuggestBox cleared: Set to vãng lai");
                    return;
                }

                // Handle search suggestions
                var suitableItems = new List<string>();
                var inputText = sender.Text.ToLower();

                foreach (var customer in ViewModel.Customers)
                {
                    if (!string.IsNullOrEmpty(customer.Phone) && customer.Phone.Contains(inputText, StringComparison.OrdinalIgnoreCase))
                    {
                        suitableItems.Add($"{customer.Phone} - {customer.Name}");
                    }
                }

                if (!string.IsNullOrEmpty(inputText))
                {
                    suitableItems.Add($"{inputText}");
                }

                sender.ItemsSource = suitableItems;
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedText = args.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedText) && selectedText.Contains(" - "))
            {
                var parts = selectedText.Split(" - ");
                var phone = parts[0].Trim();
                var selectedCustomer = ViewModel.Customers.FirstOrDefault(c => c.Phone == phone);

                if (selectedCustomer != null)
                {
                    // Ensure that selectedCustomer.Phone is not null before passing it
                    ViewModel.UpdateCustomerInfo(selectedCustomer.Id, selectedCustomer.Phone ?? string.Empty, selectedCustomer.Name);
                    NameBox.Visibility = Visibility.Collapsed;
                }
            }
            else if (!string.IsNullOrEmpty(selectedText))
            {
                var phone = selectedText.Trim();
                ViewModel.UpdateCustomerInfo(0, phone, "");
                NameBox.Visibility = Visibility.Visible;
            }
        }
        private void TableSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableSelector.SelectedItem is Table selectedTable)
            {
                ViewModel.SelectTable(selectedTable);
            }

        }




        private void ProductGridView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var selectedProduct = (sender as GridView)?.SelectedItem as Product;

            if (selectedProduct != null)
            {
                var newDetail = ViewModel.CreateOrderDetail(selectedProduct);
                ViewModel.AddOrderDetail(newDetail);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is OrderDetail orderDetail)
            {
                var selectedOption = comboBox.SelectedItem as ProductOption;


                if (selectedOption != null)
                {
                    ViewModel.UpdateProductOption(orderDetail, selectedOption);
                }
                else
                {
                    Debug.WriteLine("ComboBox_SelectionChanged: selectedOption is null.");
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderDetail detail)
            {
                ViewModel.RemoveOrderDetail(detail);
            }
        }

        private void ProductQuantityTextBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender is NumberBox numberBox && numberBox.DataContext is OrderDetail detail)
            {
                if (int.TryParse(numberBox.Text, out int quantity))
                {
                    ViewModel.UpdateQuantity(detail, quantity);
                }
            }
        }


        private void ChoosePromotion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPromotion = (sender as ComboBox)?.SelectedItem as Promotion;


            if (selectedPromotion != null)
            {
                ViewModel.SelectedPromotion = selectedPromotion;
            }

            Debug.WriteLine($"ChoosePromotion: {selectedPromotion?.Name}, Type: {selectedPromotion?.Type}, Amount: {selectedPromotion?.Amount}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
        }

        private void ClearTableSelection_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                ViewModel.SelectedCategories = listBox.SelectedItems.Cast<Category>().ToList();
                ViewModel.ApplyProductFilter();
            }
        }



        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ViewModel.SearchKeyword = textBox.Text?.Trim() ?? string.Empty;
                ViewModel.ApplyProductFilter();
            }
        }


        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Xóa từ khóa tìm kiếm
            ViewModel.SearchKeyword = string.Empty;
            SearchBox.Text = string.Empty;

            // Xóa danh mục đã chọn
            CategoryListBox.SelectedItems.Clear();
            ViewModel.SelectedCategories.Clear();

            // Cập nhật lại danh sách
            ViewModel.ApplyProductFilter();
        }

    }
}