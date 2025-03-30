using System;
using System.Collections.ObjectModel;
using LowLand.Model.Order;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class OrderDetailViewModel
    {
        private IDao _dao;
        public ObservableCollection<Order> Orders { get; set; }
        public Order EditorAddOrder { get; set; }

        public OrderDetailViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Orders = new ObservableCollection<Order>(_dao.Orders.GetAll());



            EditorAddOrder = new Order
            {

                Details = new ObservableCollection<OrderDetail>()

            };
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



    }
}

