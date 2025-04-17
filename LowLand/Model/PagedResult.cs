using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LowLand.Model
{
    public class PagedResult<T> : INotifyPropertyChanged
    {
        private List<T> _items;
        private int _currentPage;
        private int _pageSize;
        private int _totalItems;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<T> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }


        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                if (SetProperty(ref _totalItems, value))
                {
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public PagedResult(List<T> items, int currentPage, int pageSize, int totalItems)
        {
            Items = items;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalItems = totalItems;
        }

        protected bool SetProperty<TProperty>(ref TProperty field, TProperty value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TProperty>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
