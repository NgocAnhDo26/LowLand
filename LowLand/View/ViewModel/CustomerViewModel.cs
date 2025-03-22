using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowLand.Model.Customer;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class CustomerViewModel
    {
        private IDao _dao;
        public FullObservableCollection<Customer> Customers { get; set; }
        public CustomerViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Customers = new FullObservableCollection<Customer>(
                _dao.Customers.GetAll()
            );
        }

        public void Add(Customer item)
        {
     
            int newId = _dao.Customers.Insert(item);
            if (newId > 0)
            {
                item.Id = newId; 
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
            var customerToUpdate = Customers.FirstOrDefault(c => c.Id == item.Id);
            if (customerToUpdate != null)
            {
 
                int result = _dao.Customers.UpdateById(item.Id.ToString(), item);
                if (result > 0)
                {
 
                    customerToUpdate.Name = item.Name;
                    customerToUpdate.Phone = item.Phone;
                    customerToUpdate.Point = item.Point;
                    customerToUpdate.RankName = item.RankName;
                    customerToUpdate.RegistrationDate = item.RegistrationDate;
                    // nếu point cao hơn thì cập nhật rank
                    
                }
                else
                {
                    Console.WriteLine("Update failed: No rows affected.");
                }
            }
        }
    }
}
