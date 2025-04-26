using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LowLand.Model;

namespace LowLand.View.ViewModel
{
    public class PagingViewModel<T> : INotifyPropertyChanged
    {
        private readonly Func<int, int, string, Task<PagedResult<T>>> _loadDataAsync;
        private PagedResult<T> _pagedItems;
        private int _pageSize;
        private int _currentPage;
        private string _searchKeyword;

        public ObservableCollection<T> Items { get; } = new ObservableCollection<T>();

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value && value > 0 && value <= TotalPages)
                {
                    _currentPage = value;
                    _ = LoadPageAsync(value); // Fire-and-forget (hoặc gọi từ UI async)
                    OnPropertyChanged(nameof(CurrentPage));
                    OnPropertyChanged(nameof(CanGoToPreviousPage));
                    OnPropertyChanged(nameof(CanGoToNextPage));
                }
            }
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    Debug.WriteLine($"SearchKeyword set to: '{_searchKeyword}'");
                    _currentPage = 1;
                    _ = LoadPageAsync(_currentPage);
                    OnPropertyChanged(nameof(SearchKeyword));
                }
            }
        }

        public int TotalPages => Math.Max(_pagedItems?.TotalPages ?? 1, 1);

        public bool CanGoToPreviousPage => CurrentPage > 1;

        public bool CanGoToNextPage => CurrentPage < TotalPages;

        public List<string> PageIndicators
        {
            get
            {
                var total = TotalPages;
                return total > 0
                    ? Enumerable.Range(1, total).Select(i => $"{i}/{total}").ToList()
                    : new List<string> { "1/1" };
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public PagingViewModel(Func<int, int, string, Task<PagedResult<T>>> loadDataAsync, int pageSize = 10)
        {
            _loadDataAsync = loadDataAsync ?? throw new ArgumentNullException(nameof(loadDataAsync));
            _pageSize = pageSize;
            _currentPage = 1;
            _searchKeyword = string.Empty;
            _pagedItems = new PagedResult<T>(new List<T>(), 1, pageSize, 0);
            _ = LoadPageAsync(1);
        }

        public async Task GoToNextPageAsync()
        {
            if (CanGoToNextPage)
            {
                _currentPage++;
                await LoadPageAsync(_currentPage);
            }
        }

        public async Task GoToPreviousPageAsync()
        {
            if (CanGoToPreviousPage)
            {
                _currentPage--;
                await LoadPageAsync(_currentPage);
            }
        }

        public async Task RefreshAsync()
        {
            if (_currentPage > TotalPages && TotalPages > 0)
            {
                _currentPage = TotalPages;
            }
            else if (_currentPage < 1)
            {
                _currentPage = 1;
            }
            await LoadPageAsync(_currentPage);
        }

        public async Task LoadPageAsync(int pageNumber)
        {
            Debug.WriteLine($"Loading page {pageNumber} with keyword: '{_searchKeyword}'");

            var result = await _loadDataAsync(pageNumber, _pageSize, _searchKeyword);
            _pagedItems = result ?? new PagedResult<T>(new List<T>(), pageNumber, _pageSize, 0);

            _currentPage = Math.Clamp(pageNumber, 1, TotalPages);

            Items.Clear();
            foreach (var item in _pagedItems.Items)
            {
                Items.Add(item);
            }

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(PageIndicators));
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
            OnPropertyChanged(nameof(CurrentPage));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}