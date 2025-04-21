using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Customer;
using LowLand.Model.Discount;
using LowLand.Model.Order;
using LowLand.Model.Product;
using LowLand.Model.Table;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class AddOrderViewModel : INotifyPropertyChanged
    {
        private readonly IDao _dao;
        public ObservableCollection<Order> Orders { get; set; }
        public Order EditorAddOrder { get; set; }
        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public ObservableCollection<CustomerRank> CustomerRanks { get; set; }
        public ObservableCollection<Table> AvailableTables { get; set; }
        public Table SelectedTable { get; set; }

        public FullObservableCollection<Promotion> AvailablePromotions { get; set; }
        private Promotion _selectedPromotion;
        private double _rankDiscountPercentage;
        private int _promotionDiscountAmount;
        private int _rankDiscountAmount;

        public Promotion SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                _selectedPromotion = value;
                EditorAddOrder.PromotionId = _selectedPromotion?.Id ?? 0;
                Debug.WriteLine($"Selected Promotion: {_selectedPromotion?.Name}, ID: {EditorAddOrder.PromotionId}");
                OnPropertyChanged(nameof(SelectedPromotion));
                UpdateDiscountsAndTotal();
            }
        }

        public double RankDiscountPercentage
        {
            get => _rankDiscountPercentage;
            set
            {
                _rankDiscountPercentage = value;
                Debug.WriteLine($"RankDiscountPercentage: {_rankDiscountPercentage}%");
                OnPropertyChanged(nameof(RankDiscountPercentage));
                UpdateDiscountsAndTotal();
            }
        }

        public int PromotionDiscountAmount
        {
            get => _promotionDiscountAmount;
            private set
            {
                _promotionDiscountAmount = value;
                OnPropertyChanged(nameof(PromotionDiscountAmount));
            }
        }

        public int RankDiscountAmount
        {
            get => _rankDiscountAmount;
            private set
            {
                _rankDiscountAmount = value;
                OnPropertyChanged(nameof(RankDiscountAmount));
            }
        }

        public int TotalAfterDiscount
        {
            get => EditorAddOrder.TotalAfterDiscount;
            private set
            {
                EditorAddOrder.TotalAfterDiscount = value;
                OnPropertyChanged(nameof(TotalAfterDiscount));
                OnPropertyChanged(nameof(EditorAddOrder));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AddOrderViewModel()
        {
            var emptyTable = new Table { Id = -1, Name = "— Không chọn bàn —" };
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            //order
            Orders = new ObservableCollection<Order>(_dao.Orders.GetAll());
            EditorAddOrder = new Order
            {
                Details = new ObservableCollection<OrderDetail>()
            };
            EditorAddOrder.PropertyChanged += EditorAddOrder_PropertyChanged;
            //product
            Products = new ObservableCollection<Product>(_dao.Products.GetAll());
            ProductOptions = _dao.ProductOptions.GetAll();
            // customer
            Customers = new ObservableCollection<Customer>(_dao.Customers.GetAll());
            CustomerRanks = new ObservableCollection<CustomerRank>(_dao.CustomerRanks.GetAll());
            // table
            var tableList = _dao.Tables.GetAll()
            .Where(t => t.Status == TableStatuses.Empty)
            .ToList();
            tableList.Insert(0, emptyTable);
            AvailableTables = new ObservableCollection<Table>(tableList);
            SelectedTable = AvailableTables.FirstOrDefault(t => t.Id == -1);

            LoadAvailablePromotions();
        }

        private void EditorAddOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditorAddOrder.TotalPrice))
            {
                UpdateDiscountsAndTotal();
            }
        }

        private void LoadAvailablePromotions()
        {
            var allPromotions = _dao.Promotions.GetAll();
            var activePromotions = allPromotions
                .Where(p => p.IsActive)
                .ToList();
            AvailablePromotions = new FullObservableCollection<Promotion>(activePromotions);
            Debug.WriteLine($"Loaded {activePromotions.Count} active promotions");
        }

        public void UpdateCustomerInfo(int? customerId, string customerPhone, string customerName)
        {
            if (customerId != 0)
            {
                var customer = Customers.FirstOrDefault(c => c.Id == customerId);
                if (customer != null)
                {
                    EditorAddOrder.CustomerStatus = customer.Rank?.Name ?? "Không có";
                    RankDiscountPercentage = customer.Rank?.DiscountPercentage ?? 0;
                    Debug.WriteLine($"Customer: {customer.Name}, Rank: {EditorAddOrder.CustomerStatus}, Discount: {RankDiscountPercentage}%");
                }
                else
                {
                    EditorAddOrder.CustomerStatus = "Không có";
                    RankDiscountPercentage = 0;
                }
            }
            else
            {
                EditorAddOrder.CustomerStatus = "Vãng lai";
                RankDiscountPercentage = 0;
            }
            EditorAddOrder.CustomerId = customerId;
            EditorAddOrder.CustomerPhone = customerPhone;
            EditorAddOrder.CustomerName = customerName;
            OnPropertyChanged(nameof(EditorAddOrder));
            UpdateDiscountsAndTotal();
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

        private void UpdateDiscountsAndTotal()
        {


            // Tính giảm giá khuyến mãi
            if (SelectedPromotion != null && EditorAddOrder.TotalPrice >= SelectedPromotion.MinimumOrderValue)
            {
                if (SelectedPromotion.Type == PromotionType.Percentage)
                {
                    PromotionDiscountAmount = (int)(EditorAddOrder.TotalPrice * (SelectedPromotion.Amount / 100));
                }
                else
                {
                    PromotionDiscountAmount = (int)SelectedPromotion.Amount;
                }
                Debug.WriteLine($"PromotionDiscountAmount: {PromotionDiscountAmount} VNĐ (Promotion = {SelectedPromotion.Name}, Type = {SelectedPromotion.Type}, Amount = {SelectedPromotion.Amount})");
            }

            else
            {
                PromotionDiscountAmount = 0;
                Debug.WriteLine("No promotion applied");
            }
            // Tính giảm giá rank
            RankDiscountAmount = (int)((EditorAddOrder.TotalPrice - PromotionDiscountAmount) * (RankDiscountPercentage / 100));
            Debug.WriteLine($"RankDiscountAmount: {RankDiscountAmount} VNĐ (TotalPrice = {EditorAddOrder.TotalPrice}, Percentage = {RankDiscountPercentage}%)");

            // Tính TotalAfterDiscount
            int totalAfterDiscount = EditorAddOrder.TotalPrice - RankDiscountAmount - PromotionDiscountAmount;
            TotalAfterDiscount = Math.Max(0, totalAfterDiscount);
            Debug.WriteLine($"Final TotalAfterDiscount: {TotalAfterDiscount} (TotalPrice = {EditorAddOrder.TotalPrice}, RankDiscount = {RankDiscountAmount}, PromotionDiscount = {PromotionDiscountAmount})");
        }

        private void CalculateCostPrice(Order order)
        {
            decimal totalCostPrice = 0;

            foreach (var detail in order.Details)
            {
                // Lấy option nếu có, ưu tiên nếu đã chọn
                var option = ProductOptions.FirstOrDefault(opt => opt.OptionId == detail.OptionId);

                if (option != null)
                {
                    detail.CostPrice = option.CostPrice;
                }
                else
                {
                    var product = Products.FirstOrDefault(p => p.Id == detail.ProductId);
                    detail.CostPrice = product?.CostPrice ?? 0;
                }

                totalCostPrice += detail.CostPrice * detail.quantity;
            }

            order.TotalCostPrice = totalCostPrice;
        }

        public void Add()
        {
            EditorAddOrder.Date = DateTime.Now;
            EditorAddOrder.Status = "Đang xử lý";

            if (SelectedTable != null && SelectedTable.Id != -1)
            {

                SelectedTable.Status = TableStatuses.Occupied;
            }


            var customer = Customers.FirstOrDefault(c => c.Id == EditorAddOrder.CustomerId);
            if (customer != null)
            {
                customer.Point += (int)Math.Round((double)EditorAddOrder.TotalPrice / 1000);
                Debug.WriteLine($"Customer: {customer.Name} - {customer.Phone} - {customer.Point}");
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
            // cal cost price
            CalculateCostPrice(EditorAddOrder);
            // TotalAfterDiscount đã được tính trong UpdateDiscountsAndTotal
            int result = _dao.Orders.Insert(EditorAddOrder);
            if (result != -1)
            {
                EditorAddOrder.Id = _dao.Orders.GetAll().Max(o => o.Id);
                Orders.Add(EditorAddOrder);
                if (SelectedTable != null && SelectedTable.Id != -1)
                {
                    Debug.WriteLine($"SelectedTable: {SelectedTable.Name} - {SelectedTable.Id} - {SelectedTable.Status} - {EditorAddOrder.Id}");
                    SelectedTable.OrderId = EditorAddOrder.Id;
                    SelectedTable.Status = TableStatuses.Occupied;
                    _dao.Tables.UpdateById(SelectedTable.Id.ToString(), SelectedTable);
                }
            }
            else
            {
                Debug.WriteLine("Insert failed: No rows affected.");
            }
        }

        public ResponseCode addWithNewCustomer()
        {
            if (string.IsNullOrEmpty(EditorAddOrder.CustomerName) || string.IsNullOrEmpty(EditorAddOrder.CustomerPhone))
            {
                return ResponseCode.NotFound;
            }

            var customer = new Customer
            {
                Name = EditorAddOrder.CustomerName,
                Phone = EditorAddOrder.CustomerPhone,
                Point = 0,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Now),
                Rank = CustomerRanks.FirstOrDefault(r => r.Id == 1)
            };
            Debug.WriteLine($"Customer: {customer.Name} - {customer.Phone}");

            var existingCustomer = Customers.FirstOrDefault(c => c.Phone == customer.Phone);
            if (existingCustomer != null)
            {
                return ResponseCode.ExistsCustomer;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(customer.Phone, @"^0\d{9,10}$"))
            {
                return ResponseCode.InvalidValue;
            }

            int result = _dao.Customers.Insert(customer);
            if (result != 0)
            {
                customer.Id = _dao.Customers.GetAll().Max(c => c.Id);
                EditorAddOrder.CustomerId = customer.Id;
                Customers.Add(customer);
                UpdateCustomerInfo(customer.Id, customer.Phone, customer.Name);
                Debug.WriteLine($"Customer: {customer.Name} - {customer.Phone} - {customer.Id}");
                return ResponseCode.Success;
            }
            else
            {
                Debug.WriteLine("Insert failed: No rows affected.");
                return ResponseCode.Error;
            }
        }

        public Customer GetCustomerByPhone(string phone)
        {
            return Customers.FirstOrDefault(c => c.Phone == phone);
        }
        public void SelectTable(Table table)
        {
            if (table == null) return;

            SelectedTable = table;


            Debug.WriteLine($"[SelectTable] Bàn '{table.Name}' (ID: {table.Id}) được chọn, sẽ xử lý khi tạo đơn.");
        }
        public void UpdateTotalPriceFromDetails()
        {
            if (EditorAddOrder.Details != null)
            {
                EditorAddOrder.TotalPrice = EditorAddOrder.Details.Sum(d => d.Price);
                Debug.WriteLine($"[ViewModel] Updated TotalPrice = {EditorAddOrder.TotalPrice}");
                UpdateDiscountsAndTotal();
            }
        }

        public void UpdateProductOption(OrderDetail detail, ProductOption selectedOption)
        {
            if (selectedOption == null || detail == null) return;

            detail.OptionId = selectedOption.OptionId;
            detail.OptionName = selectedOption.Name;
            detail.ProductPrice = selectedOption.SalePrice;
            detail.Price = detail.ProductPrice * detail.quantity;

            UpdateTotalPriceFromDetails();
        }

        public void UpdateQuantity(OrderDetail detail, int quantity)
        {
            if (detail == null) return;

            detail.quantity = quantity;
            detail.Price = detail.ProductPrice * quantity;

            UpdateTotalPriceFromDetails();
        }

        public void RemoveOrderDetail(OrderDetail detail)
        {
            if (detail == null) return;

            EditorAddOrder.Details.Remove(detail);
            UpdateTotalPriceFromDetails();
        }

        public void AddOrderDetail(OrderDetail detail)
        {
            if (detail == null) return;

            EditorAddOrder.Details.Add(detail);
            UpdateTotalPriceFromDetails();
        }



        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}