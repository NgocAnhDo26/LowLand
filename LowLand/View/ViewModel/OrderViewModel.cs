using System;
using System.ComponentModel;
using System.Linq;
using LowLand.Model.Order;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private readonly IDao _dao;
        private readonly PagingViewModel<Order> _paging;

        public PagingViewModel<Order> Paging => _paging;

        public event PropertyChangedEventHandler PropertyChanged;

        public OrderViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _paging = new PagingViewModel<Order>(
                (page, size, keyword) => _dao.Orders.GetAll(page, size, keyword),
                pageSize: 10
            );
        }

        public void Add(Order item)
        {
            item.Date = DateTime.Now;
            item.Status = "Đang xử lý";
            int result = _dao.Orders.Insert(item);
            if (result == 1)
            {
                item.Id = _dao.Orders.GetAll().Max(o => o.Id);
                _paging.Refresh();
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
                _paging.Refresh();
            }
            else
            {
                Console.WriteLine("Delete failed: No rows affected.");
            }
        }

        public void Update(Order item)
        {
            int result = _dao.Orders.UpdateById(item.Id.ToString(), item);
            if (result == 1)
            {
                _paging.Refresh();
            }
            else
            {
                Console.WriteLine("Update failed: No rows affected.");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}