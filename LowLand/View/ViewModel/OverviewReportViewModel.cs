using System;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LowLand.Services;
using SkiaSharp;

namespace LowLand.View.ViewModel
{
    public class OverviewReportViewModel
    {
        private IDao _dao;
        public double TodayRevenue { get; set; }
        public double RevenueComparisonPercentage { get; set; } = 0;
        public double TodayTotalOrder { get; set; }
        public double TotalOrderComparisonPercentage { get; set; } = 0;
        public double TodayAverageOrderValue { get; set; }
        public double AverageOrderValueComparisonPercentage { get; set; } = 0;
        public ISeries[] RevenueSeries { get; set; } = Array.Empty<ISeries>();
        public ICartesianAxis[] RevenueXAxes { get; set; } = Array.Empty<Axis>();
        public ICartesianAxis[] RevenueYAxes { get; set; } = Array.Empty<Axis>();
        public ISeries[] TotalOrderSeries { get; set; } = Array.Empty<ISeries>();
        public ICartesianAxis[] TotalOrderXAxes { get; set; } = Array.Empty<Axis>();
        public ICartesianAxis[] TotalOrderYAxes { get; set; } = new Axis[] { new Axis { MinLimit = 0 } };

        public OverviewReportViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            var todayOrders = _dao.Orders.GetAll()
                .Where(o => o.Date == DateTime.Now.Date)
                .ToList();

            var yesterdayOrders = _dao.Orders.GetAll()
                .Where(o => o.Date == DateTime.Now.Date.AddDays(-1))
                .ToList();

            TodayRevenue = todayOrders.Sum(o => o.TotalAfterDiscount);
            TodayTotalOrder = todayOrders.Count;
            TodayAverageOrderValue = TodayTotalOrder == 0 ? 0 : TodayRevenue / TodayTotalOrder;

            if (yesterdayOrders.Count > 0)
            {
                var yesterdayRevenue = yesterdayOrders.Sum(o => o.TotalAfterDiscount);
                var amount = TodayRevenue - yesterdayRevenue;
                RevenueComparisonPercentage = (amount / yesterdayRevenue) * 100;

                var yesterdayTotalOrder = yesterdayOrders.Count;
                var totalOrderAmount = TodayTotalOrder - yesterdayTotalOrder;
                TotalOrderComparisonPercentage = (totalOrderAmount / yesterdayTotalOrder) * 100;

                var averageOrderValue = yesterdayTotalOrder == 0 ? 0 : yesterdayRevenue / yesterdayTotalOrder;
                var averageOrderValueAmount = TodayAverageOrderValue - averageOrderValue;
                AverageOrderValueComparisonPercentage = (averageOrderValueAmount / averageOrderValue) * 100;
            }
            else
            {
                RevenueComparisonPercentage = 100;
                TotalOrderComparisonPercentage = 100;
                AverageOrderValueComparisonPercentage = 100;
            }

            InitializeReports();
        }

        private void InitializeReports()
        {
            // Date lables of recent 7 days (dd/MM/YYYY)
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

            CalculateRecentRevenue();
            CalculateRecentTotalOrders();
        }

        private void CalculateRecentRevenue()
        {
            var colors = new[]
            {
                new SKColor(45, 64, 89),
                new SKColor(255, 212, 96)
            };

            // Get the last 7 days of orders
            var last7DaysOrders = _dao.Orders.GetAll()
                .Where(o => o.Date >= DateTime.Now.Date.AddDays(-6) && o.Date <= DateTime.Now.Date)
                .ToList();

            // Calculate the revenue for each day
            var dailyRevenue = new double[7];
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                dailyRevenue[i] = last7DaysOrders
                    .Where(o => o.Date == date)
                    .Sum(o => o.TotalAfterDiscount);
            }

            // Calculate the profit for each day
            var dailyProfit = new double[7];
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                dailyProfit[i] = last7DaysOrders
                    .Where(o => o.Date == date)
                    .Sum(o => o.TotalAfterDiscount - o.TotalPrice);
            }

            // Reverse the array to match the order of the days
            Array.Reverse(dailyRevenue);

            // Create the series for the chart
            RevenueSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = dailyRevenue,
                    Stroke = null,
                    Name = "Doanh thu",
                    Fill = new LinearGradientPaint(
                        // the gradient will use the following colors array
                        new[] { new SKColor(84, 247, 201), new SKColor(27, 136, 217) },
                        // we must go from the point:
                        // (x0, y0) where x0 could be read as "the middle of the x axis" (0.5) and y0 as "the start of the y axis" (0)
                        new SKPoint(0.5f, 0),
                        // to the point:
                        // (x1, y1) where x1 could be read as "the middle of the x axis" (0.5) and y0 as "the end of the y axis" (1)
                        new SKPoint(0.5f, 1))
                },
                new LineSeries<double>
                {
                    Values = dailyProfit,
                    GeometrySize = 16,
                    Name="Lợi nhuận",
                    Stroke = new LinearGradientPaint(colors) { StrokeThickness = 4 },
                    GeometryStroke = new LinearGradientPaint(colors) { StrokeThickness = 4 },
                    Fill = null
                },
            };
        }

        private void CalculateRecentTotalOrders()
        {
            // Get the last 7 days of orders
            var last7DaysOrders = _dao.Orders.GetAll()
                .Where(o => o.Date >= DateTime.Now.Date.AddDays(-6) && o.Date <= DateTime.Now.Date)
                .ToList();

            // Calculate the total orders for each day
            var dailyTotalOrders = new int[7];
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Now.Date.AddDays(-i);
                dailyTotalOrders[i] = last7DaysOrders
                    .Where(o => o.Date == date)
                    .Count();
            }

            // Reverse the array to match the order of the days
            Array.Reverse(dailyTotalOrders);

            // Create the series for the chart
            TotalOrderSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = dailyTotalOrders,
                    Stroke = null,
                    Name = "Tổng đơn hàng",
                    Fill = new LinearGradientPaint(
                        // the gradient will use the following colors array
                        new[] { new SKColor(84, 247, 201), new SKColor(27, 136, 217) },
                        // we must go from the point:
                        // (x0, y0) where x0 could be read as "the middle of the x axis" (0.5) and y0 as "the start of the y axis" (0)
                        new SKPoint(0.5f, 0),
                        // to the point:
                        // (x1, y1) where x1 could be read as "the middle of the x axis" (0.5) and y0 as "the end of the y axis" (1)
                        new SKPoint(0.5f, 1))
                }
            };
        }
    }
}
