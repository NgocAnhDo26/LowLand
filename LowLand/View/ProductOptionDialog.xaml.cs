using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class ProductOptionDialog : ContentDialog
    {
        public ProductOptionViewModel ViewModel { get; set; }
        public ProductInfoViewModel ProductInfoViewModel { get; set; }

        public ProductOptionDialog(ProductInfoViewModel viewModel, int optionId)
        {
            //ProductInfoViewModel = viewModel;
            //ViewModel = new ProductOptionViewModel(ProductInfoViewModel.Product.Id, optionId);

            //this.DataContext = ViewModel;
            //this.InitializeComponent();
        }

        // Handle save button click
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //ContentDialog infoDialog = new ContentDialog
            //{
            //    CloseButtonText = "OK",
            //    XamlRoot = this.XamlRoot
            //};

            //if (ViewModel.Option.OptionId == -1)
            //{
            //    infoDialog.Title = "Thêm tùy chọn";

            //    if (ProductInfoViewModel.AddNewProductOption(ViewModel.Option))
            //    {
            //        this.Hide();
            //        infoDialog.Content = "Thêm tùy chọn thành công";
            //        await infoDialog.ShowAsync();
            //    }
            //    else
            //    {
            //        this.Hide();
            //        infoDialog.Content = "Thêm tùy chọn thất bại";
            //        await infoDialog.ShowAsync();
            //    }

            //    return;
            //}

            //infoDialog.Title = "Cập nhật tùy chọn";
            //if (ProductInfoViewModel.UpdateProductOption(ViewModel.Option))
            //{
            //    this.Hide();
            //    infoDialog.Content = "Cập nhật tùy chọn thành công";
            //    await infoDialog.ShowAsync();
            //}
            //else
            //{
            //    this.Hide(); // Close the dialog after saving
            //    infoDialog.Content = "Cập nhật tùy chọn thất bại";
            //    await infoDialog.ShowAsync();
            //}
        }
    }
}
