using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Discount;
using LowLand.Model.Order;
using LowLand.Model.Product;
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
                // Xử lý khi người dùng xóa hết nội dung (nhấn nút "X")
                if (string.IsNullOrEmpty(sender.Text))
                {
                    ViewModel.UpdateCustomerInfo(0, "", "");
                    NameBox.Visibility = Visibility.Visible;
                    Debug.WriteLine("AutoSuggestBox cleared: Set to vãng lai");
                    return;
                }

                // Xử lý gợi ý tìm kiếm
                var suitableItems = new List<string>();
                var inputText = sender.Text.ToLower();

                foreach (var customer in ViewModel.Customers)
                {
                    if (customer.Phone.ToLower().Contains(inputText))
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
            var selectedText = args.SelectedItem.ToString();

            if (selectedText.Contains(" - "))
            {
                var parts = selectedText.Split(" - ");
                var phone = parts[0].Trim();
                var selectedCustomer = ViewModel.Customers.FirstOrDefault(c => c.Phone == phone);

                if (selectedCustomer != null)
                {
                    ViewModel.UpdateCustomerInfo(selectedCustomer.Id, selectedCustomer.Phone, selectedCustomer.Name);
                    NameBox.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                var phone = selectedText.Trim();
                ViewModel.UpdateCustomerInfo(0, phone, "");
                NameBox.Visibility = Visibility.Visible;
            }
        }

        private void ProductGridView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var selectedProduct = (sender as GridView)?.SelectedItem as Product;

            if (selectedProduct != null)
            {
                var newOrderDetail = ViewModel.CreateOrderDetail(selectedProduct);
                if (ViewModel.EditorAddOrder.Details != null)
                {
                    ViewModel.EditorAddOrder.Details.Add(newOrderDetail);
                    ViewModel.EditorAddOrder.TotalPrice = ViewModel.EditorAddOrder.Details.Sum(d => d.Price);
                    Debug.WriteLine($"Product added: TotalPrice = {ViewModel.EditorAddOrder.TotalPrice}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, RankDiscount = {ViewModel.RankDiscountAmount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is OrderDetail orderDetail)
            {
                var selectedOption = comboBox.SelectedItem as ProductOption;
                if (selectedOption != null)
                {
                    orderDetail.OptionId = selectedOption.OptionId;
                    orderDetail.OptionName = selectedOption.Name;
                    orderDetail.ProductPrice = selectedOption.SalePrice;
                    orderDetail.Price = orderDetail.ProductPrice * orderDetail.quantity;
                }

                if (ViewModel.EditorAddOrder.Details != null)
                {
                    ViewModel.EditorAddOrder.TotalPrice = ViewModel.EditorAddOrder.Details.Sum(d => d.Price);
                    Debug.WriteLine($"Option changed: TotalPrice = {ViewModel.EditorAddOrder.TotalPrice}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, RankDiscount = {ViewModel.RankDiscountAmount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is OrderDetail orderDetail)
            {
                if (orderDetail != null && ViewModel.EditorAddOrder.Details != null)
                {
                    ViewModel.EditorAddOrder.Details.Remove(orderDetail);
                    ViewModel.EditorAddOrder.TotalPrice = ViewModel.EditorAddOrder.Details.Sum(d => d.Price);
                    Debug.WriteLine($"Product deleted: TotalPrice = {ViewModel.EditorAddOrder.TotalPrice}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, RankDiscount = {ViewModel.RankDiscountAmount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
                }
            }
        }

        private void ProductQuantityTextBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender is NumberBox numberBox && numberBox.DataContext is OrderDetail orderDetail)
            {
                if (int.TryParse(numberBox.Text, out int quantity))
                {
                    orderDetail.quantity = quantity;
                    orderDetail.Price = orderDetail.ProductPrice * quantity;

                    if (ViewModel.EditorAddOrder.Details != null)
                    {
                        ViewModel.EditorAddOrder.TotalPrice = ViewModel.EditorAddOrder.Details.Sum(d => d.Price);
                        Debug.WriteLine($"Quantity changed: TotalPrice = {ViewModel.EditorAddOrder.TotalPrice}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, RankDiscount = {ViewModel.RankDiscountAmount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
                    }
                }
            }
        }

        private void ChoosePromotion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPromotion = (sender as ComboBox)?.SelectedItem as Promotion;
            ViewModel.SelectedPromotion = selectedPromotion;
            Debug.WriteLine($"ChoosePromotion: {selectedPromotion?.Name}, Type: {selectedPromotion?.Type}, Amount: {selectedPromotion?.Amount}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
        }
    }
}