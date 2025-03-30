using System;
using System.Collections.ObjectModel;
using System.Linq;
using LowLand.Model.Order;
using LowLand.Services;


namespace LowLand.View.ViewModel
{
    public class OrderViewModel
    {
        private IDao _dao;
        public ObservableCollection<Order> Orders { get; set; }

        public OrderViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Orders = new ObservableCollection<Order>(_dao.Orders.GetAll());
        }
        public void Add(Order item)
        {
            item.Date = DateTime.Now;
            item.Status = "Pending";
            int result = _dao.Orders.Insert(item);
            if (result == 1)
            {
                item.Id = _dao.Orders.GetAll().Max(o => o.Id);
                Orders.Add(item);
            }
            else
            {
                Console.WriteLine("Insert failed: No rows affected.");
            }
        }

        public void Remove(Order item)
        {
            int result = _dao.Orders.DeleteById(item.Id.ToString());
            if (result == 1)
            {
                Orders.Remove(item);
            }
            else
            {
                Console.WriteLine("Delete failed: No rows affected.");
            }
        }

        public void Update(Order item)
        {
            var orderToUpdate = Orders.FirstOrDefault(o => o.Id == item.Id);
            if (orderToUpdate != null)
            {
                int result = _dao.Orders.UpdateById(item.Id.ToString(), item);
                if (result == 1)
                {
                    int index = Orders.IndexOf(orderToUpdate);
                    if (index != -1)
                    {
                        Orders[index] = item;
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
