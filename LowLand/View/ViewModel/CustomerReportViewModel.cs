using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LowLand.Services;
using Microsoft.UI.Dispatching;

namespace LowLand.View.ViewModel
{
    public class CustomerReportViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        private readonly DispatcherQueue _dispatcherQueue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsLoading { get; set; } = true;

        // Customer report properties
        public ObservableCollection<ISeries> TotalSpentSeries { get; set; } = [];
        public ObservableCollection<Axis> TotalSpentXAxes { get; set; } = [];
        public ObservableCollection<Axis> TotalSpentYAxes { get; set; } = [];

        // New customers properties
        public ObservableCollection<ISeries> NewCustomerSeries { get; set; } = [];
        public ObservableCollection<Axis> NewCustomerXAxes { get; set; } = [];
        public ObservableCollection<Axis> NewCustomerYAxes { get; set; } = [];

        public CustomerReportViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            InitializeCharts();
            LoadCustomerReportDataAsync();
        }

        private void InitializeCharts()
        {
            var currencyYAxis = new Axis
            {
                Name = "Tổng chi tiêu (VNĐ)",
                NameTextSize = 13,
                MinLimit = 0,
                Labeler = value => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))
            };

            TotalSpentYAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = currencyYAxis.Name,
                    NameTextSize = currencyYAxis.NameTextSize,
                    MinLimit = currencyYAxis.MinLimit,
                    Labeler = currencyYAxis.Labeler
                }
            };

            NewCustomerYAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Số lượng khách mới",
                    NameTextSize = 13,
                    MinLimit = 0,
                }
            };

            TotalSpentXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Tên khách hàng",
                    NameTextSize = 13,
                    LabelsRotation = -35,
                }
            };

            NewCustomerXAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Thời gian",
                    NameTextSize = 13,
                }
            };
        }

        private async void LoadCustomerReportDataAsync()
        {
            await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = true);
            try
            {
                await Task.Run(async () =>
                {
                    var topCustomersQuery = _dao.Orders.GetAll()
                        .Where(o => o.Status == "Hoàn thành")
                        .GroupBy(o => o.CustomerId)
                        .Select(g => new
                        {
                            CustomerId = g.Key,
                            TotalSpent = g.Sum(o => o.TotalAfterDiscount)
                        })
                        .OrderByDescending(x => x.TotalSpent)
                        .Take(10)
                        .Join(_dao.Customers.GetAll(),
                            orderGroup => orderGroup.CustomerId,
                            customer => customer.Id,
                            (orderGroup, customer) => new
                            {
                                CustomerName = customer.Name,
                                orderGroup.TotalSpent
                            })
                        .ToList();

                    var totalSpent = topCustomersQuery.Select(x => x.TotalSpent).ToList();
                    var customerLabels = topCustomersQuery.Select(x => x.CustomerName).ToList();

                    // Fetch new customers for the last 14 days in a single query
                    var endDate = DateOnly.FromDateTime(DateTime.Now.Date);
                    var startDate = endDate.AddDays(-13); // 14 days inclusive
                    var newCustomersByDate = _dao.Customers.GetAll()
                        .Where(c => c.RegistrationDate >= startDate && c.RegistrationDate <= endDate)
                        .GroupBy(c => c.RegistrationDate)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Count = g.Count()
                        })
                        .ToDictionary(x => x.Date, x => x.Count);

                    var newCustomers = new List<int>();
                    var dateLabels = new List<string>();
                    for (int i = 0; i < 14; i++)
                    {
                        var date = endDate.AddDays(-i);
                        dateLabels.Insert(0, date.ToString("dd/MM"));
                        newCustomers.Insert(0, newCustomersByDate.GetValueOrDefault(date, 0));
                    }

                    await _dispatcherQueue.TryEnqueueAsync(() =>
                    {
                        TotalSpentXAxes[0].Labels = customerLabels;
                        NewCustomerXAxes[0].Labels = dateLabels;

                        TotalSpentSeries.Clear();
                        TotalSpentSeries.Add(new ColumnSeries<int>
                        {
                            Values = totalSpent,
                            Name = "Tổng chi tiêu",
                            DataLabelsSize = 13
                        });

                        NewCustomerSeries.Clear();
                        NewCustomerSeries.Add(new ColumnSeries<int>
                        {
                            Values = newCustomers,
                            Name = "Khách hàng mới",
                            DataLabelsSize = 13
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                await _dispatcherQueue.TryEnqueueAsync(() => Debug.WriteLine($"Error loading customer report data: {ex.Message}"));
            }
            finally
            {
                await _dispatcherQueue.TryEnqueueAsync(() => IsLoading = false);
            }
        }
    }
}
