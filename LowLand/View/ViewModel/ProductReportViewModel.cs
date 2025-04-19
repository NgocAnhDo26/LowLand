using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LowLand.Model.Product;
using LowLand.Services;
using PropertyChanged;

namespace LowLand.View.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class ProductReportViewModel : INotifyPropertyChanged
    {
        class ProductSale
        {
            public int? ProductId { get; set; }
            public string ProductName { get; set; }
            public int QuantitySold { get; set; }
        }

        private IDao _dao;

        public DateOnly? FromDateFilter { get; set; }
        public DateOnly? ToDateFilter { get; set; }

        public ISeries[] AllProductSaleColumnSeries { get; set; }
        public ISeries[] AllProductSalePieSeries { get; set; }
        public ICartesianAxis[] AllProductSaleXAxes { get; set; }

        public ISeries[] ProductSaleColumnSeries { get; set; }
        public ISeries[] ProductSalePieSeries { get; set; }
        public ICartesianAxis[] ProductSaleXAxes { get; set; }

        public ISeries[] CategorySaleColumnSeries { get; set; }
        public ISeries[] CategorySalePieSeries { get; set; }
        public ICartesianAxis[] CategorySaleXAxes { get; set; }

        public ProductReportViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            GetProductSaleFigures();
            GetCategorySaleFigures();
        }

        public void ApplyTimeRangeFilter()
        {
            // Validate the date range
            if (FromDateFilter > ToDateFilter)
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc!");
            }

            // Recalculate the sales figures based on the new date range
            GetProductSaleFigures();
            GetCategorySaleFigures();
        }

        private void GetProductSaleFigures()
        {
            var startDate = FromDateFilter ?? DateOnly.MinValue;
            var endDate = ToDateFilter ?? DateOnly.MaxValue;

            var orders = _dao.Orders.GetAll()
                .Where(order => DateOnly.FromDateTime(order.Date) >= startDate
                && DateOnly.FromDateTime(order.Date) <= endDate);

            var orderDetails = _dao.OrderDetails.GetAll()
                .Where(orderDetail => orders.Any(order => order.Id == orderDetail.OrderId))
                .ToList();

            var allProducts = _dao.Products.GetAll();

            var allProductSales = allProducts
                .GroupJoin(orderDetails,
                    product => product.Id,
                    orderDetail => orderDetail.ProductId,
                    (product, details) => new ProductSale
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        QuantitySold = details.Sum(od => od.quantity)
                    })
                .OrderByDescending(ps => ps.QuantitySold)
                .ToList();

            var topProductSales = allProductSales
                .Take(6)
                .ToList();

            ProductSaleColumnSeries = new ISeries[]
            {
               new ColumnSeries<int>
               {
                   Values = new ObservableCollection<int>(topProductSales.Select(ps => ps.QuantitySold).ToArray()),
                   Name = "Doanh số"
               }
            };

            ProductSaleXAxes = new Axis[]
            {
               new Axis
               {
                   Name = "Tên sản phẩm",
                   NameTextSize = 13,
                   Labels = topProductSales.Select(ps => ps.ProductName).ToArray()
               }
            };

            ProductSalePieSeries = topProductSales
               .Where(sale => sale.QuantitySold > 0)
               .Select(sale => new PieSeries<int>
               {
                   Values = new[] { sale.QuantitySold },
                   Name = sale.ProductName,
               })
               .ToArray();

            AllProductSaleColumnSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = new ObservableCollection<int>(allProductSales.Select(ps => ps.QuantitySold).ToArray()),
                    Name = "Doanh số"
                }
            };

            AllProductSaleXAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Tên sản phẩm",
                    NameTextSize = 13,
                    Labels = allProductSales.Select(ps => ps.ProductName).ToArray()
                }
            };

            AllProductSalePieSeries = allProductSales
                .Where(sale => sale.QuantitySold > 0)
                .Select(sale => new PieSeries<int>
                {
                    Values = new[] { sale.QuantitySold },
                    Name = sale.ProductName,
                })
                .ToArray();
        }

        private void GetCategorySaleFigures()
        {
            var startDate = FromDateFilter ?? DateOnly.MinValue;
            var endDate = ToDateFilter ?? DateOnly.MaxValue;

            // Get all single products
            var products = _dao.Products.GetAll()
                .Where(product => product is SingleProduct)
                .Select(product => (product as SingleProduct)!)
                .ToList();

            // Get all orders
            var orders = _dao.Orders.GetAll()
                .Where(order => DateOnly.FromDateTime(order.Date) >= startDate
                    && DateOnly.FromDateTime(order.Date) <= endDate);

            // Get all order details
            var orderDetails = _dao.OrderDetails.GetAll()
                .Where(orderDetail => orders.Any(order => order.Id == orderDetail.OrderId))
                .ToList();

            // Get all categories
            var categories = _dao.Categories.GetAll()
                .ToList();

            // Ensure products are grouped by unique categories
            var productCategories = products
                .Where(product => product.Category != null) // Exclude products without a category
                .GroupBy(product => product.Category.Id) // Group by unique category ID
                .Select(g => new
                {
                    Category = categories.FirstOrDefault(c => c.Id == g.Key), // Match category by ID
                    Products = g.ToList()
                })
                .Where(g => g.Category != null) // Exclude groups with null categories
                .ToList();

            // Get all categories and their sales
            var categorySales = productCategories
                .Select(g => new
                {
                    Category = g.Category,
                    Sales = g.Products
                        .SelectMany(product => orderDetails
                            .Where(od => od.ProductId == product.Id)
                            .Select(od => od.quantity))
                        .Sum()
                })
                .OrderByDescending(cs => cs.Sales)
                .Take(5)
                .ToList();

            CategorySaleColumnSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = new ObservableCollection<int>(categorySales.Select(cs => cs.Sales).ToArray()),
                    Name = "Doanh số"
                }
            };

            CategorySaleXAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Tên danh mục",
                    NameTextSize = 13,
                    Labels = categorySales.Select(cs => cs.Category!.Name).ToArray() // Ensure non-null category names
                }
            };

            CategorySalePieSeries = categorySales
                .Where(sale => sale.Sales > 0)
                .Select(sale => new PieSeries<int>
                {
                    Values = new[] { sale.Sales },
                    Name = sale.Category!.Name, // Ensure non-null category names
                })
                .ToArray();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}