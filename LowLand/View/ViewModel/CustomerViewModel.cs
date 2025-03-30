using System;
using System.Linq;
using LowLand.Model.Customer;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class CustomerViewModel
    {
        private IDao _dao;
        public FullObservableCollection<Customer> Customers { get; set; }
        public FullObservableCollection<CustomerRank> CustomerRanks { get; set; }
        public CustomerViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Customers = new FullObservableCollection<Customer>(
                _dao.Customers.GetAll()
            );
            CustomerRanks = new FullObservableCollection<CustomerRank>(
                _dao.CustomerRanks.GetAll()
            );
        }

        public void Add(Customer item)
        {
            int result = _dao.Customers.Insert(item);
            if (result == 1)
            {

                Customers.Add(item);
            }
            else
            {
                Console.WriteLine("Insert failed: No rows affected.");
            }
        }

        public void Remove(Customer item)
        {
            int result = _dao.Customers.DeleteById(item.Id.ToString());
            if (result > 0)
            {
                Customers.Remove(item);

            }
            else
            {
                Console.WriteLine("Delete failed: No rows affected.");
            }
        }

        public void Update(Customer item)
        {

            int newRankId = CustomerRanks
               .Where(r => r.PromotionPoint <= item.Point)
               .OrderByDescending(r => r.PromotionPoint)
               .Select(r => r.Id)
               .FirstOrDefault();
            // update rank
            if (newRankId != 0)
            {
                item.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
            }
            var customerToUpdate = Customers.FirstOrDefault(c => c.Id == item.Id);
            if (customerToUpdate != null)
            {
                int result = _dao.Customers.UpdateById(item.Id.ToString(), item);
                if (result <= 0)
                {
                    Console.WriteLine("Update failed: No rows affected.");
                }

                foreach (var order in _dao.Orders.GetAll())
                {
                    if (order.CustomerId == item.Id)
                    {
                        order.CustomerName = item.Name;
                        order.CustomerPhone = item.Phone;
                        _dao.Orders.UpdateById(order.Id.ToString(), order);
                    }
                }
            }
        }

    }
}
