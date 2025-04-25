using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LowLand.Services;
using Microsoft.UI.Dispatching;
using SkiaSharp;

namespace LowLand.View.ViewModel
{
    public class OverviewReportViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        private readonly DispatcherQueue _dispatcherQueue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsLoading { get; set; } = true;
        public double TodayRevenue { get; set; }
        public double RevenueComparisonPercentage { get; set; } = 0;
        public double TodayTotalOrder { get; set; }
        public double TotalOrderComparisonPercentage { get; set; } = 0;
        public double TodayAverageOrderValue { get; set; }
        public double AverageOrderValueComparisonPercentage { get; set; } = 0;
        public ISeries[] RevenueSeries { get; set; }
        public ICartesianAxis[] RevenueXAxes { get; set; }
        public ICartesianAxis[] RevenueYAxes { get; set; }
        public ISeries[] TotalOrderSeries { get; set; }
        public ICartesianAxis[] TotalOrderXAxes { get; set; }
        public ICartesianAxis[] TotalOrderYAxes { get; set; }

        public OverviewReportViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Initialize axes with placeholder values to satisfy LiveCharts requirements
            var placeholderLabels = new[] { "Loading..." };
            RevenueSeries = Array.Empty<ISeries>();
            RevenueXAxes = new Axis[] { new Axis { Labels = placeholderLabels, TextSize = 14 } };
            RevenueYAxes = new Axis[] { new Axis { Labeler = Labelers.Currency, MinLimit = 0 } };
            TotalOrderSeries = Array.Empty<ISeries>();
            TotalOrderXAxes = new Axis[] { new Axis { Labels = placeholderLabels, TextSize = 14 } };
            TotalOrderYAxes = new Axis[] { new Axis { MinLimit = 0 } };

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
            try
            {
                await Task.Run(async () =>
                {
                    // Fetch orders for today, yesterday, and last 7 days in a single query
                    var startDate = DateTime.Now.Date.AddDays(-6);
                    var endDate = DateTime.Now.Date;
                    var orders = _dao.Orders.GetAll()
                        .Where(o => o.Date >= startDate && o.Date <= endDate)
                        .ToList();

                    var todayOrders = orders.Where(o => o.Date == DateTime.Now.Date).ToList();
                    var yesterdayOrders = orders.Where(o => o.Date == DateTime.Now.Date.AddDays(-1)).ToList();

                    double todayRevenue = todayOrders.Sum(o => o.TotalAfterDiscount);
                    double todayTotalOrder = todayOrders.Count;
                    double todayAverageOrderValue = todayTotalOrder == 0 ? 0 : todayRevenue / todayTotalOrder;

                    double revenueComparisonPercentage = 0;
                    double totalOrderComparisonPercentage = 0;
                    double averageOrderValueComparisonPercentage = 0;

                    if (yesterdayOrders.Count > 0)
                    {
                        var yesterdayRevenue = yesterdayOrders.Sum(o => o.TotalAfterDiscount);
                        var amount = todayRevenue - yesterdayRevenue;
                        revenueComparisonPercentage = yesterdayRevenue == 0 ? 100 : (amount / yesterdayRevenue) * 100;

                        var yesterdayTotalOrder = yesterdayOrders.Count;
                        var totalOrderAmount = todayTotalOrder - yesterdayTotalOrder;
                        totalOrderComparisonPercentage = yesterdayTotalOrder == 0 ? 100 : (totalOrderAmount / yesterdayTotalOrder) * 100;

                        var yesterdayAverageOrderValue = yesterdayTotalOrder == 0 ? 0 : yesterdayRevenue / yesterdayTotalOrder;
                        var averageOrderValueAmount = todayAverageOrderValue - yesterdayAverageOrderValue;
                        averageOrderValueComparisonPercentage = yesterdayAverageOrderValue == 0 ? 100 : (averageOrderValueAmount / yesterdayAverageOrderValue) * 100;
                    }
                    else
                    {
                        revenueComparisonPercentage = 100;
                        totalOrderComparisonPercentage = 100;
                        averageOrderValueComparisonPercentage = 100;
                    }

                    await _dispatcherQueue.TryEnqueueAsync(() =>
                    {
                        TodayRevenue = todayRevenue;
                        RevenueComparisonPercentage = revenueComparisonPercentage;
                        TodayTotalOrder = todayTotalOrder;
                        TotalOrderComparisonPercentage = totalOrderComparisonPercentage;
                        TodayAverageOrderValue = todayAverageOrderValue;
                        AverageOrderValueComparisonPercentage = averageOrderValueComparisonPercentage;
                    });

                    await InitializeReportsAsync(orders);
                });
            }
            catch (Exception ex)
            {
                await _dispatcherQueue.TryEnqueueAsync(() => Debug.WriteLine($"Error loading overview data: {ex.Message}"));
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
            }
        }

        private async Task InitializeReportsAsync(System.Collections.Generic.List<LowLand.Model.Order.Order> orders)
        {
            // Date labels of recent 7 days (dd/MM)
            var dateAxes = new[]
            {
                DateTime.Now.Date.AddDays(-6).ToString("dd/MM"),
                DateTime.Now.Date.AddDays(-5).ToString("dd/MM"),
                DateTime.Now.Date.AddDays(-4).ToString("dd/MM"),
                DateTime.Now.Date.AddDays(-3).ToString("dd/MM"),
                DateTime.Now.Date.AddDays(-2).ToString("dd/MM"),
                DateTime.Now.Date.AddDays(-1).ToString("dd/MM"),
                DateTime.Now.Date.ToString("dd/MM")
            };

            await _dispatcherQueue.TryEnqueueAsync(() =>
            {
                RevenueXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = dateAxes,
                        TextSize = 14,
                    }
                };

                RevenueYAxes = new Axis[]
                {
                    new Axis
                    {
                        Labeler = Labelers.Currency,
                        MinLimit = 0,
                    }
                };

                TotalOrderXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = dateAxes,
                        TextSize = 14,
                    }
                };
            });

            await Task.WhenAll(
                CalculateRecentRevenueAsync(orders),
                CalculateRecentTotalOrdersAsync(orders)
            );
        }

        private async Task CalculateRecentRevenueAsync(System.Collections.Generic.List<LowLand.Model.Order.Order> orders)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var colors = new[]
                    {
                        new SKColor(45, 64, 89),
                        new SKColor(255, 212, 96)
                    };

                    // Calculate the revenue and profit for each day
                    var dailyRevenue = new double[7];
                    var dailyProfit = new double[7];
                    for (int i = 0; i < 7; i++)
                    {
                        var date = DateTime.Now.Date.AddDays(-i);
                        dailyRevenue[i] = orders
                            .Where(o => o.Date == date)
                            .Sum(o => o.TotalAfterDiscount);
                        dailyProfit[i] = (double)orders
                            .Where(o => o.Date == date)
                            .Sum(o => o.TotalAfterDiscount - o.TotalCostPrice);
                    }

                    // Reverse the arrays to match the order of the days
                    Array.Reverse(dailyRevenue);
                    Array.Reverse(dailyProfit);

                    await _dispatcherQueue.TryEnqueueAsync(() =>
                    {
                        RevenueSeries = new ISeries[]
                        {
                            new ColumnSeries<double>
                            {
                                Values = dailyRevenue,
                                Stroke = null,
                                Name = "Doanh thu",
                                Fill = new LinearGradientPaint(
                                    new[] { new SKColor(84, 247, 201), new SKColor(27, 136, 217) },
                                    new SKPoint(0.5f, 0),
                                    new SKPoint(0.5f, 1))
                            },
                            new LineSeries<double>
                            {
                                Values = dailyProfit,
                                GeometrySize = 16,
                                Name = "Lợi nhuận",
                                Stroke = new LinearGradientPaint(colors) { StrokeThickness = 4 },
                                GeometryStroke = new LinearGradientPaint(colors) { StrokeThickness = 4 },
                                Fill = null
                            }
                        };
                    });
                }
                catch (Exception ex)
                {
                    await _dispatcherQueue.TryEnqueueAsync(() => Debug.WriteLine($"Error calculating recent revenue: {ex.Message}\n{ex.StackTrace}"));
                }
            });
        }

        private async Task CalculateRecentTotalOrdersAsync(System.Collections.Generic.List<LowLand.Model.Order.Order> orders)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Calculate the total orders for each day
                    var dailyTotalOrders = new int[7];
                    for (int i = 0; i < 7; i++)
                    {
                        var date = DateTime.Now.Date.AddDays(-i);
                        dailyTotalOrders[i] = orders
                            .Where(o => o.Date == date)
                            .Count();
                    }

                    // Reverse the array to match the order of the days
                    Array.Reverse(dailyTotalOrders);

                    await _dispatcherQueue.TryEnqueueAsync(() =>
                    {
                        TotalOrderSeries = new ISeries[]
                        {
                            new ColumnSeries<int>
                            {
                                Values = dailyTotalOrders,
                                Stroke = null,
                                Name = "Tổng đơn hàng",
                                Fill = new LinearGradientPaint(
                                    new[] { new SKColor(84, 247, 201), new SKColor(27, 136, 217) },
                                    new SKPoint(0.5f, 0),
                                    new SKPoint(0.5f, 1))
                            }
                        };
                    });
                }
                catch (Exception ex)
                {
                    await _dispatcherQueue.TryEnqueueAsync(() => Debug.WriteLine($"Error calculating recent total orders: {ex.Message}\n{ex.StackTrace}"));
                }
            });
        }
    }
}