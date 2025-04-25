using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LowLand.Model.Customer;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class CustomerViewModel : INotifyPropertyChanged
    {
        private readonly IDao _dao;
        private readonly PagingViewModel<Customer> _paging;

        public PagingViewModel<Customer> Paging => _paging;
        public FullObservableCollection<CustomerRank> CustomerRanks { get; set; }
        public Customer EditingCustomer { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CustomerViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _paging = new PagingViewModel<Customer>(
                async (page, size, keyword) => await Task.Run(() => _dao.Customers.GetAll(page, size, keyword)), // Updated to use Customers repository
                pageSize: 10
            );

            CustomerRanks = new FullObservableCollection<CustomerRank>(
                _dao.CustomerRanks.GetAll()
            );
            EditingCustomer = null;
        }
        // ham check xem sdt co bi trung khong
        public bool IsPhoneNumberExists(string phoneNumber)
        {
            return _dao.Customers.GetAll().Any(c => c.Phone == phoneNumber);
        }
        public void Add(Customer item)
        {
            int result = _dao.Customers.Insert(item);
            if (result == 1)
            {
                _paging.RefreshAsync().Wait();
                Debug.WriteLine($"Added customer ID: {item.Id}, refreshed paging");
            }
            else
            {
                Debug.WriteLine("Insert failed: No rows affected.");
            }
        }

        public void Remove(Customer item)
        {
            int result = _dao.Customers.DeleteById(item.Id.ToString());
            if (result > 0)
            {
                _paging.RefreshAsync().Wait();
                Debug.WriteLine($"Removed customer ID: {item.Id}, refreshed paging");
            }
            else
            {
                Debug.WriteLine("Delete failed: No rows affected.");
            }
        }

        public void Update(Customer item)
        {
            int newRankId = CustomerRanks
                .Where(r => r.PromotionPoint <= item.Point)
                .OrderByDescending(r => r.PromotionPoint)
                .Select(r => r.Id)
                .FirstOrDefault();
            // Update rank
            if (newRankId != 0)
            {
                item.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
            }

            int result = _dao.Customers.UpdateById(item.Id.ToString(), item);
            if (result > 0)
            {
                // Update related orders
                foreach (var order in _dao.Orders.GetAll())
                {
                    if (order.CustomerId == item.Id)
                    {
                        order.CustomerName = item.Name;
                        order.CustomerPhone = item.Phone;
                        _dao.Orders.UpdateById(order.Id.ToString(), order);
                    }
                }
                _paging.RefreshAsync().Wait(); // Updated to use RefreshAsync
                Debug.WriteLine($"Updated customer ID: {item.Id}, refreshed paging");
            }
            else
            {
                Debug.WriteLine("Update failed: No rows affected.");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}