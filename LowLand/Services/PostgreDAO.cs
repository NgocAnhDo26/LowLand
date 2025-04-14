using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using LowLand.Model.Customer;
using LowLand.Model.Discount;
using LowLand.Model.Order;
using LowLand.Model.Product;
using Npgsql;

namespace LowLand.Services
{
    public class PostgreDao : IDao
    {
        public IRepository<Customer> Customers { get; set; } = new CustomerRepository();
        public IRepository<CustomerRank> CustomerRanks { get; set; } = new CustomerRankRepository();
        public IRepository<Order> Orders { get; set; } = new OrderRepository();
        public IRepository<OrderDetail> OrderDetails { get; set; } = new OrderDetailRepository();
        public IRepository<Category> Categories { get; set; } = new CategoryRepository();
        public IRepository<Product> Products { get; set; } = new ProductRepository();
        public IRepository<ComboItem> ComboItems { get; set; } = new ComboItemRepository();
        public IRepository<ProductOption> ProductOptions { get; set; } = new ProductOptionRepository();
        public IRepository<Promotion> Promotions { get; set; } = new PromotionRepository();
    }

    public abstract class BaseRepository<T>
    {
        protected readonly string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");

        protected List<T> ExecuteQuery(string query, Func<NpgsqlDataReader, T> mapFunction)
        {
            var results = new List<T>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
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
                catch (Exception ex)
                {
                    throw new Exception($"Không thể kết nối với Database!");
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
                Rank = reader.IsDBNull(reader.GetOrdinal("customer_rank_id")) ? null : new CustomerRank
                {
                    Id = reader.GetInt32(reader.GetOrdinal("customer_rank_id")),
                    Name = reader.GetString(reader.GetOrdinal("customer_rank_name")),
                    PromotionPoint = reader.GetInt32(reader.GetOrdinal("promotion_point")),
                    DiscountPercentage = reader.GetInt32(reader.GetOrdinal("discount_percentage"))
                }
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
                Rank = new CustomerRank
                {
                    Id = reader.GetInt32(reader.GetOrdinal("customer_rank_id")),
                    Name = reader.GetString(reader.GetOrdinal("customer_rank_name")),
                    PromotionPoint = reader.GetInt32(reader.GetOrdinal("promotion_point")),
                    DiscountPercentage = reader.GetInt32(reader.GetOrdinal("discount_percentage"))
                },
            })!;
        }

        public int Insert(Customer info) => ExecuteNonQuery($"""
             INSERT INTO customer (name, phone, point, registration_date, customer_rank_id) 
             VALUES ('{info.Name}', 
             '{info.Phone}',
             {info.Point},
             '{info.RegistrationDate.ToString("yyyy-MM-dd")}',
             {info.Rank.Id}
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
            , registration_date = '{info.RegistrationDate.ToString("yyyy-MM-dd")}'
            , customer_rank_id = {info.Rank.Id}
            WHERE customer_id = '{id}'
            """);
            return affectedRows;
        }

        // extraclass 

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
        private readonly OrderDetailRepository _orderDetailRepository = new OrderDetailRepository();
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
            DELETE FROM "order" WHERE order_id = '{id}'
        """);
        }

        public List<Order> GetAll()
        {
            var orders = ExecuteQuery($"""
        SELECT order_id, customer_id, customer_phone, total_after_discount, total_price, status, promotion_id, date,customer_name
        FROM "order"
    """, reader => new Order
            {
                Id = reader.GetInt32(reader.GetOrdinal("order_id")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("customer_id")) ? null : reader.GetInt32(reader.GetOrdinal("customer_id")),
                CustomerStatus = reader.IsDBNull(reader.GetOrdinal("customer_id")) ? "Vãng lai" : "Thành viên",
                CustomerPhone = reader.IsDBNull(reader.GetOrdinal("customer_phone")) ? null : reader.GetString(reader.GetOrdinal("customer_phone")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("customer_name")) ? null : reader.GetString(reader.GetOrdinal("customer_name")),
                PromotionId = reader.IsDBNull(reader.GetOrdinal("promotion_id")) ? null : reader.GetInt32(reader.GetOrdinal("promotion_id")),
                TotalPrice = reader.GetInt32(reader.GetOrdinal("total_price")),
                TotalAfterDiscount = reader.GetInt32(reader.GetOrdinal("total_after_discount")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                Date = reader.GetDateTime(reader.GetOrdinal("date"))
            });

            foreach (var order in orders)
            {
                order.Details = new ObservableCollection<OrderDetail>(_orderDetailRepository.GetByOrderId(order.Id.ToString()));
            }

            return orders;
        }

        public Order GetById(string id)
        {
            var order = ExecuteSingleQuery($"""
        SELECT order_id, customer_id, customer_phone, total_after_discount, total_price, status, promotion_id, date,customer_name
        FROM "order" WHERE order_id = '{id}'
    """, reader => new Order
            {
                Id = reader.GetInt32(reader.GetOrdinal("order_id")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("customer_id")) ? null : reader.GetInt32(reader.GetOrdinal("customer_id")),
                CustomerStatus = reader.IsDBNull(reader.GetOrdinal("customer_id")) ? "Vãng lai" : "Thành viên",
                CustomerPhone = reader.IsDBNull(reader.GetOrdinal("customer_phone")) ? null : reader.GetString(reader.GetOrdinal("customer_phone")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("customer_name")) ? null : reader.GetString(reader.GetOrdinal("customer_name")),
                PromotionId = reader.GetInt32(reader.GetOrdinal("promotion_id")),
                TotalPrice = reader.GetInt32(reader.GetOrdinal("total_price")),
                TotalAfterDiscount = reader.GetInt32(reader.GetOrdinal("total_after_discount")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                Date = reader.GetDateTime(reader.GetOrdinal("date"))
            });

            if (order != null)
            {
                order.Details = new ObservableCollection<OrderDetail>(_orderDetailRepository.GetByOrderId(order.Id.ToString()));
            }

            return order!;
        }

        public int Insert(Order info)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO ""order"" (customer_id, customer_phone, customer_name, promotion_id, total_price, total_after_discount, status, date)
                VALUES (@CustomerId, @CustomerPhone, @CustomerName, @PromotionId, @TotalPrice, @TotalAfterDiscount, @Status, @Date)
                RETURNING order_id", conn);

            cmd.Parameters.AddWithValue("@CustomerId", (object?)info.CustomerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomerPhone", (object?)info.CustomerPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomerName", (object?)info.CustomerName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PromotionId", (object?)info.PromotionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TotalPrice", info.TotalPrice);
            cmd.Parameters.AddWithValue("@TotalAfterDiscount", info.TotalAfterDiscount);
            cmd.Parameters.AddWithValue("@Status", (object?)info.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Date", info.Date);

            var orderId = (int)cmd.ExecuteScalar();

            foreach (var detail in info.Details)
            {
                _orderDetailRepository.Insert(detail, orderId);
            }

            return orderId;
        }


        public int UpdateById(string id, Order info)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
        UPDATE ""order"" SET 
            customer_id = @CustomerId,
            customer_phone = @CustomerPhone,
            customer_name = @CustomerName,
            promotion_id = @PromotionId,
            total_price = @TotalPrice,
            total_after_discount = @TotalAfterDiscount,
            status = @Status,
            date = @Date
        WHERE order_id = @OrderId", conn);

            cmd.Parameters.AddWithValue("@CustomerId", (object?)info.CustomerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomerPhone", (object?)info.CustomerPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomerName", (object?)info.CustomerName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PromotionId", (object?)info.PromotionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TotalPrice", info.TotalPrice);
            cmd.Parameters.AddWithValue("@TotalAfterDiscount", info.TotalAfterDiscount);
            cmd.Parameters.AddWithValue("@Status", (object?)info.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Date", info.Date);


            if (!int.TryParse(id, out int orderId))
            {
                throw new ArgumentException("Invalid Order ID format");
            }
            cmd.Parameters.AddWithValue("@OrderId", orderId);

            int rowsAffected = cmd.ExecuteNonQuery();

            using var deleteCmd = new NpgsqlCommand(@"DELETE FROM order_detail WHERE order_id = @OrderId", conn);
            deleteCmd.Parameters.AddWithValue("@OrderId", orderId);
            deleteCmd.ExecuteNonQuery();

            foreach (var detail in info.Details)
            {
                _orderDetailRepository.Insert(detail, orderId);
            }

            return rowsAffected;
        }


    }
    public class OrderDetailRepository : BaseRepository<OrderDetail>, IRepository<OrderDetail>
    {
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
            DELETE FROM order_detail WHERE order_detail_id = '{id}'
        """);
        }

        public List<OrderDetail> GetAll()
        {
            throw new NotImplementedException();
        }

        public OrderDetail GetById(string id)
        {
            return ExecuteSingleQuery($"""
            SELECT order_detail_id, order_id, product_id, quantity, sale_price, product_name, option_id, option_name
            FROM order_detail WHERE order_detail_id = '{id}'
        """, reader => new OrderDetail
            {
                Id = reader.GetInt32(reader.GetOrdinal("order_detail_id")),
                OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                ProductId = reader.IsDBNull(reader.GetOrdinal("product_id")) ? null : reader.GetInt32(reader.GetOrdinal("product_id")),
                ProductPrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
                quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                Price = reader.GetInt32(reader.GetOrdinal("sale_price")),
                ProductName = reader.GetString(reader.GetOrdinal("product_name")),
                OptionId = reader.IsDBNull(reader.GetOrdinal("option_id")) ? null : reader.GetInt32(reader.GetOrdinal("option_id")),
                OptionName = reader.IsDBNull(reader.GetOrdinal("option_name")) ? null : reader.GetString(reader.GetOrdinal("option_name"))
            })!;
        }

        public int Insert(OrderDetail info)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
            INSERT INTO order_detail (order_id, product_id, product_price, quantity, sale_price, product_name, option_id, option_name)
            VALUES (@orderId, @productId, @productPrice, @quantity, @salePrice, @productName, @optionId, @optionName)", conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", info.OrderId);
                    cmd.Parameters.AddWithValue("@productId", info.ProductId);
                    cmd.Parameters.AddWithValue("@productPrice", info.ProductPrice);
                    cmd.Parameters.AddWithValue("@quantity", info.quantity);
                    cmd.Parameters.AddWithValue("@salePrice", info.Price);
                    cmd.Parameters.AddWithValue("@productName", info.ProductName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@optionId", info.OptionId.HasValue ? (object)info.OptionId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@optionName", string.IsNullOrEmpty(info.OptionName) ? (object)DBNull.Value : info.OptionName);

                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public int Insert(OrderDetail info, int OrderId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
            INSERT INTO order_detail (order_id, product_id, product_price, quantity, sale_price, product_name, option_id, option_name)
            VALUES (@orderId, @productId, @productPrice, @quantity, @salePrice, @productName, @optionId, @optionName)", conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", OrderId);
                    cmd.Parameters.AddWithValue("@productId", info.ProductId);
                    cmd.Parameters.AddWithValue("@productPrice", info.ProductPrice);
                    cmd.Parameters.AddWithValue("@quantity", info.quantity);
                    cmd.Parameters.AddWithValue("@salePrice", info.Price);
                    cmd.Parameters.AddWithValue("@productName", info.ProductName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@optionId", info.OptionId.HasValue ? (object)info.OptionId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@optionName", string.IsNullOrEmpty(info.OptionName) ? (object)DBNull.Value : info.OptionName);

                    return cmd.ExecuteNonQuery();
                }
            }
        }


        public int UpdateById(string id, OrderDetail info)
        {
            return ExecuteNonQuery($"""
            UPDATE order_detail SET 
                order_id = {info.OrderId},
                product_id = {info.ProductId},
                product_price = {info.ProductPrice},
                quantity = {info.quantity},
                sale_price = {info.Price},
                product_name = '{info.ProductName}',
                option_id = {info.OptionId},
                option_name = '{info.OptionName}'
            WHERE order_detail_id = '{id}'
        """);
        }
        public List<OrderDetail> GetByOrderId(string id)
        {
            return ExecuteQuery($"""
            SELECT order_detail_id, order_id, product_id,product_price, quantity, sale_price, product_name, option_id, option_name
            FROM order_detail WHERE order_id = '{id}'
            """, reader => new OrderDetail
            {
                Id = reader.GetInt32(reader.GetOrdinal("order_detail_id")),
                OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                ProductPrice = reader.GetInt32(reader.GetOrdinal("product_price")),
                quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                Price = reader.GetInt32(reader.GetOrdinal("sale_price")),
                ProductName = reader.GetString(reader.GetOrdinal("product_name")),
                OptionId = reader.IsDBNull(reader.GetOrdinal("option_id")) ? null : reader.GetInt32(reader.GetOrdinal("option_id")),
                OptionName = reader.IsDBNull(reader.GetOrdinal("option_name")) ? null : reader.GetString(reader.GetOrdinal("option_name"))
            });

        }
    }

    public class CategoryRepository : BaseRepository<Category>, IRepository<Category>
    {
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
            DELETE FROM category WHERE category_id = '{id}'
        """);
        }

        public List<Category> GetAll()
        {
            return ExecuteQuery($"""
            SELECT category_id, name FROM category
        """, reader => new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("category_id")),
                Name = reader.GetString(reader.GetOrdinal("name"))
            });
        }

        public Category? GetById(string id)
        {
            return ExecuteSingleQuery($"""
            SELECT category_id, name FROM category WHERE category_id = '{id}'
        """, reader => new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("category_id")),
                Name = reader.GetString(reader.GetOrdinal("name"))
            });
        }

        public int Insert(Category info)
        {
            return ExecuteNonQuery($"""
            INSERT INTO category (name) VALUES ('{info.Name}')
            """);

        }



        public int UpdateById(string id, Category info)
        {
            return ExecuteNonQuery($"""
            UPDATE category SET name = '{info.Name}' WHERE category_id = '{id}'
        """);
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
                       c.category_id, c.name AS category_name
                      
                FROM product p
                LEFT JOIN category c ON p.category_id = c.category_id
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
                        Image = reader.GetString(reader.GetOrdinal("image"))
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
                       c.category_id, c.name AS category_name
                  
                FROM product p

                LEFT JOIN category c ON p.category_id = c.category_id
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
                    INSERT INTO product (is_combo, name, sale_price, cost_price, image, category_id) 
                    VALUES (false, '{single.Name}', {single.SalePrice}, {single.CostPrice}, '{single.Image}', {single.Category.Id})
                """);
            }
            else if (info is ComboProduct combo)
            {
                return ExecuteNonQuery($"""
                    INSERT INTO product (is_combo, name, sale_price, cost_price, image) 
                    VALUES (true, '{combo.Name}', {combo.SalePrice}, {combo.CostPrice}, '{combo.Image}')
                """);
            }

            return -1;
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
                        category_id = {single.Category.Id}
                    WHERE product_id = '{id}'
                """);
            }
            else if (info is ComboProduct combo)
            {
                return ExecuteNonQuery($"""
                    UPDATE product 
                    SET name = '{combo.Name}', 
                        sale_price = {combo.SalePrice}, 
                        cost_price = {combo.CostPrice}, 
                        image = '{combo.Image}', 
                        is_combo = true
                    WHERE product_id = '{id}'
                """);
            }

            return -1;
        }
        //private List<ComboItem> GetComboProductItems(int comboId)
        //{
        //    var results = new List<ComboItem>();
        //    using (var conn = new NpgsqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        using (var cmd = new NpgsqlCommand($"""
        //            SELECT c.item_id, c.product_id, p.name, p.image, p.sale_price, p.cost_price, 
        //                   o.option_id, o.name AS option_name, o.sale_price as option_sale_price, o.cost_price as option_cost_price, c.quantity 
        //            FROM combo_item c
        //            LEFT JOIN product p ON p.product_id = c.product_id
        //            LEFT JOIN option o ON o.option_id = c.option_id
        //            WHERE c.combo_id = {comboId}
        //            """, conn))
        //        using (var reader = cmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                var item = new ComboItem
        //                {
        //                    ItemId = reader.GetInt32(reader.GetOrdinal("item_id")),
        //                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
        //                    Product = new SingleProduct
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("product_id")),
        //                        Name = reader.GetString(reader.GetOrdinal("name")),
        //                        Image = reader.GetString(reader.GetOrdinal("image")),
        //                        SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
        //                        CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price"))
        //                    },
        //                };

        //                var hasOption = !reader.IsDBNull(reader.GetOrdinal("option_id"));

        //                if (hasOption)
        //                {
        //                    item.Option = new ProductOption
        //                    {
        //                        OptionId = reader.GetInt32(reader.GetOrdinal("option_id")),
        //                        Name = reader.GetString(reader.GetOrdinal("option_name")),
        //                        SalePrice = reader.GetInt32(reader.GetOrdinal("option_sale_price")),
        //                        CostPrice = reader.GetInt32(reader.GetOrdinal("option_cost_price"))
        //                    };
        //                }

        //                results.Add(item);
        //            }
        //        }
        //    }

        //    return results;
        //}

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

    internal class ProductOptionRepository : BaseRepository<ProductOption>, IRepository<ProductOption>
    {
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
                DELETE FROM option WHERE option_id = '{id}'
            """);
        }

        public List<ProductOption> GetAll()
        {
            return ExecuteQuery("""
                SELECT option_id, product_id, name, cost_price, sale_price   
                FROM option 
            """, reader => new ProductOption
            {
                OptionId = reader.GetInt32(reader.GetOrdinal("option_id")),
                ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price"))
            });
        }

        public ProductOption GetById(string id)
        {
            return ExecuteSingleQuery($"""
                SELECT option_id, product_id, name, cost_price, sale_price
                  
                FROM option WHERE option_id = '{id}'

            """, reader => new ProductOption
            {
                OptionId = reader.GetInt32(reader.GetOrdinal("option_id")),
                ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price"))
            })!;
        }

        public int Insert(ProductOption info)
        {
            return ExecuteNonQuery($"""
                INSERT INTO option (product_id, name, cost_price, sale_price)
                VALUES ({info.ProductId}, '{info.Name}', {info.CostPrice}, {info.SalePrice})
                """);

        }

        public int UpdateById(string id, ProductOption info)
        {
            return ExecuteNonQuery($"""
            UPDATE option SET 
                product_id = {info.ProductId},
                name = '{info.Name}',
                cost_price = {info.CostPrice},
                sale_price = {info.SalePrice}
            WHERE option_id = '{id}'
            """);
        }
    }

    internal class ComboItemRepository : BaseRepository<ComboItem>, IRepository<ComboItem>
    {
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
                DELETE FROM combo_item WHERE item_id = '{id}'
            """);
        }

        public List<ComboItem> GetAll()
        {
            return ExecuteQuery("""
                SELECT c.item_id, c.combo_id, c.product_id, p.name AS product_name, p.category_id, cate.name AS category_name, p.sale_price, p.cost_price, p.image,
                    c.option_id, o.name AS option_name, o.sale_price AS option_sale_price, o.cost_price AS option_cost_price,
                    c.quantity
                FROM combo_item c
                LEFT JOIN product p ON c.product_id = p.product_id
                LEFT JOIN category cate ON p.category_id = cate.category_id
                LEFT JOIN option o ON c.option_id = o.option_id
            """, reader => new ComboItem
            {
                ItemId = reader.GetInt32(reader.GetOrdinal("item_id")),
                ComboId = reader.GetInt32(reader.GetOrdinal("combo_id")),
                Product = new SingleProduct
                {
                    Id = reader.GetInt32(reader.GetOrdinal("product_id")),
                    Name = reader.GetString(reader.GetOrdinal("product_name")),
                    SalePrice = reader.GetInt32(reader.GetOrdinal("sale_price")),
                    CostPrice = reader.GetInt32(reader.GetOrdinal("cost_price")),
                    Image = reader.GetString(reader.GetOrdinal("image")),
                    Category = new Category()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("category_id")),
                        Name = reader.GetString(reader.GetOrdinal("category_name"))
                    },
                },
                Option = reader.IsDBNull(reader.GetOrdinal("option_id")) ? null : new ProductOption
                {
                    OptionId = reader.GetInt32(reader.GetOrdinal("option_id")),
                    Name = reader.GetString(reader.GetOrdinal("option_name")),
                    SalePrice = reader.GetInt32(reader.GetOrdinal("option_sale_price")),
                    CostPrice = reader.GetInt32(reader.GetOrdinal("option_cost_price"))
                },
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity"))
            });
        }

        public ComboItem GetById(string id)
        {
            throw new NotImplementedException();
        }

        public int Insert(ComboItem info)
        {
            return ExecuteNonQuery($"""
                INSERT INTO combo_item (combo_id, product_id, option_id, quantity)
                VALUES ({info.ComboId}, '{info.Product.Id}', {(info.Option != null ? info.Option.OptionId.ToString() : "NULL")}, {info.Quantity})
            """);
        }

        public int UpdateById(string id, ComboItem info)
        {
            return ExecuteNonQuery($"""
                UPDATE combo_item SET 
                    combo_id = {info.ComboId},
                    product_id = '{info.Product.Id}',
                    option_id = {(info.Option != null ? info.Option.OptionId.ToString() : "NULL")},
                    quantity = {info.Quantity}
                WHERE item_id = '{id}'
                """);
        }
    }

    internal class PromotionRepository : BaseRepository<Promotion>, IRepository<Promotion>
    {
        public List<Promotion> GetAll()
        {
            return ExecuteQuery("""
                SELECT promotion_id, promotion_name, description, start_date, end_date, promotion_type, amount, minimum_order_value, is_active
                FROM promotion
            """, reader => new Promotion
            {
                Id = reader.GetInt32(reader.GetOrdinal("promotion_id")),
                Name = reader.GetString(reader.GetOrdinal("promotion_name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                StartDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("start_date"))),
                EndDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("end_date"))),
                Type = (PromotionType)reader.GetInt32(reader.GetOrdinal("promotion_type")),
                Amount = reader.GetDouble(reader.GetOrdinal("amount")),
                MinimumOrderValue = reader.GetInt32(reader.GetOrdinal("minimum_order_value")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            });
        }

        public Promotion GetById(string id)
        {
            return ExecuteSingleQuery($"""
                SELECT promotion_id, promotion_name, description, start_date, end_date, promotion_type, amount, minimum_order_value, is_active
                FROM promotion WHERE promotion_id = '{id}'
            """, reader => new Promotion
            {
                Id = reader.GetInt32(reader.GetOrdinal("promotion_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                StartDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("start_date"))),
                EndDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("end_date"))),
                Type = (PromotionType)reader.GetInt32(reader.GetOrdinal("promotion_type")),
                Amount = reader.GetDouble(reader.GetOrdinal("amount")),
                MinimumOrderValue = reader.GetInt32(reader.GetOrdinal("minimum_order_value")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active"))
            })!;
        }

        public int Insert(Promotion info)
        {
            return ExecuteNonQuery($"""
                INSERT INTO promotion (promotion_name, description, start_date, end_date, promotion_type, amount, minimum_order_value, is_active)
                VALUES ('{info.Name}', '{info.Description}', '{info.StartDate.ToString("yyyy-MM-dd")}', '{info.EndDate.ToString("yyyy-MM-dd")}', {(int)info.Type}, {info.Amount}, {(info.MinimumOrderValue == null ? "NULL" : info.MinimumOrderValue)}, {info.IsActive})
            """);
        }

        public int UpdateById(string id, Promotion info)
        {
            return ExecuteNonQuery($"""
                UPDATE promotion SET 
                    promotion_name = '{info.Name}',
                    description = '{info.Description}',
                    start_date = '{info.StartDate.ToString("yyyy-MM-dd")}',
                    end_date = '{info.EndDate.ToString("yyyy-MM-dd")}',
                    promotion_type = {(int)info.Type},
                    amount = {info.Amount},
                    minimum_order_value = {info.MinimumOrderValue},
                    is_active = {info.IsActive}
                WHERE promotion_id = '{id}'
            """);
        }
        public int DeleteById(string id)
        {
            return ExecuteNonQuery($"""
                DELETE FROM promotion WHERE promotion_id = '{id}'
                """);
        }
    }

}
