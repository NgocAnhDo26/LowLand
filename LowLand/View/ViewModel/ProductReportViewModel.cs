using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LowLand.Model.Product;
using LowLand.Services;
using Microsoft.UI.Dispatching;

namespace LowLand.View.ViewModel
{
    public class ProductSale
    {
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
    }

    public class PredictedProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public double PredictedSales { get; set; }
    }

    public class ProductReportViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        private ForecastPredictionService _predictionService;
        private readonly DispatcherQueue _dispatcherQueue; // For UI thread updates

        public DateOnly? FromDateFilter { get; set; }
        public DateOnly? ToDateFilter { get; set; }

        public bool IsLoading { get; set; } = true;
        public bool IsPredictLoading { get; set; } = true;

        public ObservableCollection<PredictedProduct> TopPredictedProducts { get; set; }

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
            _predictionService = new ForecastPredictionService();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            InitializeDataAsync();
        }

        private async void InitializeDataAsync()
        {
            await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
            try
            {
                await Task.WhenAll(
                    GetProductSaleFiguresAsync(),
                    GetCategorySaleFiguresAsync(),
                    PredictTopProducts()
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialization error: {ex.Message}");
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
            }
        }

        public async Task RetrainModel()
        {
            await _dispatcherQueue.TryEnqueueAsync(() => IsPredictLoading = true);
            try
            {
                // Retrain the model
                await _predictionService.RetrainModelAsync();
                Debug.WriteLine("Model retrained successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Model retraining error: {ex.Message}");
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsPredictLoading = false);
            }
        }

        public async Task PredictTopProducts()
        {
            // Predict top 5 products  
            await _dispatcherQueue.TryEnqueueAsync(() => IsPredictLoading = true);
            try
            {
                var top5 = await _predictionService.MakePredictionAsync();
                TopPredictedProducts = new ObservableCollection<PredictedProduct>();

                // Display results  
                // Get product info from the database to display
                var products = _dao.Products.GetAll()
                    .Where(p => top5.Keys.Contains(p.Id))
                    .ToList();

                Debug.WriteLine("Top 5 Products for Next Week:");
                foreach (var kv in top5)
                {
                    TopPredictedProducts.Add(new PredictedProduct
                    {
                        ProductId = kv.Key,
                        ProductName = products.FirstOrDefault(p => p.Id == kv.Key)?.Name ?? "Unknown",
                        Image = products.FirstOrDefault(p => p.Id == kv.Key)?.Image ?? "product_default.jpg",
                        PredictedSales = kv.Value
                    });

                    Debug.WriteLine($"ProductID: {kv.Key}, Predicted Sales: {kv.Value:F2}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsPredictLoading = false);
            }

        }

        public async Task ApplyTimeRangeFilter()
        {
            if (FromDateFilter > ToDateFilter)
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc!");
            }

            await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
            try
            {
                await Task.WhenAll(
                    GetProductSaleFiguresAsync(),
                    GetCategorySaleFiguresAsync()
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Filter application error: {ex.Message}");
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
            }
        }

        private async Task GetProductSaleFiguresAsync()
        {
            await Task.Run(async () =>
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
                try
                {
                    var startDate = FromDateFilter ?? DateOnly.MinValue;
                    var endDate = ToDateFilter ?? DateOnly.MaxValue;

                    var orders = _dao.Orders.GetAll()
                        .Where(order => DateOnly.FromDateTime(order.Date) >= startDate
                            && DateOnly.FromDateTime(order.Date) <= endDate)
                        .ToList();

                    var orderDetails = _dao.OrderDetails.GetAll()
                        .Where(orderDetail => orders.Any(order => order.Id == orderDetail.OrderId))
                        .ToList();

                    var allProducts = _dao.Products.GetAll().ToList();

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
                        .ToList();

                    var topProductSales = allProductSales
                        .OrderByDescending(ps => ps.QuantitySold)
                        .Take(6)
                        .ToList();

                    await _dispatcherQueue.TryEnqueueAsync(() =>
                    {
                        ProductSaleColumnSeries = new ISeries[]
                        {
                            new ColumnSeries<int>
                            {
                                Values = new ObservableCollection<int>(topProductSales.Select(ps => ps.QuantitySold)),
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
                                Values = new ObservableCollection<int>(allProductSales.Select(ps => ps.QuantitySold)),
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
                    });
                }
                finally
                {
                    await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
                }
            });
        }

        private async Task GetCategorySaleFiguresAsync()
        {
            await Task.Run(async () =>
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
                try
                {
                    var startDate = FromDateFilter ?? DateOnly.MinValue;
                    var endDate = ToDateFilter ?? DateOnly.MaxValue;

                    var products = _dao.Products.GetAll()
                        .Where(product => product is SingleProduct)
                        .Select(product => (SingleProduct)product)
                        .ToList();

                    var orders = _dao.Orders.GetAll()
                        .Where(order => DateOnly.FromDateTime(order.Date) >= startDate
                            && DateOnly.FromDateTime(order.Date) <= endDate)
                        .ToList();

                    var orderDetails = _dao.OrderDetails.GetAll()
                        .Where(orderDetail => orders.Any(order => order.Id == orderDetail.OrderId))
                        .ToList();

                    var categories = _dao.Categories.GetAll().ToList();

                    var productCategories = products
                        .Where(product => product.Category != null)
                        .GroupBy(product => product.Category.Id)
                        .Select(g => new
                        {
                            Category = categories.FirstOrDefault(c => c.Id == g.Key),
                            Products = g.ToList()
                        })
                        .Where(g => g.Category != null)
                        .ToList();

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

                    await _dispatcherQueue.TryEnqueueAsync(() =>
                    {
                        CategorySaleColumnSeries = new ISeries[]
                        {
                            new ColumnSeries<int>
                            {
                                Values = new ObservableCollection<int>(categorySales.Select(cs => cs.Sales)),
                                Name = "Doanh số"
                            }
                        };

                        CategorySaleXAxes = new Axis[]
                        {
                            new Axis
                            {
                                Name = "Tên danh mục",
                                NameTextSize = 13,
                                Labels = categorySales.Select(cs => cs.Category!.Name).ToArray()
                            }
                        };

                        CategorySalePieSeries = categorySales
                            .Where(sale => sale.Sales > 0)
                            .Select(sale => new PieSeries<int>
                            {
                                Values = new[] { sale.Sales },
                                Name = sale.Category!.Name,
                            })
                            .ToArray();
                    });
                }
                finally
                {
                    await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}