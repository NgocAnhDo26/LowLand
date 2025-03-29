using System;
using System.Diagnostics;
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
            ProductInfoViewModel = viewModel;
            ViewModel = new ProductOptionViewModel(ProductInfoViewModel.Product.Id, optionId);

            this.DataContext = ViewModel;
            this.InitializeComponent();
        }

        // Handle save button click
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ContentDialog infoDialog = new ContentDialog
            {
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            if (ViewModel.Option.OptionId == -1)
            {
                if (!ProductInfoViewModel.AddNewProductOption(ViewModel.Option))
                {
                    this.Hide();
                    infoDialog.Title = "Thêm tùy chọn";
                    infoDialog.Content = "Thêm tùy chọn thất bại";
                    await infoDialog.ShowAsync();
                }

                return;
            }

            if (!ProductInfoViewModel.UpdateProductOption(ViewModel.Option))
            {
                this.Hide(); // Close the dialog after saving
                infoDialog.Title = "Cập nhật tùy chọn";
                infoDialog.Content = "Cập nhật tùy chọn thất bại";
                await infoDialog.ShowAsync();
            }
        }


        // Validate cost price
        private void CostPriceBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            int value = (int)sender.Value;

            if (value < 0)
            {
                WarningBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                WarningBar.Message = "Giá vốn không thể nhỏ hơn 0!";
                IsPrimaryButtonEnabled = false;
            }
            else if (value > (int)SalePriceBox.Value)
            {
                WarningBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                WarningBar.Message = "Giá bán không được thấp hơn giá vốn!";
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                WarningBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;
            }
        }

        // Validate sale price
        private void SalePriceBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            int value = (int)sender.Value;
            Debug.WriteLine(value);

            if (value < 0)
            {
                WarningBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                WarningBar.Message = "Giá bán không thể nhỏ hơn 0!";
                IsPrimaryButtonEnabled = false;
            }
            else if (value < (int)CostPriceBox.Value)
            {
                WarningBar.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                WarningBar.Message = "Giá bán không được thấp hơn giá vốn!";
                IsPrimaryButtonEnabled = false;
            }
            else
            {
                WarningBar.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                IsPrimaryButtonEnabled = true;
            }
        }
    }
}
