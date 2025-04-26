using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LowLand.Model.Order;
using LowLand.Services;
using Microsoft.UI.Dispatching;

namespace LowLand.View.ViewModel
{
    public partial class RevenueReportViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        private readonly DispatcherQueue _dispatcherQueue;
        private List<Order> _allOrders = [];

        public bool IsLoading { get; set; } = true;

        public List<string> TimePeriods { get; } = ["Ngày", "Tuần", "Tháng", "Quý", "Năm"];

        // Biểu đồ Tổng Doanh thu và Lợi nhuận
        public string SelectedTotalTimePeriod { get; set; } = "Ngày";
        public ObservableCollection<ISeries> TotalRevenueProfitSeries { get; set; } = [];
        public ObservableCollection<Axis> TotalRevenueProfitXAxes { get; set; } = [];
        public ObservableCollection<Axis> TotalRevenueProfitYAxes { get; set; } = [];

        // Biểu đồ Doanh thu và Lợi nhuận Trung bình
        public string SelectedAverageTimePeriod { get; set; } = "Ngày";
        public ObservableCollection<ISeries> AverageTransactionValueSeries { get; set; } = [];
        public ObservableCollection<Axis> AverageTransactionValueXAxes { get; set; } = [];
        public ObservableCollection<Axis> AverageTransactionValueYAxes { get; set; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;


        public RevenueReportViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            InitializeCharts();
            LoadAllDataAsync();
        }

        private void InitializeCharts()
        {
            var currencyYAxis = new Axis
            {
                Name = "Giá trị (VNĐ)",
                NameTextSize = 13,
                MinLimit = 0,
                Labeler = value => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))
            };

            var timeXAxis = new Axis
            {
                Name = "Thời gian",
                NameTextSize = 13,
                LabelsRotation = 15
            };

            // Since Axis does not have a Clone method, create new instances with the same properties
            TotalRevenueProfitYAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = currencyYAxis.Name,
                    NameTextSize = currencyYAxis.NameTextSize,
                    MinLimit = currencyYAxis.MinLimit,
                    Labeler = currencyYAxis.Labeler
                }
            };

            AverageTransactionValueYAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = currencyYAxis.Name,
                    NameTextSize = currencyYAxis.NameTextSize,
                    MinLimit = currencyYAxis.MinLimit,
                    Labeler = currencyYAxis.Labeler
                }
            };

            TotalRevenueProfitXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = timeXAxis.Name,
                    NameTextSize = timeXAxis.NameTextSize,
                    LabelsRotation = timeXAxis.LabelsRotation
                }
            };

            AverageTransactionValueXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = timeXAxis.Name,
                    NameTextSize = timeXAxis.NameTextSize,
                    LabelsRotation = timeXAxis.LabelsRotation
                }
            };
        }

        private async void LoadAllDataAsync()
        {
            await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
            try
            {
                // Fetch all orders once, assuming async support
                _allOrders = await Task.Run(() => _dao.Orders.GetAll().ToList());

                // Filter completed orders
                _allOrders = _allOrders
                    .Where(o => o.Status.Equals("Hoàn thành", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                await Task.WhenAll(
                    LoadTotalChartDataAsync(),
                    LoadAverageChartDataAsync()
                );
            }
            catch (Exception ex)
            {
                await _dispatcherQueue.TryEnqueueAsync(() => Debug.WriteLine($"Error loading revenue data: {ex.Message}"));
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
            }
        }


        // --- Data Loading Logic ---

        // Tải dữ liệu cho biểu đồ Tổng doanh thu/lợi nhuận
        public async Task LoadTotalChartDataAsync()
        {
            await Task.Run(async () =>
            {
                var revenueData = new ObservableCollection<ObservablePoint>();
                var profitData = new ObservableCollection<ObservablePoint>(); // Lợi nhuận vẫn là mock
                var labels = new List<string>();
                var random = new Random();

                DateTime startDate, endDate;
                int numberOfUnits;

                // Xác định khoảng thời gian và số đơn vị
                (startDate, endDate, numberOfUnits) = GetDateRange(SelectedTotalTimePeriod);

                // Lọc đơn hàng trong khoảng thời gian
                var ordersInPeriod = _allOrders
                    .Where(o => o.Date >= startDate.Date && o.Date <= endDate.Date)
                    .ToList();

                // Tính toán dựa trên kỳ đã chọn
                switch (SelectedTotalTimePeriod)
                {
                    case "Ngày":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var currentDate = startDate.AddDays(i).Date;
                            labels.Add(currentDate.ToString("dd/MM"));
                            double dailyRevenue = ordersInPeriod
                                .Where(o => o.Date == currentDate)
                                .Sum(o => (double)o.TotalAfterDiscount);
                            revenueData.Add(new ObservablePoint(i, dailyRevenue));

                            double dailyProfit = (double)ordersInPeriod
                                .Where(o => o.Date == currentDate)
                                .Sum(o => o.TotalAfterDiscount - o.TotalCostPrice);
                            profitData.Add(new ObservablePoint(i, dailyProfit));
                        }
                        break;

                    case "Tuần":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var weekStartDate = startDate.AddDays(i * 7);
                            var weekEndDate = weekStartDate.AddDays(6);
                            var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(weekStartDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                            labels.Add($"Tuần {week}, {weekStartDate.Year}");

                            double weeklyRevenue = ordersInPeriod
                                .Where(o => o.Date.Date >= weekStartDate && o.Date.Date <= weekEndDate)
                                .Sum(o => (double)o.TotalAfterDiscount);
                            revenueData.Add(new ObservablePoint(i, weeklyRevenue));

                            double weeklyProfit = (double)ordersInPeriod
                                .Where(o => o.Date.Date >= weekStartDate && o.Date.Date <= weekEndDate)
                                .Sum(o => o.TotalAfterDiscount - o.TotalCostPrice);
                            profitData.Add(new ObservablePoint(i, weeklyProfit));
                        }
                        break;

                    case "Tháng":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var monthDate = startDate.AddMonths(i);
                            labels.Add(monthDate.ToString("MM/yyyy"));
                            double monthlyRevenue = ordersInPeriod
                                .Where(o => o.Date.Year == monthDate.Year && o.Date.Month == monthDate.Month)
                                .Sum(o => (double)o.TotalAfterDiscount);
                            revenueData.Add(new ObservablePoint(i, monthlyRevenue));

                            double monthlyProfit = (double)ordersInPeriod
                                .Where(o => o.Date.Year == monthDate.Year && o.Date.Month == monthDate.Month)
                                .Sum(o => o.TotalAfterDiscount - o.TotalCostPrice);
                            profitData.Add(new ObservablePoint(i, monthlyProfit));
                        }
                        break;

                    case "Quý":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var (quarterStartDate, quarterEndDate, quarterLabel) = GetQuarterDetails(startDate, i);
                            labels.Add(quarterLabel);
                            double quarterlyRevenue = ordersInPeriod
                                .Where(o => o.Date.Date >= quarterStartDate && o.Date.Date <= quarterEndDate)
                                .Sum(o => (double)o.TotalAfterDiscount);
                            revenueData.Add(new ObservablePoint(i, quarterlyRevenue));

                            double quarterlyProfit = (double)ordersInPeriod
                                .Where(o => o.Date.Date >= quarterStartDate && o.Date.Date <= quarterEndDate)
                                .Sum(o => o.TotalAfterDiscount - o.TotalCostPrice);
                            profitData.Add(new ObservablePoint(i, quarterlyProfit));
                        }
                        break;

                    case "Năm":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var year = startDate.AddYears(i).Year;
                            labels.Add(year.ToString());
                            double yearlyRevenue = ordersInPeriod
                                .Where(o => o.Date.Year == year)
                                .Sum(o => (double)o.TotalAfterDiscount);
                            revenueData.Add(new ObservablePoint(i, yearlyRevenue));

                            var yearlyProfit = (double)ordersInPeriod
                                .Where(o => o.Date.Year == year)
                                .Sum(o => o.TotalAfterDiscount - o.TotalCostPrice);
                            profitData.Add(new ObservablePoint(i, yearlyProfit));
                        }
                        break;
                }

                await _dispatcherQueue.TryEnqueueAsync(() =>
                {
                    TotalRevenueProfitXAxes[0].Name = SelectedTotalTimePeriod;
                    TotalRevenueProfitXAxes[0].Labels = labels;

                    TotalRevenueProfitSeries.Clear();
                    TotalRevenueProfitSeries.Add(new ColumnSeries<ObservablePoint>
                    {
                        Name = "Doanh thu",
                        Values = revenueData,
                        Stroke = null,
                        DataLabelsPosition = DataLabelsPosition.Top,
                        DataLabelsFormatter = (point) => $"{point.Coordinate.PrimaryValue:N0} VNĐ"
                    });
                    TotalRevenueProfitSeries.Add(new LineSeries<ObservablePoint>
                    {
                        Name = "Lợi nhuận",
                        Values = profitData,
                        Fill = null,
                        DataLabelsPosition = DataLabelsPosition.Bottom,
                        DataLabelsFormatter = (point) => $"{point.Coordinate.PrimaryValue:N0} VNĐ"
                    });
                });
            });
        }


        public async Task LoadAverageChartDataAsync()
        {
            await Task.Run(async () =>
            {
                var avgTransactionValueData = new ObservableCollection<ObservablePoint>();
                var labels = new List<string>();

                DateTime startDate, endDate;
                int numberOfUnits;

                (startDate, endDate, numberOfUnits) = GetDateRange(SelectedAverageTimePeriod);

                var ordersInPeriod = _allOrders
                    .Where(o => o.Date.Date >= startDate.Date && o.Date.Date <= endDate.Date)
                    .ToList();

                // Tính toán trung bình
                switch (SelectedAverageTimePeriod)
                {
                    case "Ngày":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var currentDate = startDate.AddDays(i).Date;
                            labels.Add(currentDate.ToString("dd/MM"));

                            int dailyTotalOrders = ordersInPeriod.Where(o => o.Date == currentDate).Count();
                            double avgTransactionValue = ordersInPeriod
                                .Where(o => o.Date == currentDate)
                                .Sum(o => (double)o.TotalAfterDiscount) / Math.Max(dailyTotalOrders, 1);
                            avgTransactionValueData.Add(new ObservablePoint(i, avgTransactionValue));
                        }
                        break;

                    case "Tuần":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var weekStartDate = startDate.AddDays(i * 7);
                            var weekEndDate = weekStartDate.AddDays(6);
                            var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(weekStartDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                            labels.Add($"Tuần {week}, {weekStartDate.Year}");

                            double weeklyTotalOrders = ordersInPeriod
                                .Where(o => o.Date.Date >= weekStartDate && o.Date.Date <= weekEndDate)
                                .Count();
                            double weeklyATV = ordersInPeriod
                                .Where(o => o.Date.Date >= weekStartDate && o.Date.Date <= weekEndDate)
                                .Sum(o => (double)o.TotalAfterDiscount) / Math.Max(weeklyTotalOrders, 1);
                            avgTransactionValueData.Add(new ObservablePoint(i, weeklyATV));
                        }
                        break;

                    case "Tháng":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var monthDate = startDate.AddMonths(i);
                            labels.Add(monthDate.ToString("MM/yyyy"));
                            int monthlyTotalOrders = ordersInPeriod
                                .Where(o => o.Date.Year == monthDate.Year && o.Date.Month == monthDate.Month)
                                .Count();
                            double monthlyATV = ordersInPeriod
                                .Where(o => o.Date.Year == monthDate.Year && o.Date.Month == monthDate.Month)
                                .Sum(o => (double)o.TotalAfterDiscount) / Math.Max(monthlyTotalOrders, 1);
                            avgTransactionValueData.Add(new ObservablePoint(i, monthlyATV));
                        }
                        break;

                    case "Quý":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var (quarterStartDate, quarterEndDate, quarterLabel) = GetQuarterDetails(startDate, i);
                            labels.Add(quarterLabel);
                            double quarterlyTotalOrders = ordersInPeriod
                                .Where(o => o.Date.Date >= quarterStartDate && o.Date.Date <= quarterEndDate)
                                .Count();
                            double quarterlyATV = ordersInPeriod
                                .Where(o => o.Date.Date >= quarterStartDate && o.Date.Date <= quarterEndDate)
                                .Sum(o => (double)o.TotalAfterDiscount) / Math.Max(quarterlyTotalOrders, 1);
                            avgTransactionValueData.Add(new ObservablePoint(i, quarterlyATV));
                        }
                        break;

                    case "Năm":
                        labels = new List<string>();
                        for (int i = 0; i < numberOfUnits; i++)
                        {
                            var year = startDate.AddYears(i).Year;
                            labels.Add(year.ToString());
                            int yearlyTotalOrders = ordersInPeriod
                                .Where(o => o.Date.Year == year)
                                .Count();
                            double yearlyATV = ordersInPeriod
                                .Where(o => o.Date.Year == year)
                                .Sum(o => (double)o.TotalAfterDiscount) / Math.Max(yearlyTotalOrders, 1);
                            avgTransactionValueData.Add(new ObservablePoint(i, yearlyATV));
                        }
                        break;
                }

                await _dispatcherQueue.TryEnqueueAsync(() =>
                {
                    AverageTransactionValueXAxes[0].Name = SelectedAverageTimePeriod;
                    AverageTransactionValueXAxes[0].Labels = labels;

                    AverageTransactionValueSeries.Clear();
                    AverageTransactionValueSeries.Add(new LineSeries<ObservablePoint>
                    {
                        Name = "Giá trị trung bình mỗi hóa đơn",
                        Values = avgTransactionValueData,
                        Fill = null,
                        DataLabelsPosition = DataLabelsPosition.Bottom,
                        DataLabelsFormatter = (point) => $"{point.Coordinate.PrimaryValue:N0} đ"
                    });
                });
            });
        }

        // Lấy khoảng ngày và số đơn vị thời gian dựa trên lựa chọn
        private (DateTime StartDate, DateTime EndDate, int NumberOfUnits) GetDateRange(string timePeriod)
        {
            DateTime today = DateTime.Now.Date;
            DateTime startDate = today;
            DateTime endDate = today;
            int numberOfUnits = 0;

            switch (timePeriod)
            {
                case "Ngày": // 14 ngày gần nhất
                    numberOfUnits = 14;
                    startDate = today.AddDays(-(numberOfUnits - 1));
                    endDate = today;
                    break;
                case "Tuần": // 6 tuần gần nhất
                    numberOfUnits = 6;
                    startDate = GetFirstDayOfWeek(today.AddDays(-7 * (numberOfUnits - 1)));
                    endDate = GetFirstDayOfWeek(today).AddDays(6); // Kết thúc vào Chủ Nhật của tuần hiện tại
                    break;
                case "Tháng": // 12 tháng gần nhất
                    numberOfUnits = 12;
                    startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-(numberOfUnits - 1));
                    var lastMonthEndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
                    endDate = lastMonthEndDate; // Kết thúc vào ngày cuối của tháng hiện tại
                    break;
                case "Quý": // 5 quý gần nhất
                    numberOfUnits = 5;
                    var currentQuarterStart = GetStartOfQuarter(today.Year, (today.Month - 1) / 3 + 1);
                    startDate = AddQuarters(currentQuarterStart, -(numberOfUnits - 1));
                    endDate = AddQuarters(currentQuarterStart, 1).AddDays(-1); // Ngày cuối của quý hiện tại
                    break;
                case "Năm": // 5 năm gần nhất
                    numberOfUnits = 5;
                    startDate = new DateTime(today.Year - (numberOfUnits - 1), 1, 1);
                    endDate = new DateTime(today.Year, 12, 31); // Ngày cuối của năm hiện tại
                    break;
            }
            return (startDate, endDate, numberOfUnits);
        }

        // Lấy chi tiết của quý thứ i tính từ startQuarterDate
        private (DateTime QuarterStartDate, DateTime QuarterEndDate, string Label) GetQuarterDetails(DateTime startQuarterDate, int quarterIndex)
        {
            var currentQuarterStart = AddQuarters(startQuarterDate, quarterIndex);
            var currentQuarterEnd = AddQuarters(currentQuarterStart, 1).AddDays(-1);
            var quarter = (currentQuarterStart.Month - 1) / 3 + 1;
            var year = currentQuarterStart.Year;
            var label = $"Quý {quarter}/{year}";
            return (currentQuarterStart, currentQuarterEnd, label);
        }

        // Lấy ngày đầu tiên của tuần (Thứ 2)
        private static DateTime GetFirstDayOfWeek(DateTime date)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            DayOfWeek firstDay = ci.DateTimeFormat.FirstDayOfWeek; // Lấy ngày đầu tuần theo Culture hiện tại (có thể là CN)
                                                                   // Chỉnh về thứ 2 nếu cần
            firstDay = DayOfWeek.Monday; // Ép về thứ 2 theo ISO 8601

            int offset = firstDay - date.DayOfWeek;
            if (offset > 0) offset -= 7; // Đảm bảo luôn lùi về đúng tuần
            return date.AddDays(offset).Date;
        }

        // Lấy ngày đầu quý
        private static DateTime GetStartOfQuarter(int year, int quarter)
        {
            if (quarter < 1 || quarter > 4)
                throw new ArgumentOutOfRangeException(nameof(quarter), "Quý phải từ 1 đến 4.");
            int month = (quarter - 1) * 3 + 1;
            return new DateTime(year, month, 1);
        }

        // Cộng/Trừ số quý vào một ngày
        private static DateTime AddQuarters(DateTime date, int quarters)
        {
            return date.AddMonths(quarters * 3);
        }
    }
}
