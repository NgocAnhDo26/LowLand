using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Customer;
using LowLand.Model.Order;
using LowLand.Model.Product;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class UpdateOrderViewModel
    {
        private IDao _dao;
        public ObservableCollection<Order> Orders { get; set; }
        public Order EditorAddOrder { get; set; }
        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<ProductOption> ProductOptions { get; set; }

        public UpdateOrderViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Orders = new ObservableCollection<Order>(_dao.Orders.GetAll());

            EditorAddOrder = new Order
            {
                Details = new ObservableCollection<OrderDetail>()
            };

            ProductOptions = new ObservableCollection<ProductOption>(_dao.ProductOptions.GetAll());
            Customers = new ObservableCollection<Customer>(_dao.Customers.GetAll());
            Products = new ObservableCollection<Product>(_dao.Products.GetAll());

        }
        public void AddOptionToOrderDetail(Order order)
        {

            if (order.Details == null) return;
            foreach (var detail in order.Details)
            {
                Debug.WriteLine("detail.ProductId: " + detail.ProductId);
                if (detail.ProductId != null)
                {
                    detail.ProductOptions = new ObservableCollection<ProductOption>(
                        ProductOptions.Where(po => po.ProductId == detail.ProductId)
                    );
                }
            }
        }

        public OrderDetail CreateOrderDetail(Product selectedProduct)
        {
            if (selectedProduct == null) return null;

            var orderDetail = new OrderDetail
            {
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                ProductPrice = selectedProduct.SalePrice,
                Price = selectedProduct.SalePrice * 1,
                quantity = 1,
                ProductOptions = new ObservableCollection<ProductOption>(
                    ProductOptions.Where(po => po.ProductId == selectedProduct.Id)
                )
            };
            return orderDetail;
        }
        public void Add(Order item)
        {
            // gan date (datetime) = now
            item.Date = DateTime.Now;
            // gan status = "Pending"
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
        public OrderDetail CreatOrderDetail(Product selectedProduct)
        {
            if (selectedProduct == null) return null;

            var orderDetail = new OrderDetail
            {
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                ProductPrice = selectedProduct.SalePrice,
                Price = selectedProduct.SalePrice * 1,
                quantity = 1
            };
            return orderDetail;
        }
        public Customer GetCustomerByPhone(string phone)
        {
            var customer = Customers.FirstOrDefault(c => c.Phone == phone);
            if (customer != null)
            {
                return customer;
            }
            return null;
        }
        public void Update(Order item)
        {
            // if (item.PromotionId == null)
            //
            //{
            item.TotalAfterDiscount = item.TotalPrice;
            // }
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
