using System;
using System.Collections.ObjectModel;
using System.Linq;
using LowLand.Model.Order;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class OrderDetailViewModel
    {
        private IDao _dao;
        public ObservableCollection<OrderDetail> OrderDetails { get; set; }
        public OrderDetail EditorAddOrderDetail { get; set; }

        public OrderDetailViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            OrderDetails = new ObservableCollection<OrderDetail>(_dao.OrderDetails.GetAll());
            EditorAddOrderDetail = new OrderDetail { };
        }

        public void Add(OrderDetail item)
        {
            int result = _dao.OrderDetails.Insert(item);
            if (result == 1)
            {
                item.Id = _dao.OrderDetails.GetAll().Max(od => od.Id);
                OrderDetails.Add(item);
            }
            else
            {
                Console.WriteLine("Insert failed: No rows affected.");
            }
        }

        public void Remove(OrderDetail item)
        {
            int result = _dao.OrderDetails.DeleteById(item.Id.ToString());
            if (result == 1)
            {
                OrderDetails.Remove(item);
            }
            else
            {
                Console.WriteLine("Delete failed: No rows affected.");
            }
        }

        public void Update(OrderDetail item)
        {
            var orderDetailToUpdate = OrderDetails.FirstOrDefault(od => od.Id == item.Id);
            if (orderDetailToUpdate != null)
            {
                int result = _dao.OrderDetails.UpdateById(item.Id.ToString(), item);
                if (result == 1)
                {
                    int index = OrderDetails.IndexOf(orderDetailToUpdate);
                    if (index != -1)
                    {
                        OrderDetails[index] = item;
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

