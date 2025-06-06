﻿using LowLand.Model.Customer;
using LowLand.Model.Discount;
using LowLand.Model.Order;
using LowLand.Model.Product;
using LowLand.Model.Table;

namespace LowLand.Services
{
    public interface IDao
    {
        // Product repositories
        IRepository<Product> Products { get; set; }
        IRepository<ComboItem> ComboItems { get; set; }
        IRepository<ProductOption> ProductOptions { get; set; }
        IRepository<Category> Categories { get; set; }

        // Order repositories
        IRepository<Order> Orders { get; set; }
        IRepository<OrderDetail> OrderDetails { get; set; }

        // Customer repositories
        IRepository<Customer> Customers { get; set; }
        IRepository<CustomerRank> CustomerRanks { get; set; }

        // Promotion repositories
        IRepository<Promotion> Promotions { get; set; }
        // Table repositories
        IRepository<Table> Tables { get; set; }
    }
}
