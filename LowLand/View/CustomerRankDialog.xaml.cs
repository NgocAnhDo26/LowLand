﻿using System;
using System.Linq;
using LowLand.Model.Customer;
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
    public sealed partial class CustomerRankDialog : ContentDialog
    {
        private CustomerRankViewModel ViewModel = new CustomerRankViewModel();

        public CustomerRankDialog(CustomerRankViewModel viewModel)
        {
            this.InitializeComponent();
            this.ViewModel = viewModel;
            if (ViewModel.updateMode == true)
            {
                ViewModel.EditorAddCustomerRank = viewModel.EditorAddCustomerRank;
                this.Loaded += CustomerRankDialog_Loaded;
                Title = "Chỉnh sửa hạng thành viên";
            }
            else
            {
                ViewModel.EditorAddCustomerRank = new CustomerRank
                {
                    Name = string.Empty,
                    PromotionPoint = 0,
                    DiscountPercentage = 0
                };
                Title = "Thêm hạng thành viên";

            }
        }
        private void CustomerRankDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.EditorAddCustomerRank != null && ViewModel.EditorAddCustomerRank.Id == 1)
            {
                nameTextBox.IsEnabled = true;
                discountPercentageTextBox.IsEnabled = true;
                promotionPointTextBox.IsEnabled = false;
            }
        }

        private async void PrimaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel.updateMode == false)
            {
                // check cac box
                if (string.IsNullOrWhiteSpace(ViewModel.EditorAddCustomerRank.Name))
                {
                    errorTextBlock.Content = "Vui lòng điền đầy đủ thông tin ";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (ViewModel.EditorAddCustomerRank.PromotionPoint < 0)
                {
                    errorTextBlock.Content = "Mốc điểm cần thiết phải lớn hơn 0.";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                if (ViewModel.EditorAddCustomerRank.DiscountPercentage < 0)
                {
                    errorTextBlock.Content = "Mức ưu đãi phải lớn hơn 0.";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (ViewModel.EditorAddCustomerRank.DiscountPercentage > 100)
                {
                    errorTextBlock.Content = "Mức ưu đãi không được lớn hơn 100%";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (ViewModel.CustomerRanks.ToList().Any(c => c.PromotionPoint == ViewModel.EditorAddCustomerRank.PromotionPoint))
                {
                    errorTextBlock.Content = "Mốc điểm này đã được thiết lập.";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                await ViewModel.Add(ViewModel.EditorAddCustomerRank);
                ViewModel.EditorAddCustomerRank = new CustomerRank
                {
                    Name = string.Empty,
                    PromotionPoint = 0,
                    DiscountPercentage = 0
                };

                string message = "Thêm hạng khách hàng thành công.";

                var infoDialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await infoDialog.ShowAsync();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ViewModel.EditorAddCustomerRank.Name))
                {
                    errorTextBlock.Content = "Vui lòng điền đầy đủ thông tin ";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                if (ViewModel.EditorAddCustomerRank.PromotionPoint < 0)
                {
                    errorTextBlock.Content = "Số điểm cần thiết phải lớn hơn hoặc bằng 0.";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                if (ViewModel.EditorAddCustomerRank.DiscountPercentage < 0)
                {
                    errorTextBlock.Content = "Tỉ lệ giảm giá phải lớn hơn hoặc bằng 0.";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                // check trung moc diem khac ngoai moc nay
                if (ViewModel.CustomerRanks.ToList().Any(c => c.PromotionPoint == ViewModel.EditorAddCustomerRank.PromotionPoint && c.Id != ViewModel.EditorAddCustomerRank.Id))
                {
                    errorTextBlock.Content = "Mốc điểm này đã được thiết lập.";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                if (ViewModel.EditorAddCustomerRank.DiscountPercentage > 100)
                {
                    errorTextBlock.Content = "Mức ưu đãi không được lớn hơn 100%";
                    errorTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }
                await ViewModel.Update(ViewModel.EditorAddCustomerRank);
                ViewModel.EditorAddCustomerRank = new CustomerRank
                {
                    Name = string.Empty,
                    PromotionPoint = 0,
                    DiscountPercentage = 0
                };

                string message = "Cập nhật hạng khách hàng thành công.";

                var infoDialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await infoDialog.ShowAsync();
            }
        }

        private void CloseButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.EditorAddCustomerRank = new CustomerRank
            {
                Name = string.Empty,
                PromotionPoint = 0,
                DiscountPercentage = 0
            };
        }
    }
}

