using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LowLand.Model.Order;
using LowLand.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Printing;
using Windows.Foundation;
using Windows.Graphics.Printing;
using Windows.System;

namespace LowLand.View.ViewModel
{
    public class OrderViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IDao _dao;
        private readonly PagingViewModel<Order> _paging;
        private PrintDocument printDocument;
        private IPrintDocumentSource printDocumentSource;
        private Order currentOrderToPrint;
        private List<UIElement> printPreviewPages = new();

        public PagingViewModel<Order> Paging => _paging;

        public event PropertyChangedEventHandler PropertyChanged;

        public OrderViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            _paging = new PagingViewModel<Order>(
                (page, size, keyword) => _dao.Orders.GetAll(page, size, keyword),
                pageSize: 10
            );
            InitializePrinting();
        }

        private void InitializePrinting()
        {
            RegisterForPrinting();
        }

        public void Add(Order item)
        {
            item.Date = DateTime.Now;
            item.Status = "Đang xử lý";
            int result = _dao.Orders.Insert(item);
            if (result == 1)
            {
                item.Id = _dao.Orders.GetAll().Max(o => o.Id);
                _paging.Refresh();
            }
            else
            {
                Console.WriteLine("Insert failed: No rows affected.");
            }
        }

        public void Remove(Order item)
        {
            int result = _dao.Orders.DeleteById(item.Id.ToString());
            if (result == 1)
            {
                _paging.Refresh();
            }
            else
            {
                Console.WriteLine("Delete failed: No rows affected.");
            }
        }

        public void Update(Order item)
        {
            int result = _dao.Orders.UpdateById(item.Id.ToString(), item);
            if (result == 1)
            {
                _paging.Refresh();
            }
            else
            {
                Console.WriteLine("Update failed: No rows affected.");
            }
        }

        public async Task PrintInvoice(Order order, XamlRoot xamlRoot)
        {
            currentOrderToPrint = order;
            Debug.WriteLine($"In hóa đơn #{currentOrderToPrint.Id}");
            Debug.WriteLine($"Khách hàng: {currentOrderToPrint.CustomerName}");
            Debug.WriteLine($"SĐT: {currentOrderToPrint.CustomerPhone}");
            printDocument.InvalidatePreview();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

            try
            {
                await PrintManagerInterop.ShowPrintUIForWindowAsync(hwnd);
            }
            catch
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Lỗi khi in",
                    Content = "Không thể in vào lúc này. Vui lòng thử lại sau.",
                    PrimaryButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void RegisterForPrinting()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            var printManager = PrintManagerInterop.GetForWindow(hWnd);
            printManager.PrintTaskRequested += PrintTask_Requested;

            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += PrintDocument_Paginate;
            printDocument.GetPreviewPage += PrintDocument_GetPreviewPage;
            printDocument.AddPages += PrintDocument_AddPages;
        }

        private void UnregisterForPrinting()
        {
            if (printDocument == null) return;

            printDocument.Paginate -= PrintDocument_Paginate;
            printDocument.GetPreviewPage -= PrintDocument_GetPreviewPage;
            printDocument.AddPages -= PrintDocument_AddPages;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            var printManager = PrintManagerInterop.GetForWindow(hWnd);
            printManager.PrintTaskRequested -= PrintTask_Requested;
        }

        private void PrintDocument_Paginate(object sender, PaginateEventArgs e)
        {
            printPreviewPages.Clear();

            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                if (currentOrderToPrint == null) return;

                var options = (PrintTaskOptions)e.PrintTaskOptions;
                var pageDescription = options.GetPageDescription(0);
                double pageWidth = pageDescription.PageSize.Width;
                double pageHeight = pageDescription.PageSize.Height;

                int maxItemsPerPage = 13;
                var details = currentOrderToPrint.Details.ToList();
                int totalPages = (int)Math.Ceiling((double)details.Count / maxItemsPerPage);

                for (int i = 0; i < totalPages; i++)
                {
                    var pageDetails = details.GetRange(i * maxItemsPerPage, Math.Min(maxItemsPerPage, details.Count - i * maxItemsPerPage));

                    var partialOrder = new Order
                    {
                        Id = currentOrderToPrint.Id,
                        CustomerName = currentOrderToPrint.CustomerName,
                        CustomerPhone = currentOrderToPrint.CustomerPhone,
                        Date = currentOrderToPrint.Date,
                        TotalAfterDiscount = currentOrderToPrint.TotalAfterDiscount,
                        Details = new ObservableCollection<OrderDetail>(pageDetails)
                    };

                    var invoicePage = new InvoicePrint(partialOrder)
                    {
                        Width = pageWidth,
                        Height = pageHeight
                    };

                    invoicePage.Measure(new Size(pageWidth, pageHeight));
                    invoicePage.Arrange(new Rect(0, 0, pageWidth, pageHeight));
                    invoicePage.UpdateLayout();

                    printPreviewPages.Add(invoicePage);
                }

                printDocument.SetPreviewPageCount(totalPages, PreviewPageCountType.Final);
                Debug.WriteLine($"✅ Đã phân chia hóa đơn thành {totalPages} trang");
            });
        }

        private void PrintDocument_GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            int pageIndex = e.PageNumber - 1;

            if (pageIndex >= 0 && pageIndex < printPreviewPages.Count)
            {
                printDocument.SetPreviewPage(e.PageNumber, printPreviewPages[pageIndex]);
            }
            else
            {
                Debug.WriteLine($"⚠️ Không tìm thấy trang số {e.PageNumber} trong danh sách preview!");
            }
        }

        private void PrintDocument_AddPages(object sender, AddPagesEventArgs e)
        {
            foreach (var page in printPreviewPages)
            {
                printDocument.AddPage(page);
            }
            printDocument.AddPagesComplete();
        }

        private void PrintTask_Requested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var printTask = args.Request.CreatePrintTask($"Hóa đơn #{currentOrderToPrint.Id}", PrintTaskSourceRequested);
            printTask.Completed += PrintTask_Completed;


        }

        private void PrintTaskSourceRequested(PrintTaskSourceRequestedArgs args)
        {
            args.SetSource(printDocumentSource);
        }

        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            string status = args.Completion switch
            {
                PrintTaskCompletion.Failed => "In thất bại!",
                PrintTaskCompletion.Canceled => "Đã huỷ in.",
                _ => "In thành công!"
            };
            Debug.WriteLine(status);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            UnregisterForPrinting();
        }
    }
}