using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowLand.Model.Customer;
using Npgsql;
using LowLand.Model.Order;
using LowLand.Model.Product;

namespace LowLand.Services
{
    public class PostgreDAO : IDao
    {
      //  private readonly string connectionString = "Host=localhost;Port=5432;Username=hoangkha_ngocanhne;Password=ngocanh_hoangkhane;Database=lowland";

        public IRepository<Customer> Customers { get; set; } = new CustomerRepository();
        public IRepository<CustomerRank> CustomerRanks { get; set; } = new CustomerRankRepository();
        public IRepository<Order> Orders { get; set; } = new OrderRepository();
        public IRepository<OrderDetail> OrderDetails { get; set; } = new OrderDetailRepository();
        public IRepository<Category> Categories { get; set; } = new CategoryRepository();
        public IRepository<Product> Products { get; set; } = new ProductRepository();
        public IRepository<ProductType> ProductTypes { get; set; } = new ProductTypeRepository();
    }
    public abstract class BaseRepository<T>
    {
        protected readonly string connectionString = "Host=localhost;Port=5432;Username=hoangkha_ngocanhne;Password=ngocanh_hoangkhane;Database=lowland";

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
                DiscountPercentage =  reader.GetInt32(reader.GetOrdinal("discount_percentage"))
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
              PromotionPoint =  reader.GetInt32(reader.GetOrdinal("promotion_point")),
              DiscountPercentage =reader.GetInt32(reader.GetOrdinal("discount_percentage"))
          })!;

        }

        public int Insert(CustomerRank info) => ExecuteNonQuery($"""
            INSERT INTO customer_rank (customer_rank_name, promotion_point, discount_percentage) 
            VALUES (
            '{info.Name}'
            , {info.PromotionPoint }
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        public List<Product> GetAll()
        {
            throw new NotImplementedException();
        }
        public Product GetById(string id)
        {
            throw new NotImplementedException();
        }
        public int Insert(Product info)
        {
            throw new NotImplementedException();
        }
        public int UpdateById(string id, Product info)
        {
            throw new NotImplementedException();
        }
    }


    public class ProductTypeRepository : BaseRepository<ProductType>, IRepository<ProductType>
    {
        public int DeleteById(string id)
        {
            throw new NotImplementedException();
        }
        public List<ProductType> GetAll()
        {
            throw new NotImplementedException();
        }
        public ProductType GetById(string id)
        {
            throw new NotImplementedException();
        }
        public int Insert(ProductType info)
        {
            throw new NotImplementedException();
        }
        public int UpdateById(string id, ProductType info)
        {
            throw new NotImplementedException();
        }
    }
}
