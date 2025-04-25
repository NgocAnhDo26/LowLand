using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LowLand.Model.Customer;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class CustomerRankViewModel
    {
        private IDao _dao;
        public FullObservableCollection<CustomerRank> CustomerRanks { get; set; }
        public CustomerRank EditorAddCustomerRank { get; set; }
        public bool updateMode;

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public CustomerRankViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            CustomerRanks = new FullObservableCollection<CustomerRank>(
                _dao.CustomerRanks.GetAll()
            );
            EditorAddCustomerRank = new CustomerRank
            {
                Name = string.Empty,
                PromotionPoint = 0,
                DiscountPercentage = 0
            };
            updateMode = false;
        }

        public int getNewRankId(int point)
        {
            return CustomerRanks
                .Where(r => r.PromotionPoint <= point)
                .OrderByDescending(r => r.PromotionPoint)
                .Select(r => r.Id)
                .FirstOrDefault();
        }

        public async Task Add(CustomerRank item)
        {
            IsProcessing = true;

            int result = await Task.Run(() => _dao.CustomerRanks.Insert(item));
            if (result == 1)
            {
                item.Id = _dao.CustomerRanks.GetAll().Max(r => r.Id);
                CustomerRanks.Add(item);

                await UpdateAffectedCustomersAsync();
            }
            else
            {
                Debug.WriteLine("Insert failed: No rows affected.");
            }

            IsProcessing = false;
        }

        public async Task Remove(CustomerRank item)
        {
            IsProcessing = true;

            try
            {
                int result = await Task.Run(() => _dao.CustomerRanks.DeleteById(item.Id.ToString()));
                if (result == 1)
                {
                    CustomerRanks.Remove(item);
                    await UpdateAffectedCustomersAsync();
                }
                else
                {
                    Debug.WriteLine("Delete failed: No rows affected.");
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                Debug.WriteLine($"Postgres Error: {ex.Message}");
            }

            IsProcessing = false;
        }

        public async Task Update(CustomerRank item)
        {
            IsProcessing = true;

            var customerToUpdate = CustomerRanks.FirstOrDefault(c => c.Id == item.Id);
            if (customerToUpdate != null)
            {
                int result = await Task.Run(() => _dao.CustomerRanks.UpdateById(item.Id.ToString(), item));
                if (result == 1)
                {
                    var index = CustomerRanks.IndexOf(customerToUpdate);
                    if (index != -1)
                    {
                        CustomerRanks[index] = item;
                    }

                    await UpdateAffectedCustomersAsync();
                }
                else
                {
                    Debug.WriteLine("Update failed: No rows affected.");
                }
            }

            IsProcessing = false;
        }

        /// <summary>
        /// Chỉ update những khách hàng thực sự bị thay đổi Rank
        /// </summary>
        private async Task UpdateAffectedCustomersAsync()
        {
            await Task.Run(() =>
            {
                var customers = _dao.Customers.GetAll();

                foreach (var customer in customers)
                {
                    int newRankId = getNewRankId(customer.Point);
                    if (customer.Rank == null || customer.Rank.Id != newRankId)
                    {
                        customer.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
                        _dao.Customers.UpdateById(customer.Id.ToString(), customer);
                    }
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
