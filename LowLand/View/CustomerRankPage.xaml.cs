using System;
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
    public sealed partial class CustomerRankPage : Page
    {
        private CustomerRankViewModel ViewModel { get; set; } = new CustomerRankViewModel();
        public CustomerRankPage()
        {
            this.InitializeComponent();
        }

        private async void addButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CustomerRankDialog(ViewModel)
            {
                XamlRoot = this.XamlRoot
            };
            var result = await dialog.ShowAsync();
            dialog.Hide();
        }

        private async void updateButton_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin khách hàng được chọn
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Model.Customer.CustomerRank selectedCustomerRank)
            {

                ViewModel.EditorAddCustomerRank = selectedCustomerRank;
                ViewModel.updateMode = true;

                var dialog = new CustomerRankDialog(ViewModel)
                {
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                dialog.Hide();
            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is Model.Customer.CustomerRank selectedCustomerRank)

                {
                    // neu id =1 thi khong cho xoa
                    if (selectedCustomerRank.Id == 1)
                    {
                        ContentDialog deleteDialog = new ContentDialog
                        {
                            Title = "Không thể xóa",
                            Content = $"Không thể xóa hạng mặc định {selectedCustomerRank.Name}!",
                            CloseButtonText = "Đóng",
                            XamlRoot = this.XamlRoot
                        };
                        ContentDialogResult result = await deleteDialog.ShowAsync();
                        deleteDialog.Hide();
                    }
                    else
                    {
                        ContentDialog deleteDialog = new ContentDialog
                        {
                            Title = "Xác nhận xóa",
                            Content = $"Bạn có chắc muốn xóa hạng {selectedCustomerRank.Name}?",
                            PrimaryButtonText = "Xóa",
                            CloseButtonText = "Hủy",
                            XamlRoot = this.XamlRoot
                        };
                        ContentDialogResult result = await deleteDialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            await ViewModel.Remove(selectedCustomerRank);
                        }

                        // Close the dialog
                        deleteDialog.Hide();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


    }
}
