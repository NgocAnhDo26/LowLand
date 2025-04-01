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
    public class AddOrderViewModel
    {
        private IDao _dao;
        public ObservableCollection<Order> Orders { get; set; }
        public Order EditorAddOrder { get; set; }
        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<ProductOption> ProductOptions { get; set; }
        public ObservableCollection<CustomerRank> CustomerRanks { get; set; } // Added this property

        public AddOrderViewModel()
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
            CustomerRanks = new ObservableCollection<CustomerRank>(_dao.CustomerRanks.GetAll()); // Initialize it here
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
            item.Date = DateTime.Now;
            item.Status = "Đang xử lý";
            Debug.WriteLine("item.Total: " + item.TotalPrice, item.TotalAfterDiscount);
            var customer = Customers.FirstOrDefault(c => c.Id == item.CustomerId);
            if (customer != null)
            {
                customer.Point += (int)Math.Round((double)item.TotalPrice / 1000);

                int newRankId = CustomerRanks
                    .Where(r => r.PromotionPoint <= customer.Point)
                    .OrderByDescending(r => r.PromotionPoint)
                    .Select(r => r.Id)
                    .FirstOrDefault();

                if (newRankId != 0)
                {
                    customer.Rank = CustomerRanks.FirstOrDefault(r => r.Id == newRankId);
                }
                _dao.Customers.UpdateById(customer.Id.ToString(), customer);


            }

            item.TotalAfterDiscount = item.TotalPrice;

            int result = _dao.Orders.Insert(item);
            if (result == 1)
            {
                item.Id = _dao.Orders.GetAll().Max(o => o.Id);
                Orders.Add(item);
            }
            else
            {
                Debug.WriteLine("Insert failed: No rows affected.");
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
    }
}
