using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Order;
using LowLand.Model.Product;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddOrderPage : Page
    {
        public OrderViewModel ViewModel { get; set; } = new OrderViewModel();
        public AddOrderPage()
        {
            this.InitializeComponent();
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

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


        // lay danh sach sdt-ten khach hang


        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<string>();
                var inputText = sender.Text.ToLower();

                foreach (var customer in ViewModel.Customers)
                {
                    if (customer.Phone.ToLower().Contains(inputText))
                    {
                        suitableItems.Add($"{customer.Phone} - {customer.Name}");
                    }
                }

                if (suitableItems.Count == 0)
                {
                    suitableItems.Add("No results found");
                }

                sender.ItemsSource = suitableItems;
            }
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var selectedText = args.SelectedItem.ToString();

            if (selectedText == "No results found") return;

            var phoneNumber = selectedText.Split(" - ")[0];


            var selectedCustomer = ViewModel.Customers.FirstOrDefault(c => c.Phone == phoneNumber);

            if (selectedCustomer != null)
            {
                ViewModel.EditorAddOrder.CustomerId = selectedCustomer.Id; // Gán CustomerId
                ViewModel.EditorAddOrder.CustomerPhone = selectedCustomer.Phone;
                ViewModel.EditorAddOrder.CustomerName = selectedCustomer.Name;


            }

        }





        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is OrderDetail orderDetail)
            {
                if (int.TryParse(textBox.Text, out int quantity))
                {
                    // Cập nhật số lượng và giá
                    orderDetail.quantity = quantity;
                    orderDetail.Price = orderDetail.ProductPrice * quantity;

                    // Ensure ProductTotalText is defined in the context
                    var productTotalText = textBox.FindName("ProductTotalText") as TextBlock;
                    if (productTotalText != null)
                    {
                        productTotalText.Text = $"{orderDetail.Price}";
                    }

                    // Cập nhật tổng tiền
                    TotalAmountValue.Text = $"{ViewModel.EditorAddOrder.Details.Sum(d => d.Price)}";

                    // Debug log
                    foreach (var detail in ViewModel.EditorAddOrder.Details)
                    {
                        Debug.WriteLine($"🛒 {detail.ProductName} - {detail.quantity} - {detail.Price}");
                    }
                }
            }
        }


        private void ProductGridView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var selectedProduct = (sender as GridView)?.SelectedItem as Product;

            if (selectedProduct != null)
            {
                var newOrderDetail = ViewModel.CreateOrderDetail(selectedProduct);
                ViewModel.EditorAddOrder.Details.Add(newOrderDetail);
                Debug.WriteLine($"🛒 Added Product (DoubleTap): {selectedProduct.Name}");
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is ProductOption productOption)
            {
                // Cập nhật giá của sản phẩm dựa vào lựa chọn mới
                productOption.Selected = comboBox.SelectedItem as string;

                // Cập nhật tổng tiền vào ViewModel
                ViewModel.EditorAddOrder.TotalPrice = ViewModel.EditorAddOrder.Details.Sum(d => d.Price);

                // Cập nhật UI (nếu cần)
                TotalAmountValue.Text = $"{ViewModel.EditorAddOrder.TotalPrice:N0}";
            }
        }


        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}
