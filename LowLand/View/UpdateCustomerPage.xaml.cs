using System;
using System.Linq;
using System.Threading.Tasks;
using LowLand.Model.Customer;
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
        private Customer EditingCustomer;

        public UpdateCustomerPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int customerId)
            {
                EditingCustomer = ViewModel.Customers.FirstOrDefault(c => c.Id == customerId);
                if (EditingCustomer != null)
                {
                    NameBox.Text = EditingCustomer.Name;
                    PhoneBox.Text = EditingCustomer.Phone;
                    PointBox.Text = EditingCustomer.Point.ToString();
                    RankNameBox.Text = EditingCustomer.Rank.Name;
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
            if (EditingCustomer != null)
            {
                EditingCustomer.Name = NameBox.Text;
                EditingCustomer.Phone = PhoneBox.Text;
                EditingCustomer.Point = int.TryParse(PointBox.Text, out int point) ? point : 0;

                ViewModel.Update(EditingCustomer);

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
