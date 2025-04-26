using LowLand.Model.Discount;
using LowLand.Utils;
using LowLand.View.Converter;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPromotionPage : Page
    {
        private AddPromotionViewModel ViewModel = new AddPromotionViewModel();
        public AddPromotionPage()
        {
            this.InitializeComponent();

            // Set NumberFormatter directly
            MinimumOrderValueTextBox.NumberFormatter = new NumberFormatter();
            DataContext = ViewModel;
        }

        private void PromotionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PromotionTypeComboBox.SelectedItem == null)
            {
                return;
            }

            var selectedType = (PromotionType)PromotionTypeComboBox.SelectedIndex;
            ViewModel.ChangePromotionType(selectedType);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Promotion promotion)
            {
                AddPromotionTitle.Text = promotion.Id == -1 ? "Thêm khuyến mãi mới" : "Cập nhật khuyến mãi";
                PromotionTypeComboBox.SelectedIndex = (int)promotion.Type;
                ViewModel.LoadPromotion(promotion);
            }
        }

        private void ApplyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var response = ViewModel.SavePromotion();

            // Show appropriate messages based on response code
            string message = response switch
            {
                ResponseCode.Success => "Lưu khuyến mãi thành công!",
                ResponseCode.EmptyName => "Tên khuyến mãi không được để trống!",
                ResponseCode.NameExists => "Tên khuyến mãi đã tồn tại!",
                ResponseCode.InvalidValue => "Phần trăm khuyến mãi không được lớn hơn 100%!",
                ResponseCode.EmptyDate => "Ngày bắt đầu và ngày kết thúc không được để trống!",
                ResponseCode.InvalidDate => "Ngày bắt đầu không được lớn hơn ngày kết thúc!",
                ResponseCode.InvalidStatus => "Không thể kích hoạt do thời gian hiện tại nằm ngoài khoảng thời gian khuyến mãi!",
                ResponseCode.Error => "Đã xảy ra lỗi khi lưu khuyến mãi!",

            };

            var dialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            dialog.ShowAsync().Completed = (asyncOperation, asyncStatus) =>
            {
                if (response == ResponseCode.Success)
                {
                    Frame.Navigate(typeof(PromotionsPage));
                }
            };
        }

        private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
