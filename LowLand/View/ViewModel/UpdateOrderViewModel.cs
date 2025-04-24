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
    public class UpdateOrderViewModel : INotifyPropertyChanged
    {
        private readonly IDao _dao;
        public ObservableCollection<Order> Orders { get; set; }

        public Order EditorAddOrder { get; set; }
        public ObservableCollection<Customer> Customers { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public List<ProductOption> ProductOptions { get; set; }
        public ObservableCollection<CustomerRank> CustomerRanks { get; set; }
        public FullObservableCollection<Promotion> AvailablePromotions { get; set; }
        public ObservableCollection<Table> AvailableTables { get; set; }
        public Table SelectedTable { get; set; }
        public List<Category> SelectedCategories { get; set; } = new List<Category>();
        private List<Table> _allTables;

        private Promotion _selectedPromotion;
        private double _rankDiscountPercentage;
        private int _promotionDiscountAmount;
        private int _rankDiscountAmount;
        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<Product> FilteredProducts { get; set; } = new ObservableCollection<Product>();
        public string SearchKeyword { get; set; } = string.Empty;
        public Category SelectedCategory { get; set; }
        public Promotion SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                _selectedPromotion = value;
                EditorAddOrder.PromotionId = _selectedPromotion?.Id ?? 0;
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

        public UpdateOrderViewModel()
        {
            var emptyTable = new Table { Id = -1, Name = "— Không chọn bàn —" };
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Orders = new ObservableCollection<Order>(_dao.Orders.GetAll());
            EditorAddOrder = new Order { Details = new ObservableCollection<OrderDetail>() };
            EditorAddOrder.PropertyChanged += EditorAddOrder_PropertyChanged;
            Products = new ObservableCollection<Product>(_dao.Products.GetAll());
            ProductOptions = _dao.ProductOptions.GetAll();
            Customers = new ObservableCollection<Customer>(_dao.Customers.GetAll());
            CustomerRanks = new ObservableCollection<CustomerRank>(_dao.CustomerRanks.GetAll());
            var tableList = _dao.Tables.GetAll()
            .Where(t => t.Status == TableStatuses.Empty)
            .ToList();
            tableList.Insert(0, emptyTable);
            AvailableTables = new ObservableCollection<Table>(tableList);
            //      SelectedTable = AvailableTables.FirstOrDefault(t => t.Id == -1) ?? new Table { Id = -1, Name = "— Không chọn bàn —" };
            Categories = new ObservableCollection<Category>(_dao.Categories.GetAll());
            Categories.Add(new Category { Id = -1, Name = "Combo" }); // Danh mục cho Combo

            SelectedCategory = null;
            ApplyProductFilter();
            LoadAvailablePromotions();
        }
        public void Init(Order order)
        {
            EditorAddOrder = order;
            EditorAddOrder.PropertyChanged += EditorAddOrder_PropertyChanged;




            _allTables = _dao.Tables.GetAll(); // <- Chỉ lấy 1 lần
            var currentTable = _allTables.FirstOrDefault(t => t.OrderId is int id && id == order.Id);

            var tableList = _allTables
                .Where(t => t.Status == TableStatuses.Empty || (currentTable != null && t.Id == currentTable.Id))
                .ToList();

            var emptyTable = new Table { Id = -1, Name = "— Không chọn bàn —" };
            tableList.Insert(0, emptyTable);

            AvailableTables = new ObservableCollection<Table>(tableList);
            SelectedTable = currentTable ?? emptyTable;




            AddOptionToOrderDetail(EditorAddOrder);

            UpdateCustomerInfo(order.CustomerId, order.CustomerPhone, order.CustomerName);


            SelectedPromotion = AvailablePromotions.FirstOrDefault(p => p.Id == order.PromotionId);

            UpdateDiscountsAndTotal();
        }
        public void ApplyProductFilter()
        {
            var filtered = Products.Where(p =>
                (string.IsNullOrEmpty(SearchKeyword) || p.Name.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)) &&
                (
                    SelectedCategories == null || SelectedCategories.Count == 0 ||
                    (p is SingleProduct sp && SelectedCategories.Any(c => sp.Category?.Id == c.Id)) ||
                    (p is ComboProduct && SelectedCategories.Any(c => c.Name == "Combo"))
                )).ToList();

            FilteredProducts.Clear();
            foreach (var product in filtered)
                FilteredProducts.Add(product);
        }


        private void EditorAddOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditorAddOrder.TotalPrice))
                UpdateDiscountsAndTotal();
        }
        public void SelectTable(Table table)
        {
            if (table == null) return;

            SelectedTable = table;


            Debug.WriteLine($"[SelectTable] Bàn '{table.Name}' (ID: {table.Id}) được chọn, sẽ xử lý khi tạo đơn.");
        }
        private void LoadAvailablePromotions()
        {
            var allPromotions = _dao.Promotions.GetAll();
            var activePromotions = allPromotions.Where(p => p.IsActive).ToList();
            AvailablePromotions = new FullObservableCollection<Promotion>(activePromotions);
        }

        public void UpdateCustomerInfo(int? customerId, string phone, string name)
        {
            if (customerId != null)
            {
                var customer = Customers.FirstOrDefault(c => c.Id == customerId);
                if (customer != null)
                {
                    EditorAddOrder.CustomerStatus = customer.Rank?.Name ?? "Không có";
                    RankDiscountPercentage = customer.Rank?.DiscountPercentage ?? 0;
                }
            }
            else
            {
                EditorAddOrder.CustomerStatus = "Vãng lai";
                RankDiscountPercentage = 0;
            }

            EditorAddOrder.CustomerId = customerId;
            EditorAddOrder.CustomerPhone = phone;
            EditorAddOrder.CustomerName = name;
            Debug.WriteLine($"---------------CustomerId: {customerId}, Phone: {phone}, Name: {name}");
            OnPropertyChanged(nameof(EditorAddOrder));
            UpdateDiscountsAndTotal();
        }

        public OrderDetail CreateOrderDetail(Product selectedProduct)
        {
            if (selectedProduct == null) return null;

            return new OrderDetail
            {
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                ProductPrice = selectedProduct.SalePrice,
                Price = selectedProduct.SalePrice,
                quantity = 1,
                ProductOptions = new ObservableCollection<ProductOption>(
                    ProductOptions.Where(po => po.ProductId == selectedProduct.Id)
                )
            };
        }

        public void AddOptionToOrderDetail(Order order)
        {
            if (order.Details == null) return;
            foreach (var detail in order.Details)
            {
                detail.ProductOptions = new ObservableCollection<ProductOption>(
                    ProductOptions.Where(po => po.ProductId == detail.ProductId)
                );
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

        //----------






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

        // cal cost price
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



        public void Update(Order item)
        {
            if (item.CustomerId == 0)
                throw new Exception("CustomerId không được để trống khi cập nhật!");

            item.Date = DateTime.Now;
            item.Status = "Đang xử lý";

            var oldTable = _allTables?.FirstOrDefault(t => t.OrderId is int id && id == item.Id);

            if (oldTable != null && (SelectedTable == null || oldTable.Id != SelectedTable.Id))
            {
                oldTable.Status = TableStatuses.Empty;
                oldTable.OrderId = null;
                _dao.Tables.UpdateById(oldTable.Id.ToString(), oldTable);
                Debug.WriteLine($"[Update] Giải phóng bàn cũ: {oldTable.Name}");
            }


            if (SelectedTable != null && SelectedTable.Id != -1)
            {
                SelectedTable.Status = TableStatuses.Occupied;
                SelectedTable.OrderId = item.Id;
            }

            var orderToUpdate = Orders.FirstOrDefault(o => o.Id == item.Id);
            if (orderToUpdate != null)
            {
                // Tính toán lại giá vốn
                CalculateCostPrice(item);

                int result = _dao.Orders.UpdateById(item.Id.ToString(), item);
                if (result == 1)
                {
                    int index = Orders.IndexOf(orderToUpdate);
                    if (index != -1)
                        Orders[index] = item;

                    if (SelectedTable != null && SelectedTable.Id != -1)
                    {
                        _dao.Tables.UpdateById(SelectedTable.Id.ToString(), SelectedTable);
                    }
                }
                else
                {
                    Debug.WriteLine("Update thất bại!");
                }
            }
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
