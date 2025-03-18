using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Npgsql;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LowLand.Services;
using LowLand.Model.Customer;
using System.Diagnostics;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow : Window
    {
        private PostgreDAO _dao;
        public LoginWindow()
        {
            this.InitializeComponent();
            _dao = new PostgreDAO();
            TestDatabaseConnection();
        }
        private void TestDatabaseConnection()
        {
            try
            {
                // Test lấy danh sách tất cả khách hàng
                var customers = _dao.Customers.GetAll();
                var customer___ = _dao.Customers.GetById("2");
                var nowday = DateTime.Now;


                if (customers.Count > 0)
                {
                    // Test lấy một khách hàng cụ thể (giả sử có ID = 1)
                    var customer = _dao.Customers.GetById("1");
                    if (customer != null)
                    {
                      
                        Debug.WriteLine("Khách hàng có ID = 1: " + customer.Name);
                    }
                    else
                    {
                        Debug.WriteLine("Không tìm thấy khách hàng có ID = 1.");
                    }
                    Debug.WriteLine("Kết nối thành công và có " + customers.Count + " khách hàng trong database.");
                    Debug.WriteLine("lay theo id"+ customer___.Name);
                   // _dao.Customers.Insert(new Customer {  Name = "Nguyễn Văn A", Phone = "0123456789",Point=0, RegistrationDate=nowday, RankId = 1 });
                  //  _dao.Customers.UpdateById("3", new Customer { Name = "Nguyễn Văn B", Phone = "9876543210", Point = 10, RegistrationDate = nowday, RankId = 1 });
                //    _dao.Customers.DeleteById("4");
                }
                else
                {
                    Debug.WriteLine("Kết nối thành công nhưng không có khách hàng nào trong database.");

                  
                }

                Debug.WriteLine("🟢 BẮT ĐẦU TEST CUSTOMER RANK");

                // 1. Lấy danh sách tất cả customer rank
                var ranks = _dao.CustomerRanks.GetAll();
                Debug.WriteLine("Có " + ranks.Count + " customer rank trong database.");

                if (ranks.Count > 0)
                {
                    // 2. Lấy một customer rank cụ thể
                    var rank = _dao.CustomerRanks.GetById("1");
                    if (rank != null)
                    {
                        Debug.WriteLine($"Customer Rank ID = 1: {rank.Name}, Điểm khuyến mãi: {rank.PromotionPoint}, Giảm giá: {rank.DiscountPercentage}%");
                    }
                    else
                    {
                        Debug.WriteLine("Không tìm thấy customer rank có ID = 1.");
                    }
                }

                // 3. Insert mới một Customer Rank
                // var newRank = new CustomerRank { Name = "VIP", PromotionPoint = 500, DiscountPercentage = 10 };
                // _dao.CustomerRanks.Insert(newRank);
                //   Debug.WriteLine("Đã thêm Customer Rank mới: " + newRank.Name);

                // 4. Update Customer Rank ID = 2
                //_dao.CustomerRanks.UpdateById("4", new CustomerRank { Name = "Super VIP", PromotionPoint = 1000, DiscountPercentage = 15 });
                //    Debug.WriteLine("Đã cập nhật Customer Rank ID = 4.");

               // 5.Xóa Customer Rank ID = 4
                _dao.CustomerRanks.DeleteById("4");
                Debug.WriteLine("Đã xóa Customer Rank ID =4.");

                Debug.WriteLine("🟢 KẾT THÚC TEST CUSTOMER RANK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Kết nối thất bại: " + ex.Message);
            }
            ;
        }
    }
}
