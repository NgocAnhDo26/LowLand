using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Customer;
using LowLand.Model.Order;
using LowLand.Model.Product;

namespace LowLand.Services
{
    public class MockDAO : IDao
    {
        public IRepository<Product> Products { get; set; } = new ProductRepository();
        public IRepository<Category> Categories { get; set; } = new CategoryRepository();
        public IRepository<Order> Orders { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IRepository<OrderDetail> OrderDetails { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IRepository<Customer> Customers { get; set; } = new CustomerRepository();
        public IRepository<CustomerRank> CustomerRanks { get; set; } = new CustomerRankRepository();
        public IRepository<ProductOption> ProductOptions { get; set; } = new ProductOptionRepository();

        private class ProductRepository : IRepository<Product>
        {
            private List<Product> _products = new List<Product>()
            {
                new SingleProduct() { Id = 1, Category = new Category() {Id = 1, Name = "Cà phê" }, Name = "Phin Sữa Đá", SalePrice = 29000, CostPrice = 15000, Image = "phin_sua_da.jpg"},
                new SingleProduct() { Id = 2, Category = new Category() { Id = 1, Name = "Cà phê" }, Name = "Phin Đen Đá", SalePrice = 29000, CostPrice = 15000, Image = "phin_den_da.jpg"},
                new SingleProduct() { Id = 3, Category = new Category() { Id = 1, Name = "Cà phê" }, Name = "PhinDi Kem Sữa", SalePrice = 35000, CostPrice = 18000, Image = "phindi_kem_sua.jpg"},
                new SingleProduct() { Id = 4, Category = new Category() { Id = 4, Name = "Cà phê" }, Name = "PhinDi Choco", SalePrice = 35000, CostPrice = 18000, Image = "phindi_choco.jpg"},
                new SingleProduct() { Id = 5, Category = new Category() { Id = 5, Name = "Cà phê" }, Name = "Espresso", SalePrice = 35000, CostPrice = 18000, Image = "espresso.jpg"},
                new SingleProduct() { Id = 6, Category = new Category() { Id = 6, Name = "Cà phê" }, Name = "Americano", SalePrice = 35000, CostPrice = 18000, Image = "americano.jpg"},
                new SingleProduct() { Id = 7, Category = new Category() { Id = 7, Name = "Cà phê" }, Name = "Latte", SalePrice = 40000, CostPrice = 20000, Image = "latte.jpg"},
                new SingleProduct() { Id = 8, Category = new Category() { Id = 8, Name = "Cà phê" }, Name = "Cappuccino", SalePrice = 40000, CostPrice = 20000, Image = "cappuccino.jpg" },
                new SingleProduct() { Id = 9, Category = new Category() { Id = 9, Name = "Cà phê" }, Name = "Caramel Macchiato", SalePrice = 45000, CostPrice = 22000, Image = "caramel_macchiato.jpg"},
                new SingleProduct() { Id = 10, Category = new Category() { Id = 10, Name = "Trà" },Name = "Trà Sen Vàng", SalePrice = 39000, CostPrice = 20000, Image = "tra_sen_vang.jpg"},
                new SingleProduct() { Id = 11, Category = new Category() { Id = 11, Name = "Trà" }, Name = "Trà Thạch Đào", SalePrice = 39000, CostPrice = 20000, Image = "tra_thach_dao.jpg" },
                new SingleProduct() { Id = 12, Category = new Category() { Id = 12, Name = "Trà" }, Name = "Trà Thạch Vải", SalePrice = 39000, CostPrice = 20000, Image = "tra_thach_vai.jpg" },
                new SingleProduct() { Id = 13, Category = new Category() { Id = 13, Name = "Trà" }, Name = "Trà Thanh Đào", SalePrice = 39000, CostPrice = 20000, Image = "tra_thanh_dao.jpg" },
                new SingleProduct() { Id = 14, Category = new Category() { Id = 14, Name = "Freeze" }, Name = "Freeze Trà Xanh", SalePrice = 49000, CostPrice = 25000, Image = "freeze_tra_xanh.jpg" },
                new SingleProduct() { Id = 15, Category = new Category() { Id = 15, Name = "Freeze" }, Name = "Freeze Cookies & Cream", SalePrice = 49000, CostPrice = 25000, Image = "freeze_cookies_cream.jpg" },
                new SingleProduct() { Id = 16, Category = new Category() { Id = 16, Name = "Freeze" }, Name = "Freeze Chocolate", SalePrice = 49000, CostPrice = 25000, Image = "freeze_chocolate.jpg" },
                new SingleProduct() { Id = 17, Category = new Category() { Id = 17, Name = "Bánh ngọt" }, Name = "Bánh Croissant", SalePrice = 29000, CostPrice = 15000, Image = "banh_croissant.jpg" },
                new SingleProduct() { Id = 18, Category = new Category() { Id = 18, Name = "Bánh ngọt" }, Name = "Bánh Mousse Cacao", SalePrice = 35000, CostPrice = 18000, Image = "banh_mousse_cacao.jpg" },
                new SingleProduct() { Id = 19, Category = new Category() { Id = 19, Name = "Bánh ngọt" }, Name = "Bánh Mousse Đào", SalePrice = 35000, CostPrice = 18000, Image = "banh_mousse_dao.jpg" },
                new SingleProduct() { Id = 20, Category = new Category() { Id = 20, Name = "Bánh ngọt" }, Name = "Bánh Phô Mai Chanh Dây", SalePrice = 29000, CostPrice = 15000, Image = "banh_pho_mai_chanh_day.jpg" },
            };
            public List<Product> GetAll()
            {
                return new List<Product>(_products);
            }

            public int DeleteById(string id)
            {
                int result = _products.RemoveAll(p => p.Id == int.Parse(id));
                return result;
            }


            public Product GetById(string id)
            {
                var product = _products.FirstOrDefault(p => p.Id == int.Parse(id));
                if (product is SingleProduct singleProduct)
                {
                    return new SingleProduct()
                    {
                        Id = singleProduct.Id,
                        Name = singleProduct.Name,
                        SalePrice = singleProduct.SalePrice,
                        CostPrice = singleProduct.CostPrice,
                        Image = singleProduct.Image,
                        Category = singleProduct.Category,
                    };
                }
                else if (product is ComboProduct comboProduct)
                {
                    return new ComboProduct()
                    {
                        Id = comboProduct.Id,
                        Name = comboProduct.Name,
                        SalePrice = comboProduct.SalePrice,
                        CostPrice = comboProduct.CostPrice,
                        Image = comboProduct.Image,
                        ProductIds = comboProduct.ProductIds
                    };
                }
                else
                {
                    return null;
                }
            }

            public int Insert(Product info)
            {
                int newId = _products.Max(p => p.Id) + 1;
                info.Id = newId;
                _products.Add(info);

                return newId;
            }

            public int UpdateById(string id, Product info)
            {
                var product = _products.FirstOrDefault(p => p.Id == int.Parse(id));

                if (product != null)
                {
                    product.Name = info.Name;
                    Debug.WriteLine(product.Name);
                    product.SalePrice = info.SalePrice;
                    product.CostPrice = info.CostPrice;
                    product.Image = info.Image;

                    if (product is SingleProduct prod)
                    {
                        prod.Category = ((SingleProduct)info).Category;
                    }
                    else if (product is ComboProduct combo)
                    {
                        combo.ProductIds = ((ComboProduct)info).ProductIds;
                    }

                    return int.Parse(id);
                }
                else
                {
                    return -1;
                }
            }
        }

        private class ProductOptionRepository : IRepository<ProductOption>
        {
            private List<ProductOption> _options = new List<ProductOption>()
            {
                new ProductOption() { OptionId = 1, ProductId = 1, Name = "Size S", SalePrice = 29000, CostPrice = 15000 },
                new ProductOption() { OptionId = 2, ProductId = 1, Name = "Size M", SalePrice = 39000, CostPrice = 22000 },
                new ProductOption() { OptionId = 3, ProductId = 1, Name = "Size L", SalePrice = 49000, CostPrice = 27000 },
                new ProductOption() { OptionId = 4, ProductId = 2, Name = "Size S", SalePrice = 29000, CostPrice = 15000 },
                new ProductOption() { OptionId = 5, ProductId = 2, Name = "Size M", SalePrice = 39000, CostPrice = 22000 },
                new ProductOption() { OptionId = 6, ProductId = 2, Name = "Size L", SalePrice = 49000, CostPrice = 27000 },
                new ProductOption() { OptionId = 7, ProductId = 3, Name = "Size S", SalePrice = 35000, CostPrice = 18000 },
                new ProductOption() { OptionId = 8, ProductId = 3, Name = "Size M", SalePrice = 49000, CostPrice = 25000 },
                new ProductOption() { OptionId = 9, ProductId = 3, Name = "Size L", SalePrice = 55000, CostPrice = 32000 },
                new ProductOption() { OptionId = 10, ProductId = 12, Name = "Size S", SalePrice = 39000, CostPrice = 20000 },
                new ProductOption() { OptionId = 11, ProductId = 12, Name = "Size M", SalePrice = 49000, CostPrice = 27000 },
                new ProductOption() { OptionId = 12, ProductId = 12, Name = "Size L", SalePrice = 55000, CostPrice = 35000 },
            };

            public List<ProductOption> GetAll()
            {
                return new List<ProductOption>(_options);
            }

            public List<ProductOption> GetByProductId(int productId)
            {
                var options = _options.Where(p => p.ProductId == productId).ToList();
                return new List<ProductOption>(options);
            }

            public ProductOption GetById(string id)
            {
                var option = _options.FirstOrDefault(p => p.OptionId == int.Parse(id));

                return new ProductOption()
                {
                    OptionId = option.OptionId,
                    ProductId = option.ProductId,
                    Name = option.Name,
                    SalePrice = option.SalePrice,
                    CostPrice = option.CostPrice
                };
            }

            public int Insert(ProductOption info)
            {
                int newId = _options.Max(p => p.OptionId) + 1;
                info.OptionId = newId;
                _options.Add(info);

                return newId;
            }

            public int DeleteById(string id)
            {
                var result = _options.RemoveAll(p => p.OptionId == int.Parse(id));
                return (result > 0) ? int.Parse(id) : -1;
            }

            public int UpdateById(string id, ProductOption info)
            {
                var option = _options.FirstOrDefault(p => p.OptionId == int.Parse(id));
                if (option != null)
                {
                    option.Name = info.Name;
                    option.SalePrice = info.SalePrice;
                    option.CostPrice = info.CostPrice;
                    return int.Parse(id);
                }
                else
                {
                    return -1;
                }
            }
        }

        private class CategoryRepository : IRepository<Category>
        {
            private List<Category> _categories = new List<Category>()
            {
                new Category() { Id = 1, Name = "Cà phê" },
                new Category() { Id = 2, Name = "Trà" },
                new Category() { Id = 3, Name = "Freeze" },
                new Category() { Id = 4, Name = "Bánh ngọt" },
            };

            public List<Category> GetAll()
            {
                return new List<Category>(_categories);
            }

            public int DeleteById(string id)
            {
                var result = _categories.RemoveAll(p => p.Id == int.Parse(id));
                return result > 0 ? int.Parse(id) : -1;
            }

            public Category GetById(string id)
            {
                var category = _categories.FirstOrDefault(p => p.Id == int.Parse(id));
                if (category != null)
                {
                    return new Category()
                    {
                        Id = category.Id,
                        Name = category.Name
                    };
                }

                return null;
            }

            public int Insert(Category info)
            {
                int newId = _categories.Max(p => p.Id) + 1;
                info.Id = newId;
                _categories.Add(info);

                return newId;
            }

            public int UpdateById(string id, Category info)
            {
                var category = _categories.FirstOrDefault(p => p.Id == int.Parse(id));
                if (category != null)
                {
                    category.Name = info.Name;
                    return category.Id;
                }

                return -1;
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
                return new List<CustomerRank>(_customerRanks);
            }

            public int DeleteById(string id)
            {
                var result = _customerRanks.RemoveAll(p => p.Id == int.Parse(id));
                return result > 0 ? int.Parse(id) : -1;
            }

            public CustomerRank GetById(string id)
            {
                var customerRank = _customerRanks.FirstOrDefault(p => p.Id == int.Parse(id));
                if (customerRank != null)
                {
                    return new CustomerRank()
                    {
                        Id = customerRank.Id,
                        Name = customerRank.Name,
                        PromotionPoint = customerRank.PromotionPoint,
                        DiscountPercentage = customerRank.DiscountPercentage
                    };
                }

                return null;
            }

            public int Insert(CustomerRank info)
            {
                int newId = _customerRanks.Max(p => p.Id) + 1;
                info.Id = newId;
                _customerRanks.Add(info);
                return newId;
            }

            public int UpdateById(string id, CustomerRank info)
            {
                var customerRank = _customerRanks.FirstOrDefault(p => p.Id == int.Parse(id));
                if (customerRank != null)
                {
                    customerRank.Name = info.Name;
                    return customerRank.Id;
                }

                return -1;
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
                return new List<Customer>(_customers);
            }

            public int DeleteById(string id)
            {
                var result = _customers.RemoveAll(p => p.Id == int.Parse(id));
                return result > 0 ? int.Parse(id) : -1;
            }

            public Customer GetById(string id)
            {
                var customer = _customers.FirstOrDefault(p => p.Id == int.Parse(id));
                if (customer != null)
                {
                    return new Customer()
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Phone = customer.Phone,
                        Point = customer.Point,
                        RegistrationDate = customer.RegistrationDate,
                        RankId = customer.RankId,
                        RankName = customer.RankName,
                        PromotionPoint = customer.PromotionPoint,
                        DiscountPercentage = customer.DiscountPercentage
                    };
                }

                return null;
            }

            public int Insert(Customer info)
            {
                int newId = _customers.Max(p => p.Id) + 1;
                info.Id = newId;
                _customers.Add(info);

                return newId;
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
                    return customer.Id;
                }

                return -1;
            }
        }
    }
}