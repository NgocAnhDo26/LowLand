using System;
using LowLand.Model.Product;
using LowLand.Utils;
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
    public sealed partial class CategoriesPage : Page
    {
        public CategoriesViewModel ViewModel { get; set; }

        public CategoriesPage()
        {
            ViewModel = new CategoriesViewModel();
            DataContext = ViewModel;
            this.InitializeComponent();

            if (ViewModel.Categories.Count > 0)
            {
                CategoriesListView.SelectedIndex = 0;
            }
        }

        private async void EditCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected category from the DataContext of the sender
            var menuItem = sender as MenuFlyoutItem;
            var selectedCategory = menuItem?.DataContext as Category;

            if (selectedCategory == null)
                return;

            // Create the TextBox for editing
            var panel = new StackPanel();
            var idTextBox = new TextBox
            {
                Header = "ID",
                Text = selectedCategory.Id.ToString(),
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var textBox = new TextBox
            {
                Header = "Tên danh mục",
                Text = selectedCategory.Name,
                PlaceholderText = "Nhập tên danh mục mới",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 16, 0, 0)
            };

            panel.Children.Add(idTextBox);
            panel.Children.Add(textBox);

            // Create the content dialog
            var dialog = new ContentDialog
            {
                Title = "Sửa danh mục",
                Content = panel,
                PrimaryButtonText = "Lưu",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // Show the dialog and handle the result
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string newName = textBox.Text.Trim();
                var responseCode = ViewModel.EditCategory(selectedCategory.Id, newName);

                // Show appropriate messages based on response code
                string message = responseCode switch
                {
                    ResponseCode.Success => "Cập nhật danh mục thành công!",
                    ResponseCode.EmptyName => "Tên danh mục không được để trống!",
                    ResponseCode.NameExists => "Tên danh mục đã tồn tại!",
                    ResponseCode.Error => "Lỗi khi cập nhật danh mục!",
                    _ => "Lỗi không xác định!"
                };

                var infoDialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await infoDialog.ShowAsync();
            }
        }

        private async void DeleteCategoryButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Get the selected category from the DataContext of the sender
            var menuItem = sender as MenuFlyoutItem;
            var selectedCategory = menuItem?.DataContext as Category;

            // Create the content dialog
            var dialog = new ContentDialog
            {
                Title = "Xác nhận xóa danh mục",
                Content = $"Bạn có chắc chắn muốn xóa danh mục '{selectedCategory!.Name}' không? Hành động này là không thể hoàn tác!",
                PrimaryButtonText = "Xóa",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // Show the dialog and handle the result
            dialog.PrimaryButtonClick += async (s, e) =>
            {
                var responseCode = ViewModel.DeleteCategory(selectedCategory.Id);
                dialog.Hide();

                // Show appropriate messages based on response code
                string message = responseCode switch
                {
                    ResponseCode.ItemHaveDependency => "Không thể xóa! Danh mục này đang được sử dụng bởi một hoặc nhiều sản phẩm!",
                    ResponseCode.Success => "Xóa danh mục thành công!",
                    ResponseCode.Error => "Lỗi khi xóa danh mục!",
                    _ => "Lỗi không xác định!"
                };

                var infoDialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await infoDialog.ShowAsync();
            };

            await dialog.ShowAsync();
        }

        private async void AddNewCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Create the TextBox for entering new category
            var panel = new StackPanel();

            var textBox = new TextBox
            {
                Header = "Tên danh mục",
                PlaceholderText = "Nhập tên danh mục mới",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 8, 0, 0)
            };

            panel.Children.Add(textBox);

            // Create the content dialog
            var dialog = new ContentDialog
            {
                Title = "Thêm danh mục mới",
                Content = panel,
                PrimaryButtonText = "Thêm",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // Show the dialog and handle the result
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string newName = textBox.Text.Trim();
                var responseCode = ViewModel.AddCategory(newName);

                // Show appropriate error message based on validation failures
                string message = responseCode switch
                {
                    ResponseCode.Success => "Thêm danh mục thành công!",
                    ResponseCode.EmptyName => "Tên danh mục không được để trống!",
                    ResponseCode.NameExists => "Tên danh mục đã tồn tại!",
                    ResponseCode.Error => "Lỗi khi thêm danh mục!",
                    _ => "Lỗi không xác định!"
                };

                var infoDialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                await infoDialog.ShowAsync();
            }
        }

    }
}
