using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using LowLand.Model;

namespace LowLand.View.ViewModel
{
    public class PagingViewModel<T> : INotifyPropertyChanged
    {
        private readonly Func<int, int, string, PagedResult<T>> _loadData;
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
                    LoadPage(value);
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
                    CurrentPage = 1; // Reset to first page on search
                    OnPropertyChanged(nameof(SearchKeyword));
                }
            }
        }

        public int TotalPages => Math.Max(_pagedItems?.TotalPages ?? 1, 1); // Đảm bảo ít nhất 1 trang

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

        public event PropertyChangedEventHandler PropertyChanged;

        public PagingViewModel(Func<int, int, string, PagedResult<T>> loadData, int pageSize = 10)
        {
            _loadData = loadData ?? throw new ArgumentNullException(nameof(loadData));
            _pageSize = pageSize;
            _currentPage = 1;
            _searchKeyword = string.Empty;
            _pagedItems = new PagedResult<T>(new List<T>(), 1, pageSize, 0);
            LoadPage(1);
        }

        public void GoToNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
            }
        }

        public void GoToPreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
            }
        }

        public void Refresh()
        {
            // Đảm bảo CurrentPage hợp lệ
            if (_currentPage > TotalPages && TotalPages > 0)
            {
                _currentPage = TotalPages;
                OnPropertyChanged(nameof(CurrentPage));
            }
            else if (_currentPage < 1)
            {
                _currentPage = 1;
                OnPropertyChanged(nameof(CurrentPage));
            }
            LoadPage(_currentPage);
        }

        private void LoadPage(int pageNumber)
        {
            Debug.WriteLine($"Loading page {pageNumber} with keyword: '{_searchKeyword}'");
            var result = _loadData(pageNumber, _pageSize, _searchKeyword);
            _pagedItems = result ?? new PagedResult<T>(new List<T>(), pageNumber, _pageSize, 0);
            Debug.WriteLine($"Loaded {(_pagedItems?.Items?.Count() ?? 0)} items, TotalPages: {TotalPages}");


            if (pageNumber > TotalPages && TotalPages > 0)
            {
                _currentPage = TotalPages;
            }
            else if (pageNumber < 1)
            {
                _currentPage = 1;
            }
            else
            {
                _currentPage = pageNumber;
            }

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