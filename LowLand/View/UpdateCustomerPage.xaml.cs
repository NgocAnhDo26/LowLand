using System;
using System.Linq;
using System.Threading.Tasks;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateCustomerPage : Page
    {
        private CustomerViewModel ViewModel = new CustomerViewModel();


        public UpdateCustomerPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int customerId)
            {
                ViewModel.EditingCustomer = ViewModel.Customers.FirstOrDefault(c => c.Id == customerId);
                if (ViewModel.EditingCustomer != null)
                {
                    NameBox.Text = ViewModel.EditingCustomer.Name;
                    PhoneBox.Text = ViewModel.EditingCustomer.Phone;
                    PointBox.Text = ViewModel.EditingCustomer.Point.ToString();
                    RankNameBox.Text = ViewModel.EditingCustomer.Rank.Name;
                }
            }
        }



        private async Task<ContentDialogResult> ShowMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                PrimaryButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            return await dialog.ShowAsync();
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.EditingCustomer != null)
            {
                // check
                if (string.IsNullOrWhiteSpace(NameBox.Text))
                {
                    await ShowMessage("Tên không được để trống!");
                    return;
                }
                if (!System.Text.RegularExpressions.Regex.IsMatch(PhoneBox.Text, @"^0\d{9,10}$"))
                {
                    await ShowMessage("Số điện thoại không hợp lệ!");
                    return;
                }
                if (!int.TryParse(PointBox.Text, out int point) || point < 0)
                {
                    await ShowMessage("Điểm phải là số nguyên và lớn hơn hoặc bằng 0!");
                    return;
                }

                ViewModel.EditingCustomer.Name = NameBox.Text;
                ViewModel.EditingCustomer.Phone = PhoneBox.Text;
                ViewModel.EditingCustomer.Point = point;

                ViewModel.Update(ViewModel.EditingCustomer);

                var result = await ShowMessage("Cập nhật thông tin khách hàng thành công!");
                if (result == ContentDialogResult.Primary)
                {
                    //  Frame.GoBack();
                }
            }
            else
            {
                var result = await ShowMessage("Cập nhật thông tin khách hàng thất bại!");
                if (result == ContentDialogResult.Primary)
                {
                    // Frame.GoBack();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
