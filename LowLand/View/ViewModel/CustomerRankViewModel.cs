using System;
using System.Diagnostics;
using System.Linq;
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
        public void Add(CustomerRank item)
        {
            int result = _dao.CustomerRanks.Insert(item);
            Debug.WriteLine("result", result);
            if (result == 1)
            {
                item.Id = _dao.CustomerRanks.GetAll().Max(r => r.Id);
                CustomerRanks.Add(item);

                foreach (var customer in _dao.Customers.GetAll())
                {
                    int newRankId = getNewRankId(customer.Point);
                    customer.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
                    _dao.Customers.UpdateById(customer.Id.ToString(), customer);
                }


            }
            else
            {
                Console.WriteLine("Insert failed: No rows affected.");
            }
        }

        public void Remove(CustomerRank item)
        {
            try
            {
                int result = _dao.CustomerRanks.DeleteById(item.Id.ToString());
                CustomerRanks.Remove(item);

                Debug.WriteLine("Result: " + result);

                if (result == 1)
                {
                    foreach (var customer in _dao.Customers.GetAll())
                    {
                        if (customer.Rank == null)
                        {
                            int newRankId = getNewRankId(customer.Point);
                            customer.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
                            _dao.Customers.UpdateById(customer.Id.ToString(), customer);
                        }
                    }
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

        }

        public void Update(CustomerRank item)
        {
            var customerToUpdate = CustomerRanks.FirstOrDefault(c => c.Id == item.Id);
            if (customerToUpdate != null)
            {
                int result = _dao.CustomerRanks.UpdateById(item.Id.ToString(), item);
                if (result == 1)
                {
                    var index = CustomerRanks.IndexOf(customerToUpdate);
                    if (index != -1)
                    {
                        CustomerRanks[index] = item;
                    }

                    foreach (var customer in _dao.Customers.GetAll())
                    {
                        Debug.WriteLine("Customer: " + customer.Id);
                        Debug.WriteLine("Customer Rank: " + customer.Rank.Name);

                        int newRankId = getNewRankId(customer.Point);
                        customer.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
                        _dao.Customers.UpdateById(customer.Id.ToString(), customer);
                        Debug.WriteLine("Customer: " + customer.Id);
                        Debug.WriteLine("Customer Rank: " + customer.Rank.Name);

                    }
                }
                else
                {
                    Console.WriteLine("Update failed: No rows affected.");
                }
            }
        }
    }
}
