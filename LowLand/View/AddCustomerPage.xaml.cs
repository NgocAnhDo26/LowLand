using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using LowLand.Model.Customer;
using LowLand.View.ViewModel;

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

            var newCustomer = new Customer
            {
                Name = name,
                Phone = phone,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
                RankId = 1,
                Point = 0

            };

            customerVM.Add(newCustomer);

            ShowMessage("Thêm khách hàng thành công!");
            Frame.GoBack();
        }
    }
}
