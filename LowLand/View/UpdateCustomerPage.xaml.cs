using System;
using System.Threading.Tasks;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LowLand.View
{
    public sealed partial class UpdateCustomerPage : Page
    {
        private readonly UpdateCustomerViewModel ViewModel;

        public UpdateCustomerPage()
        {
            ViewModel = new UpdateCustomerViewModel();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int customerId)
            {
                if (ViewModel.LoadCustomer(customerId))
                {
                    NameBox.Text = ViewModel.EditingCustomer.Name;
                    PhoneBox.Text = ViewModel.EditingCustomer.Phone;
                    PointBox.Text = ViewModel.EditingCustomer.Point.ToString();
                    RankNameBox.Text = ViewModel.EditingCustomer.Rank?.Name ?? string.Empty;
                }
                else
                {
                    _ = ShowMessage(ViewModel.ErrorMessage);
                }
            }
        }

        private static bool isDialogOpen = false;

        private async Task<ContentDialogResult?> ShowMessage(string message)
        {
            if (isDialogOpen)
            {
                // Nếu đang mở dialog khác thì không mở mới, hoặc Chủ nhân có thể return gì đó tuush 
                return null;
            }

            isDialogOpen = true;

            try
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
            finally
            {
                isDialogOpen = false;
            }
        }



        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool success = ViewModel.Update(NameBox.Text, PhoneBox.Text, PointBox.Text);
            var result = await ShowMessage(ViewModel.ErrorMessage);
            if (result == ContentDialogResult.Primary && success)
            {
                Frame.Navigate(typeof(CustomerPage));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CustomerPage));
        }
    }
}