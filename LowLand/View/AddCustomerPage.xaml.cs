using System;
using LowLand.Model.Customer;
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
    public sealed partial class AddCustomerPage : Page
    {
        private CustomerViewModel customerVM;

        public AddCustomerPage()
        {
            this.InitializeComponent();
            customerVM = new CustomerViewModel();
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

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ShowMessage("Tên không được để trống!");
                return;
            }
            // check format phone 10 or 11 number @"(\d{4,5})(\d{3})(\d{3})",
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                ShowMessage("Số điện thoại không hợp lệ!");
                return;
            }
            // check phone number is exist 
            if (customerVM.IsPhoneNumberExists(phone))
            {
                ShowMessage("Số điện thoại đã tồn tại!");
                return;
            }

            var newCustomer = new Customer
            {

                Name = name,
                Phone = phone,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
                Rank = new CustomerRank
                {
                    Id = 1,
                    Name = "",
                    DiscountPercentage = 0,
                    PromotionPoint = 0
                },
                Point = 0

            };

            customerVM.Add(newCustomer);

            ShowMessage("Thêm khách hàng thành công!");
            Frame.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

