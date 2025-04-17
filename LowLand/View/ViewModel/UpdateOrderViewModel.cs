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
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Orders = new ObservableCollection<Order>(_dao.Orders.GetAll());
            EditorAddOrder = new Order { Details = new ObservableCollection<OrderDetail>() };
            EditorAddOrder.PropertyChanged += EditorAddOrder_PropertyChanged;
            Products = new ObservableCollection<Product>(_dao.Products.GetAll());
            ProductOptions = _dao.ProductOptions.GetAll();
            Customers = new ObservableCollection<Customer>(_dao.Customers.GetAll());
            CustomerRanks = new ObservableCollection<CustomerRank>(_dao.CustomerRanks.GetAll());
            LoadAvailablePromotions();
        }
        public void Init(Order order)
        {
            EditorAddOrder = order;


            EditorAddOrder.PropertyChanged += EditorAddOrder_PropertyChanged;


            AddOptionToOrderDetail(EditorAddOrder);


            UpdateCustomerInfo(order.CustomerId ?? 0, order.CustomerPhone, order.CustomerName);

            SelectedPromotion = AvailablePromotions.FirstOrDefault(p => p.Id == order.PromotionId);


            UpdateDiscountsAndTotal();
        }

        private void EditorAddOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditorAddOrder.TotalPrice))
                UpdateDiscountsAndTotal();
        }

        private void LoadAvailablePromotions()
        {
            var allPromotions = _dao.Promotions.GetAll();
            var activePromotions = allPromotions.Where(p => p.IsActive).ToList();
            AvailablePromotions = new FullObservableCollection<Promotion>(activePromotions);
        }

        public void UpdateCustomerInfo(int customerId, string phone, string name)
        {
            if (customerId != 0)
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
                }
                else
                {
                    Debug.WriteLine("Update thất bại!");
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
