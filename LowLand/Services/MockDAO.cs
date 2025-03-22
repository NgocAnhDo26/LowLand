using LowLand.Model.Customer;
using LowLand.Model.Order;
using LowLand.Model.Product;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LowLand.Services
{
    public class MockDAO : IDao
    {
        public IRepository<Product> Products { get; set; } = new ProductRepository();
        public IRepository<ProductType> ProductTypes { get; set; } = new ProductTypeRepository();
        public IRepository<Category> Categories { get; set; } = new CategoryRepository();
        public IRepository<Order> Orders { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IRepository<OrderDetail> OrderDetails { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IRepository<Customer> Customers { get; set; } = new CustomerRepository();
        public IRepository<CustomerRank> CustomerRanks { get; set; } = new CustomerRankRepository();

        private class ProductRepository : IRepository<Product>
        {
            private List<Product> _products = new List<Product>()
            {
                new Product() { Id = 1, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "Phin Sữa Đá", SalePrice = 29000, CostPrice = 15000, Image = "phin_sua_da.jpg", Size = "M" },
                new Product() { Id = 2, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "Phin Đen Đá", SalePrice = 29000, CostPrice = 15000, Image = "phin_den_da.jpg", Size = "M" },
                new Product() { Id = 3, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "PhinDi Kem Sữa", SalePrice = 35000, CostPrice = 18000, Image = "phindi_kem_sua.jpg", Size = "M" },
                new Product() { Id = 4, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "PhinDi Choco", SalePrice = 35000, CostPrice = 18000, Image = "phindi_choco.jpg", Size = "M" },
                new Product() { Id = 5, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "Espresso", SalePrice = 35000, CostPrice = 18000, Image = "espresso.jpg", Size = "S" },
                new Product() { Id = 6, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "Americano", SalePrice = 35000, CostPrice = 18000, Image = "americano.jpg", Size = "M" },
                new Product() { Id = 7, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "Latte", SalePrice = 40000, CostPrice = 20000, Image = "latte.jpg", Size = "M" },
                new Product() { Id = 8, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName="Thức uống", Name = "Cappuccino", SalePrice = 40000, CostPrice = 20000, Image = "cappuccino.jpg", Size = "M" },
                new Product() { Id = 9, ProductTypeId = 1, ProductTypeName="Cà phê", CategoryName = "Thức uống", Name = "Caramel Macchiato", SalePrice = 45000, CostPrice = 22000, Image = "caramel_macchiato.jpg", Size = "M" },
                new Product() { Id = 10, ProductTypeId = 2, ProductTypeName="Trà", CategoryName="Thức uống",Name = "Trà Sen Vàng", SalePrice = 39000, CostPrice = 20000, Image = "tra_sen_vang.jpg", Size = "M" },
                new Product() { Id = 11, ProductTypeId = 2, ProductTypeName="Trà", CategoryName = "Thức uống", Name = "Trà Thạch Đào", SalePrice = 39000, CostPrice = 20000, Image = "tra_thach_dao.jpg", Size = "M" },
                new Product() { Id = 12, ProductTypeId = 2, ProductTypeName="Trà", CategoryName = "Thức uống", Name = "Trà Thạch Vải", SalePrice = 39000, CostPrice = 20000, Image = "tra_thach_vai.jpg", Size = "M" },
                new Product() { Id = 13, ProductTypeId = 2, ProductTypeName="Trà", CategoryName = "Thức uống", Name = "Trà Thanh Đào", SalePrice = 39000, CostPrice = 20000, Image = "tra_thanh_dao.jpg", Size = "M" },
                new Product() { Id = 14, ProductTypeId = 3, ProductTypeName="Freeze", CategoryName = "Thức uống", Name = "Freeze Trà Xanh", SalePrice = 49000, CostPrice = 25000, Image = "freeze_tra_xanh.jpg", Size = "M" },
                new Product() { Id = 15, ProductTypeId = 3, ProductTypeName="Freeze", CategoryName = "Thức uống", Name = "Freeze Cookies & Cream", SalePrice = 49000, CostPrice = 25000, Image = "freeze_cookies_cream.jpg", Size = "M" },
                new Product() { Id = 16, ProductTypeId = 3, ProductTypeName="Freeze", CategoryName = "Thức uống", Name = "Freeze Chocolate", SalePrice = 49000, CostPrice = 25000, Image = "freeze_chocolate.jpg", Size = "M" },
                new Product() { Id = 17, ProductTypeId = 4, ProductTypeName="Bánh", CategoryName = "Đồ ăn", Name = "Bánh Croissant", SalePrice = 29000, CostPrice = 15000, Image = "banh_croissant.jpg", Size = "X" },
                new Product() { Id = 18, ProductTypeId = 4, ProductTypeName="Bánh", CategoryName="Đồ ăn", Name = "Bánh Mousse Cacao", SalePrice = 35000, CostPrice = 18000, Image = "banh_mousse_cacao.jpg", Size = "X" },
                new Product() { Id = 19, ProductTypeId = 4, ProductTypeName="Bánh", CategoryName = "Đồ ăn", Name = "Bánh Mousse Đào", SalePrice = 35000, CostPrice = 18000, Image = "banh_mousse_dao.jpg", Size = "X" },
                new Product() { Id = 20, ProductTypeId = 4, ProductTypeName="Bánh", CategoryName = "Đồ ăn", Name = "Bánh Phô Mai Chanh Dây", SalePrice = 29000, CostPrice = 15000, Image = "banh_pho_mai_chanh_day.jpg", Size = "X" },
            };
            public List<Product> GetAll()
            {
                return _products;
            }

            public int DeleteById(string id)
            {
                int result = _products.RemoveAll(p => p.Id == int.Parse(id));
                return result;
            }


            public Product GetById(string id)
            {
                var product = _products.FirstOrDefault(p => p.Id == int.Parse(id));
                return product;
            }

            public int Insert(Product info)
            {
                _products.Add(info);
                return 1;
            }

            public int UpdateById(string id, Product info)
            {
                var product = _products.FirstOrDefault(p => p.Id == int.Parse(id));
                if (product != null)
                {
                    product.ProductTypeId = info.ProductTypeId;
                    product.Name = info.Name;
                    product.SalePrice = info.SalePrice;
                    product.CostPrice = info.CostPrice;
                    product.Image = info.Image;
                    product.Size = info.Size;
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private class ProductTypeRepository : IRepository<ProductType>
        {
            private List<ProductType> _productTypes = new List<ProductType>()
            {
                new ProductType() { Id = 1, Name = "Cà Phê", CategoryId = 1, CategoryName="Thức uống" },
                new ProductType() { Id = 2, Name = "Trà", CategoryId = 1, CategoryName="Thức uống" },
                new ProductType() { Id = 3, Name = "Freeze", CategoryId = 1, CategoryName="Thức uống" },
                new ProductType() { Id = 4, Name = "Bánh", CategoryId = 2, CategoryName="Đồ ăn" },
            };

            public List<ProductType> GetAll()
            {
                return _productTypes;
            }
            public ProductType GetById(string id)
            {
                var productType = _productTypes.FirstOrDefault(p => p.Id == int.Parse(id));
                return productType;
            }
            public int Insert(ProductType info)
            {
                _productTypes.Add(info);
                return 1;
            }
            public int DeleteById(string id)
            {
                var result = _productTypes.RemoveAll(p => p.Id == int.Parse(id));
                return result;
            }
            public int UpdateById(string id, ProductType info)
            {
                var productType = _productTypes.FirstOrDefault(p => p.Id == int.Parse(id));
                if (productType != null)
                {
                    productType.Name = info.Name;
                    productType.CategoryId = info.CategoryId;
                    productType.CategoryName = info.CategoryName;
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private class CategoryRepository : IRepository<Category>
        {
            private List<Category> _categories = new List<Category>()
            {
                new Category() { Id = 1, Name = "Thức uống" },
                new Category() { Id = 2, Name = "Đồ ăn" },
            };

            public List<Category> GetAll()
            {
                return _categories;
            }

            public int DeleteById(string id)
            {
                _categories.RemoveAll(p => p.Id == int.Parse(id));
                return 1;
            }

            public Category GetById(string id)
            {
                var category = _categories.FirstOrDefault(p => p.Id == int.Parse(id));
                return category;
            }

            public int Insert(Category info)
            {
                _categories.Add(info);
                return 1;
            }

            public int UpdateById(string id, Category info)
            {
                var category = _categories.FirstOrDefault(p => p.Id == int.Parse(id));
                if (category != null)
                {
                    category.Name = info.Name;
                    return 1;
                }

                return 0;
            }
        }

        private class CustomerRankRepository : IRepository<CustomerRank>
        {
            private List<CustomerRank> _customerRanks = new List<CustomerRank>()
            {
                new CustomerRank() { Id = 1, Name = "Mới", PromotionPoint = 0, DiscountPercentage = 0},
                new CustomerRank() { Id = 2, Name = "Bạc", PromotionPoint = 200, DiscountPercentage = 2 },
                new CustomerRank() { Id = 3, Name = "Vàng", PromotionPoint = 1000, DiscountPercentage = 5 },
                new CustomerRank() { Id = 4, Name = "Kim Cương", PromotionPoint = 3000, DiscountPercentage = 10 },
            };
            public List<CustomerRank> GetAll()
            {
                return _customerRanks;
            }
            public int DeleteById(string id)
            {
                _customerRanks.RemoveAll(p => p.Id == int.Parse(id));
                return 1;
            }
            public CustomerRank GetById(string id)
            {
                var customerRank = _customerRanks.FirstOrDefault(p => p.Id == int.Parse(id));
                return customerRank;
            }
            public int Insert(CustomerRank info)
            {
                _customerRanks.Add(info);
                return 1;
            }
            public int UpdateById(string id, CustomerRank info)
            {
                var customerRank = _customerRanks.FirstOrDefault(p => p.Id == int.Parse(id));
                if (customerRank != null)
                {
                    customerRank.Name = info.Name;
                    return 1;
                }

                return 0;
            }
        }

        private class CustomerRepository : IRepository<Customer>
        {
            private List<Customer> _customers = new List<Customer>()
            {
                new Customer() { Id = 1, Name = "Đỗ Minh An", Phone = "0912345678", Point = 1200, RegistrationDate = new DateOnly(2021, 5, 10), RankId = 1, RankName = "Mới", PromotionPoint = 200, DiscountPercentage = 0 },
                new Customer() { Id = 2, Name = "Nguyễn Minh Duy", Phone = "0909876543", Point = 850, RegistrationDate = new DateOnly(2020, 7, 25), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 3, Name = "Lê Thị Lan", Phone = "0923456789", Point = 2500, RegistrationDate = new DateOnly(2019, 9, 30), RankId = 3, RankName = "Vàng", PromotionPoint = 3000, DiscountPercentage = 5 },
                new Customer() { Id = 4, Name = "Phan Thị Kim", Phone = "0932123456", Point = 3100, RegistrationDate = new DateOnly(2018, 11, 1), RankId = 4, RankName = "Kim Cương", PromotionPoint = int.MaxValue, DiscountPercentage = 10 },
                new Customer() { Id = 5, Name = "Bùi Thanh Mai", Phone = "0943234567", Point = 780, RegistrationDate = new DateOnly(2021, 2, 12), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 6, Name = "Trần Quang Hiếu", Phone = "0954345678", Point = 3300, RegistrationDate = new DateOnly(2019, 3, 15), RankId = 3, RankName = "Vàng", PromotionPoint = 3000, DiscountPercentage = 5 },
                new Customer() { Id = 7, Name = "Nguyễn Thị Hoa", Phone = "0965456789", Point = 1400, RegistrationDate = new DateOnly(2020, 8, 22), RankId = 1, RankName = "Mới", PromotionPoint = 200, DiscountPercentage = 0 },
                new Customer() { Id = 8, Name = "Phạm Minh Khôi", Phone = "0976567890", Point = 1000, RegistrationDate = new DateOnly(2020, 12, 30), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 9, Name = "Lê Quang Vũ", Phone = "0987678901", Point = 650, RegistrationDate = new DateOnly(2021, 6, 5), RankId = 1, RankName = "Mới", PromotionPoint = 200, DiscountPercentage = 0 },
                new Customer() { Id = 10, Name = "Vũ Minh Tân", Phone = "0998789012", Point = 900, RegistrationDate = new DateOnly(2020, 1, 13), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 11, Name = "Đặng Phương Thảo", Phone = "0910112233", Point = 1800, RegistrationDate = new DateOnly(2019, 5, 20), RankId = 3, RankName = "Vàng", PromotionPoint = 3000, DiscountPercentage = 5 },
                new Customer() { Id = 12, Name = "Trương Mai Linh", Phone = "0901223344", Point = 2500, RegistrationDate = new DateOnly(2018, 8, 10), RankId = 3, RankName = "Vàng", PromotionPoint = 3000, DiscountPercentage = 5 },
                new Customer() { Id = 13, Name = "Nguyễn Thị Như", Phone = "0922334455", Point = 300, RegistrationDate = new DateOnly(2021, 7, 30), RankId = 1, RankName = "Mới", PromotionPoint = 200, DiscountPercentage = 0 },
                new Customer() { Id = 14, Name = "Bùi Ngọc Phương", Phone = "0933445566", Point = 1400, RegistrationDate = new DateOnly(2021, 1, 8), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 15, Name = "Trần Minh Long", Phone = "0944556677", Point = 2200, RegistrationDate = new DateOnly(2020, 3, 14), RankId = 3, RankName = "Vàng", PromotionPoint = 3000, DiscountPercentage = 5 },
                new Customer() { Id = 16, Name = "Nguyễn Thái Sơn", Phone = "0955667788", Point = 4000, RegistrationDate = new DateOnly(2019, 2, 25), RankId = 4, RankName = "Kim Cương", PromotionPoint = int.MaxValue, DiscountPercentage = 10 },
                new Customer() { Id = 17, Name = "Lê Hoàng An", Phone = "0966778899", Point = 2100, RegistrationDate = new DateOnly(2020, 11, 1), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 18, Name = "Phan Thanh Kiều", Phone = "0977889900", Point = 1500, RegistrationDate = new DateOnly(2021, 4, 10), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 },
                new Customer() { Id = 19, Name = "Nguyễn Thị Thu", Phone = "0988990011", Point = 600, RegistrationDate = new DateOnly(2021, 3, 3), RankId = 1, RankName = "Mới", PromotionPoint = 200, DiscountPercentage = 0 },
                new Customer() { Id = 20, Name = "Vũ Minh Quang", Phone = "0999001122", Point = 950, RegistrationDate = new DateOnly(2020, 9, 10), RankId = 2, RankName = "Bạc", PromotionPoint = 1000, DiscountPercentage = 2 }
            };

            public List<Customer> GetAll()
            {
                return _customers;
            }

            public int DeleteById(string id)
            {
                _customers.RemoveAll(p => p.Id == int.Parse(id));
                return 1;
            }

            public Customer GetById(string id)
            {
                var customer = _customers.FirstOrDefault(p => p.Id == int.Parse(id));
                return customer;
            }

            public int Insert(Customer info)
            {
                _customers.Add(info);
                return 1;
            }

            public int UpdateById(string id, Customer info)
            {
                var customer = _customers.FirstOrDefault(p => p.Id == int.Parse(id));

                if (customer != null)
                {
                    customer.Name = info.Name;
                    customer.Phone = info.Phone;
                    customer.Point = info.Point;
                    customer.RegistrationDate = info.RegistrationDate;
                    customer.RankId = info.RankId;
                    customer.RankName = info.RankName;
                    customer.PromotionPoint = info.PromotionPoint;
                    customer.DiscountPercentage = info.DiscountPercentage;
                    return 1;
                }

                return 0;
            }
        }
    }
}
