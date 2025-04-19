using System;
using System.Diagnostics;
using LowLand.Model.Table;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class TablePage : Page
    {
        public TableViewModel ViewModel { get; set; } = new TableViewModel();

        public TablePage()
        {
            this.InitializeComponent();
        }

        private async void AddTable_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TableDialog
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.Result != null)
            {
                Debug.WriteLine($"Adding table: {dialog.Result.Name}, Capacity: {dialog.Result.Capacity}, Status: {dialog.Result.Status}");
                ViewModel.AddTable(dialog.Result);
            }
        }

        private async void EditTable_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Table tableToEdit)
            {
                var dialog = new TableDialog(tableToEdit)
                {
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && dialog.Result != null)
                {
                    dialog.Result.Id = tableToEdit.Id;
                    ViewModel.UpdateTable(dialog.Result);
                }
            }
        }

        private async void DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Table table)
            {
                // Ràng buộc trạng thái
                if (table.Status == "Có khách")
                {
                    ContentDialog warningDialog = new ContentDialog
                    {
                        Title = "Không thể xoá bàn",
                        Content = $"Bàn hiện đang được sử dụng và không thể xoá ",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await warningDialog.ShowAsync();
                    return;
                }

                // Hộp thoại xác nhận xoá
                ContentDialog confirmDialog = new ContentDialog
                {
                    Title = "Xác nhận xoá",
                    Content = $"Bạn có chắc muốn xoá bàn này ? ",
                    PrimaryButtonText = "Xoá",
                    CloseButtonText = "Huỷ",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.DeleteTable(table.Id);
                }
            }
        }


        private void TableGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Table table)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = $"Thông tin bàn {table.Name}",
                    Content = $"Trạng thái: {table.Status}\nSức chứa: {table.Capacity} người",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }
    }
}
