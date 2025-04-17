using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using LowLand.Model.Customer;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class UpdateCustomerViewModel : INotifyPropertyChanged
    {
        private readonly IDao _dao;
        private Customer _editingCustomer;
        private string _errorMessage;

        public Customer EditingCustomer
        {
            get => _editingCustomer;
            set
            {
                _editingCustomer = value;
                OnPropertyChanged(nameof(EditingCustomer));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public UpdateCustomerViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _editingCustomer = null;
            _errorMessage = string.Empty;
        }

        public bool LoadCustomer(int customerId)
        {
            try
            {
                EditingCustomer = _dao.Customers.GetById(customerId.ToString());
                if (EditingCustomer != null)
                {
                    Debug.WriteLine($"Loaded customer: ID={EditingCustomer.Id}, Name={EditingCustomer.Name}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Customer with ID {customerId} not found");
                    ErrorMessage = "Không tìm thấy khách hàng.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load customer failed: {ex.Message}");
                ErrorMessage = "Lỗi khi tải thông tin khách hàng.";
                return false;
            }
        }

        public bool Update(string name, string phone, string pointText)
        {
            if (EditingCustomer == null)
            {
                ErrorMessage = "Không có khách hàng để cập nhật.";
                return false;
            }

            // Kiểm tra đầu vào
            if (string.IsNullOrWhiteSpace(name))
            {
                ErrorMessage = "Tên không được để trống!";
                return false;
            }

            if (!Regex.IsMatch(phone, @"^0\d{9,10}$"))
            {
                ErrorMessage = "Số điện thoại không hợp lệ!";
                return false;
            }

            if (!int.TryParse(pointText, out int point) || point < 0)
            {
                ErrorMessage = "Điểm phải là số nguyên và lớn hơn hoặc bằng 0!";
                return false;
            }

            try
            {
                // Cập nhật thông tin
                EditingCustomer.Name = name;
                EditingCustomer.Phone = phone;
                EditingCustomer.Point = point;

                // Cập nhật hạng
                var customerRanks = _dao.CustomerRanks.GetAll();
                int newRankId = customerRanks
                    .Where(r => r.PromotionPoint <= point)
                    .OrderByDescending(r => r.PromotionPoint)
                    .Select(r => r.Id)
                    .FirstOrDefault();
                if (newRankId != 0)
                {
                    EditingCustomer.Rank = customerRanks.FirstOrDefault(r => r.Id == newRankId);
                }

                // Lưu vào cơ sở dữ liệu
                int result = _dao.Customers.UpdateById(EditingCustomer.Id.ToString(), EditingCustomer);
                if (result > 0)
                {
                    // Cập nhật đơn hàng liên quan
                    foreach (var order in _dao.Orders.GetAll())
                    {
                        if (order.CustomerId == EditingCustomer.Id)
                        {
                            order.CustomerName = EditingCustomer.Name;
                            order.CustomerPhone = EditingCustomer.Phone;
                            _dao.Orders.UpdateById(order.Id.ToString(), order);
                        }
                    }

                    Debug.WriteLine($"Updated customer: ID={EditingCustomer.Id}, Name={name}, Phone={phone}, Point={point}");
                    ErrorMessage = "Cập nhật thông tin khách hàng thành công!";
                    return true;
                }
                else
                {
                    Debug.WriteLine("Update failed: No rows affected.");
                    ErrorMessage = "Cập nhật thông tin khách hàng thất bại!";
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update customer failed: {ex.Message}");
                ErrorMessage = "Lỗi khi cập nhật khách hàng.";
                return false;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
