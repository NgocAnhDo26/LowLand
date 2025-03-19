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
    public sealed partial class CustomerEditPage : Page
    {
         public CustomerViewModel ViewModel { get; set; } = new CustomerViewModel();

        public CustomerEditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CustomerViewModel viewModel)
            {
                ViewModel = viewModel;
                DataContext = ViewModel;
            }
        }

        private void AddCustomerClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                string.IsNullOrWhiteSpace(phoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(pointTextBox.Text) ||
                rankComboBox.SelectedItem == null)
            {
                ShowMessage("Vui lòng nhập đầy đủ thông tin khách hàng.");
                return;
            }

            if (!int.TryParse(pointTextBox.Text, out int point))
            {
                ShowMessage("Điểm phải là số nguyên.");
                return;
            }

            var newCustomer = new Customer
            {
                Name = nameTextBox.Text,
                Phone = phoneTextBox.Text,
                Point = point,
                RankName = (rankComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                RegistrationDate = DateTime.Now,
                RankId = 1
            };

            ViewModel.Add(newCustomer);
            ClearInputs();
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (customersComboBox.SelectedItem is Customer selectedCustomer)
            {
                ViewModel.Remove(selectedCustomer);
            }
            else
            {
                ShowMessage("Vui lòng chọn khách hàng để xóa.");
            }
        }

        private void UpdateCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (customersComboBox.SelectedItem is Customer selectedCustomer)
            {
                selectedCustomer.Point += 10;
                ViewModel.Update(selectedCustomer);
            }
            else
            {
                ShowMessage("Vui lòng chọn khách hàng để cập nhật.");
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void ShowMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void ClearInputs()
        {
            nameTextBox.Text = "";
            phoneTextBox.Text = "";
            pointTextBox.Text = "";
            rankComboBox.SelectedIndex = -1;
        }
        private void OnCustomerSelected(object sender, SelectionChangedEventArgs e)
        {
            if (customersComboBox.SelectedItem is Customer selectedCustomer)
            {
                nameTextBox.Text = selectedCustomer.Name;
                phoneTextBox.Text = selectedCustomer.Phone;
                pointTextBox.Text = selectedCustomer.Point.ToString();
                rankComboBox.SelectedIndex = GetRankIndex(selectedCustomer.RankName);
            }
        }
        private int GetRankIndex(string rank)
        {
            switch (rank)
            {
                case "Bronze": return 0;
                case "Silver": return 1;
                case "Gold": return 2;
                default: return -1; 
            }
        }

    }
}
