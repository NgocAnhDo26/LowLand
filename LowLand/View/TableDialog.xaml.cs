using System.Diagnostics;
using LowLand.Model.Table;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LowLand.View
{
    public sealed partial class TableDialog : ContentDialog
    {
        public Table? Result { get; private set; }

        public TableDialog(Table? table = null)
        {
            this.InitializeComponent();

            if (table != null)
            {
                // Edit mode
                TableNameTextBox.Text = table.Name;
                CapacityTextBox.Text = table.Capacity.ToString();
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == table.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                // Add mode – chọn mặc định
                StatusComboBox.SelectedIndex = 0;
            }
        }

        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(TableNameTextBox.Text))
            {
                errorMessage = "Tên bàn không được để trống.";
                return false;
            }

            if (!int.TryParse(CapacityTextBox.Text, out int capacity) || capacity <= 0)
            {
                errorMessage = "Sức chứa phải là một số nguyên dương.";
                return false;
            }

            if (StatusComboBox.SelectedItem is not ComboBoxItem selectedStatus || string.IsNullOrWhiteSpace(selectedStatus.Content?.ToString()))
            {
                errorMessage = "Vui lòng chọn trạng thái hợp lệ.";
                return false;
            }

            return true;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Debug.WriteLine($"--PrimaryButtonClick: {sender.Title}, {sender.Content}");

            if (!ValidateInput(out string errorMessage))
            {
                ShowWarning(errorMessage);
                args.Cancel = true;
                return;
            }

            HideWarning(); // Ẩn warning nếu mọi thứ ok

            var selectedItem = StatusComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null || selectedItem.Content == null)
            {
                ShowWarning("Trạng thái không hợp lệ");
                args.Cancel = true;
                return;
            }

            Result = new Table
            {
                Name = TableNameTextBox.Text.Trim(),
                Capacity = int.Parse(CapacityTextBox.Text),
                Status = selectedItem.Content.ToString() ?? string.Empty
            };

            Debug.WriteLine($"--Adding table: {Result.Name}, Capacity: {Result.Capacity}, Status: {Result.Status}");
        }

        private void ShowWarning(string message)
        {
            WarningBar.Message = message;
            WarningBar.Visibility = Visibility.Visible;
        }

        private void HideWarning()
        {
            WarningBar.Visibility = Visibility.Collapsed;
        }
    }
}
