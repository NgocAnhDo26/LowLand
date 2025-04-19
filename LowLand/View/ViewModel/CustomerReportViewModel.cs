using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LowLand.Services;

namespace LowLand.View.ViewModel
{
    public class CustomerReportViewModel
    {
        private IDao _dao;

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
            InitializeCharts();
            LoadCustomerReportData();
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

        private void LoadCustomerReportData()
        {
            var customers = _dao.Customers.GetAll();
            var customerNames = customers.Select(c => c.Name).ToArray();
            var totalSpent = new List<decimal>();
            var newCustomers = new List<int>();
            var dateLabels = new List<string>();
            var customerLabels = new List<string>();

            var topCustomers = customers
                .Select(c => new
                {
                    Customer = c,
                    TotalSpent = _dao.Orders.GetAll()
                        .Where(o => o.CustomerId == c.Id && o.Status == "Hoàn thành")
                        .Sum(o => o.TotalAfterDiscount)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToList();

            foreach (var entry in topCustomers)
            {
                totalSpent.Add(entry.TotalSpent);
                customerLabels.Add(entry.Customer.Name);
            }

            TotalSpentXAxes[0].Labels = customerLabels;

            for (int i = 0; i < 14; ++i)
            {
                var date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-i));
                dateLabels.Insert(0, date.ToString("dd/MM"));

                var newCustomerCount = _dao.Customers.GetAll()
                    .Count(c => c.RegistrationDate == date);

                newCustomers.Insert(0, newCustomerCount);
            }

            NewCustomerXAxes[0].Labels = dateLabels;

            TotalSpentSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<decimal>
                {
                    Values = totalSpent,
                    Name = "Tổng chi tiêu",
                    DataLabelsSize = 13
                }
            };

            NewCustomerSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<int>
                {
                    Values = newCustomers,
                    Name = "Khách hàng mới",
                    DataLabelsSize = 13
                }
            };
        }
    }
}
