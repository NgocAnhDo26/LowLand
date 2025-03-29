using System;
using System.Collections.Generic;
using System.Diagnostics;
using LowLand.Model.Customer;
using LowLand.Model.Order;
using LowLand.Model.Product;
using Npgsql;

namespace LowLand.Services
{
    public class PostgreDao : IDao
    {
        //  private readonly string connectionString = "Host=localhost;Port=5432;Username=hoangkha_ngocanhne;Password=ngocanh_hoangkhane;Database=lowland";

        public IRepository<Customer> Customers { get; set; } = new CustomerRepository();
        public IRepository<CustomerRank> CustomerRanks { get; set; } = new CustomerRankRepository();
        public IRepository<Order> Orders { get; set; } = new OrderRepository();
        public IRepository<OrderDetail> OrderDetails { get; set; } = new OrderDetailRepository();
        public IRepository<Category> Categories { get; set; } = new CategoryRepository();
        public IRepository<Product> Products { get; set; } = new ProductRepository();
        public IRepository<ProductOption> ProductOptions { get; set; } = new ProductOptionRepository();
    }
    public abstract class BaseRepository<T>
    {
        protected readonly string connectionString = "Host=localhost;Port=5432;Username=admin;Password=1234;Database=myshop";

        protected List<T> ExecuteQuery(string query, Func<NpgsqlDataReader, T> mapFunction)
        {
            var results = new List<T>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(mapFunction(reader));
                    }
                }
            }
            return results;
        }

        protected T? ExecuteSingleQuery(string query, Func<NpgsqlDataReader, T> mapFunction)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read() ? mapFunction(reader) : default;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return default;
            }
        }

        protected int ExecuteNonQuery(string query)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class CustomerRepository : BaseRepository<Customer>, IRepository<Customer>
    {
        public List<Customer> GetAll()
        {
            return ExecuteQuery("""
            SELECT 
                c.customer_id, c.name, c.phone, c.point, c.registration_date, c.customer_rank_id, 
                cr.customer_rank_name, cr.promotion_point, cr.discount_percentage 
            FROM customer c LEFT JOIN customer_rank cr ON c.customer_rank_id = cr.customer_rank_id
            """, reader => new Customer
            {
                Id = reader.GetInt32(reader.GetOrdinal("customer_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                Point = reader.GetInt32(reader.GetOrdinal("point")),
                RegistrationDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("registration_date"))),
                RankId = reader.GetInt32(reader.GetOrdinal("customer_rank_id")),
                RankName = reader.GetString(reader.GetOrdinal("customer_rank_name")),
                PromotionPoint = reader.IsDBNull(reader.GetOrdinal("promotion_point")) ? null : reader.GetInt32(reader.GetOrdinal("promotion_point")),
                DiscountPercentage = reader.IsDBNull(reader.GetOrdinal("discount_percentage")) ? null : reader.GetInt32(reader.GetOrdinal("discount_percentage"))
            });
        }

        public Customer GetById(string id)
        {
            return ExecuteSingleQuery($"""
            SELECT 
                c.customer_id, c.name, c.phone, c.point, c.registration_date, c.customer_rank_id, 
                cr.customer_rank_name, cr.promotion_point, cr.discount_percentage 
            FROM customer c LEFT JOIN customer_rank cr ON c.customer_rank_id = cr.customer_rank_id
            WHERE c.customer_id = '{id}'
            """, reader => new Customer
            {
                Id = reader.GetInt32(reader.GetOrdinal("customer_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                Point = reader.GetInt32(reader.GetOrdinal("point")),
                RegistrationDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("registration_date"))),
                RankId = reader.GetInt32(reader.GetOrdinal("customer_rank_id")),
                RankName = reader.IsDBNull(reader.GetOrdinal("customer_rank_name")) ? null : reader.GetString(reader.GetOrdinal("customer_rank_name")),
                PromotionPoint = reader.IsDBNull(reader.GetOrdinal("promotion_point")) ? null : reader.GetInt32(reader.GetOrdinal("promotion_point")),
                DiscountPercentage = reader.IsDBNull(reader.GetOrdinal("discount_percentage")) ? null : reader.GetInt32(reader.GetOrdinal("discount_percentage"))
            })!;
        }

        public int Insert(Customer info) => ExecuteNonQuery($"""
             INSERT INTO customer (name, phone, point, registration_date, customer_rank_id) 
             VALUES ('{info.Name}', 
             '{info.Phone}',
             {info.Point},
             '{info.RegistrationDate.ToDateTime(new TimeOnly(0, 0))}',
             {info.RankId}
             )
             """);

        public int DeleteById(string id) => ExecuteNonQuery($"""
            DELETE FROM customer WHERE customer_id = '{id}'
            """);

        public int UpdateById(string id, Customer info)
        {
            int affectedRows = ExecuteNonQuery($"""
            UPDATE customer SET 
            name = '{info.Name}'
            , phone = '{info.Phone}'
            , point = {info.Point}
            , registration_date = '{info.RegistrationDate.ToDateTime(new TimeOnly(0, 0))}'
            , customer_rank_id = {info.RankId}
            WHERE customer_id = '{id}'
            """);


            UpdateCustomerRank(id);

            return affectedRows;
        }

        // extraclass 
        public void UpdateCustomerRank(string customerId)
        {
            ExecuteNonQuery($"""
            UPDATE customer c
            SET customer_rank_id = (
                SELECT customer_rank_id 
                FROM customer_rank 
                WHERE promotion_point <= c.point 
                ORDER BY promotion_point DESC
                LIMIT 1
            )
            WHERE c.customer_id = '{customerId}'
            """);
        }
    }



    public class CustomerRankRepository : BaseRepository<CustomerRank>, IRepository<CustomerRank>
    {
        public int DeleteById(string id) => ExecuteNonQuery($"""
            DELETE FROM customer_rank WHERE customer_rank_id = '{id}'
            """);

        public List<CustomerRank> GetAll()
        {
            return ExecuteQuery("""
                SELECT customer_rank_id, customer_rank_name, promotion_point, discount_percentage 
                FROM customer_rank
                """, reader => new CustomerRank
            {
                Id = reader.GetInt32(reader.GetOrdinal("customer_rank_id")),
                Name = reader.GetString(reader.GetOrdinal("customer_rank_name")),
                PromotionPoint = reader.GetInt32(reader.GetOrdinal("promotion_point")),
                DiscountPercentage = reader.GetInt32(reader.GetOrdinal("discount_percentage"))
            });
        }

        public CustomerRank GetById(string id)
        {
            return ExecuteSingleQuery($"""
              SELECT customer_rank_id, customer_rank_name, promotion_point, discount_percentage 
              FROM customer_rank 
              WHERE customer_rank_id = '{id}'
              """, reader => new CustomerRank
            {
                Id = reader.GetInt32(reader.GetOrdinal("customer_rank_id")),
                Name = reader.GetString(reader.GetOrdinal("customer_rank_name")),
                PromotionPoint = reader.GetInt32(reader.GetOrdinal("promotion_point")),
                DiscountPercentage = reader.GetInt32(reader.GetOrdinal("discount_percentage"))
            })!;
        }

        public int Insert(CustomerRank info) => ExecuteNonQuery($"""
            INSERT INTO customer_rank (customer_rank_name, promotion_point, discount_percentage) 
            VALUES (
            '{info.Name}'
            , {info.PromotionPoint}
            , {info.DiscountPercentage})
            """);

        public int UpdateById(string id, CustomerRank info) => ExecuteNonQuery($"""
             UPDATE customer_rank 
             SET customer_rank_name = '{info.Name}'
             , promotion_point = {info.PromotionPoint}
             , discount_percentage = {info.DiscountPercentage}
             WHERE customer_rank_id = '{id}'
             """);
    }





    public class OrderRepository : BaseRepository<Order>, IRepository<Order>
    {
        public int DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public List<Order> GetAll()
        {
            throw new NotImplementedException();
        }

        public Order GetById(string id)
        {
            throw new NotImplementedException();
        }

        public int Insert(Order info)
        {
            throw new NotImplementedException();
        }

        public int UpdateById(string id, Order info)
        {
            throw new NotImplementedException();
        }
    }
    public class OrderDetailRepository : BaseRepository<OrderDetail>, IRepository<OrderDetail>
    {
        public int DeleteById(string id)
        {
            throw new NotImplementedException();
        }
        public List<OrderDetail> GetAll()
        {
            throw new NotImplementedException();
        }
        public OrderDetail GetById(string id)
        {
            throw new NotImplementedException();
        }
        public int Insert(OrderDetail info)
        {
            throw new NotImplementedException();
        }
        public int UpdateById(string id, OrderDetail info)
        {
            throw new NotImplementedException();
        }
    }

    public class CategoryRepository : BaseRepository<Category>, IRepository<Category>
    {
        public int DeleteById(string id)
        {
            throw new NotImplementedException();
        }
        public List<Category> GetAll()
        {
            return ExecuteQuery("""
                SELECT category_id, name 
                FROM category
                """, reader => new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("category_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
            });
        }

        public Category GetById(string id)
        {
            throw new NotImplementedException();
        }

        public int Insert(Category info)
        {
            throw new NotImplementedException();
        }

        public int UpdateById(string id, Category info)
        {
            throw new NotImplementedException();
        }
    }

    public class ProductRepository : BaseRepository<Product>, IRepository<Product>
    {
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
                DELETE FROM product WHERE product_id = '{id}'
            """);
        }

        public List<Product> GetAll()
        {
            return ExecuteQuery("""
                SELECT p.product_id, p.name, p.sale_price, p.cost_price, p.image, p.is_combo, 
                       c.category_id, c.name AS category_name, 
                       pt.product_type_id, pt.name AS product_type_name 
                FROM product p
                LEFT JOIN product_type pt ON p.product_type_id = pt.product_type_id
                LEFT JOIN category c ON pt.category_id = c.category_id
            """, reader =>
            {
                bool isCombo = reader.GetBoolean(reader.GetOrdinal("is_combo"));
                return isCombo
                    ? new ComboProduct
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("product_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
                        CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                        Image = reader.GetString(reader.GetOrdinal("image")),
                        ProductIds = GetComboProductItems(reader.GetInt32(reader.GetOrdinal("product_id")))
                    }
                    : new SingleProduct
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("product_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
                        CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                        Image = reader.GetString(reader.GetOrdinal("image")),
                        Category = new Category
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("category_id")),
                            Name = reader.GetString(reader.GetOrdinal("category_name"))
                        },
                    };
            });
        }

        public Product? GetById(string id)
        {
            return ExecuteSingleQuery($"""
                SELECT p.product_id, p.name, p.sale_price, p.cost_price, p.image, p.is_combo, 
                       c.category_id, c.name AS category_name, 
                       pt.product_type_id, pt.name AS product_type_name 
                FROM product p
                LEFT JOIN product_type pt ON p.product_type_id = pt.product_type_id
                LEFT JOIN category c ON pt.category_id = c.category_id
                WHERE p.product_id = '{id}'
            """, reader =>
            {
                bool isCombo = reader.GetBoolean(reader.GetOrdinal("is_combo"));
                return isCombo
                    ? new ComboProduct
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("product_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
                        CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                        Image = reader.GetString(reader.GetOrdinal("image")),
                        ProductIds = GetComboProductItems(reader.GetInt32(reader.GetOrdinal("product_id")))
                    }
                    : new SingleProduct
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("product_id")),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
                        CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                        Image = reader.GetString(reader.GetOrdinal("image")),
                        Category = new Category
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("category_id")),
                            Name = reader.GetString(reader.GetOrdinal("category_name"))
                        },
                    };
            });
        }

        public int Insert(Product info)
        {
            if (info is SingleProduct single)
            {
                return ExecuteNonQuery($"""
            INSERT INTO product (is_combo, name, sale_price, cost_price, image,product_type_id) 
            VALUES (false, '{single.Name}', {single.SalePrice}, {single.CostPrice}, '{single.Image}')
        """);
            }
            else if (info is ComboProduct combo)
            {
                int result = ExecuteNonQuery($"""
            INSERT INTO product (is_combo, name, sale_price, cost_price, image) 
            VALUES (true, '{combo.Name}', {combo.SalePrice}, {combo.CostPrice}, '{combo.Image}')
        """);

                int comboId = GetLastInsertedId();
                foreach (var productId in combo.ProductIds)
                {
                    ExecuteNonQuery($"""
                INSERT INTO combo (product_id_combo, product_id) 
                VALUES ({comboId}, {productId})
            """);
                }
                return result;
            }
            return 0;
        }

        public int UpdateById(string id, Product info)
        {
            if (info is SingleProduct single)
            {
                return ExecuteNonQuery($"""
            UPDATE product 
            SET name = '{single.Name}', 
                sale_price = {single.SalePrice}, 
                cost_price = {single.CostPrice}, 
                image = '{single.Image}',
            WHERE product_id = '{id}'
        """);
            }
            else if (info is ComboProduct combo)
            {
                int result = ExecuteNonQuery($"""
            UPDATE product 
            SET name = '{combo.Name}', 
                sale_price = {combo.SalePrice}, 
                cost_price = {combo.CostPrice}, 
                image = '{combo.Image}', 
                is_combo = true
            WHERE product_id = '{id}'
        """);

                ExecuteNonQuery($"""
            DELETE FROM combo WHERE product_id_combo = '{id}'
        """);
                foreach (var productId in combo.ProductIds)
                {
                    ExecuteNonQuery($"""
                INSERT INTO combo (product_id_combo, product_id) 
                VALUES ({id}, {productId})
            """);
                }
                return result;
            }
            return 0;
        }


        private List<int> GetComboProductItems(int comboId)
        {
            var results = new List<int>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand($"""
        SELECT product_id FROM combo WHERE product_id_combo = {comboId}
        """, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetInt32(reader.GetOrdinal("product_id")));
                    }
                }
            }
            return results;
        }




        private int GetLastInsertedId()
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT LAST_INSERT_ID()", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? reader.GetInt32(0) : 0;
                }
            }
        }


    }

    internal class ProductOptionRepository : IRepository<ProductOption>
    {
        public int DeleteById(string id)
        {
            throw new NotImplementedException();
        }

        public List<ProductOption> GetAll()
        {
            throw new NotImplementedException();
        }

        public ProductOption GetById(string id)
        {
            throw new NotImplementedException();
        }

        public int Insert(ProductOption info)
        {
            throw new NotImplementedException();
        }

        public int UpdateById(string id, ProductOption info)
        {
            throw new NotImplementedException();
        }
    }
}
