using System;
using System.Collections.Generic;
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

            if (ViewModel.EditorAddOrder.CustomerId == 0 || string.IsNullOrEmpty(ViewModel.EditorAddOrder.CustomerPhone))
            {
                ShowMessage("Vui lòng chọn khách hàng!");
                return;
            }

            ViewModel.Add(ViewModel.EditorAddOrder);
            ShowMessage("Cập nhật thông tin khách hàng thành công!");
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
                ViewModel.EditorAddOrder.CustomerId = selectedCustomer.Id;
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
                    orderDetail.quantity = quantity;
                    orderDetail.Price = orderDetail.ProductPrice * quantity;

                    var productTotalText = textBox.FindName("ProductTotalText") as TextBlock;
                    if (productTotalText != null)
                    {
                        productTotalText.Text = $"{orderDetail.Price}";
                    }

                    if (ViewModel.EditorAddOrder.Details != null)
                    {
                        TotalAmountValue.Text = $"{ViewModel.EditorAddOrder.Details.Sum(d => d.Price)}";
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
                if (ViewModel.EditorAddOrder.Details != null)
                {
                    ViewModel.EditorAddOrder.Details.Add(newOrderDetail);

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
                }
                TotalAmountValue.Text = $"{ViewModel.EditorAddOrder.TotalPrice} VND";
            }
        }


        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderDetail orderDetail)
            {
                if (orderDetail != null)
                {
                    ViewModel.EditorAddOrder.Details.Remove(orderDetail);
                    ViewModel.EditorAddOrder.TotalPrice = ViewModel.EditorAddOrder.Details.Sum(d => d.Price);
                    TotalAmountValue.Text = $"{ViewModel.EditorAddOrder.TotalPrice} VND";
                }
            }
        }


    }
}
