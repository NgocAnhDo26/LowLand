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
using Microsoft.UI.Xaml.Navigation;

namespace LowLand.View
{
    public sealed partial class UpdateOrderPage : Page
    {
        public UpdateOrderViewModel ViewModel { get; set; } = new UpdateOrderViewModel();

        public UpdateOrderPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Order order)
            {
                ViewModel.Init(order);



            }
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
            ViewModel.Update(ViewModel.EditorAddOrder);
            ShowMessage("Cập nhật đơn hàng thành công!");
            Frame.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void ShowMessage(string message)
        {
            var dialog = new ContentDialog
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
                    ViewModel.UpdateCustomerInfo(null, "", "");
                    NameBox.Visibility = Visibility.Collapsed;

                    //ViewModel.SelectedTable = ViewModel.AvailableTables.FirstOrDefault(t => t.Id == -1);

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
                var newDetail = ViewModel.CreateOrderDetail(selectedProduct);
                ViewModel.AddOrderDetail(newDetail);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is OrderDetail orderDetail)
            {
                var selectedOption = comboBox.SelectedItem as ProductOption;
                ViewModel.UpdateProductOption(orderDetail, selectedOption);
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
            ViewModel.SelectedPromotion = selectedPromotion;
            Debug.WriteLine($"ChoosePromotion: {selectedPromotion?.Name}, Type: {selectedPromotion?.Type}, Amount: {selectedPromotion?.Amount}, TotalAfterDiscount = {ViewModel.EditorAddOrder.TotalAfterDiscount}, PromotionDiscount = {ViewModel.PromotionDiscountAmount}");
        }

        private void TableSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableSelector.SelectedItem is Table selectedTable)
            {
                ViewModel.SelectTable(selectedTable);
            }

        }
    }
}
