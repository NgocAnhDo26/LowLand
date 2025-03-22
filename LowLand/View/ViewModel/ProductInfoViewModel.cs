using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    class ProductInfoViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        private bool _isSingleProduct = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Product Product { get; set; }
        public List<Category> Categories { get; }
        public List<ProductType> AllProductTypes { get; set; }

        private FullObservableCollection<ProductType> _filteredProductTypes;
        public FullObservableCollection<ProductType> FilteredProductTypes
        {
            get => _filteredProductTypes;
            set
            {
                _filteredProductTypes = value;
            }
        }

        public ProductInfoViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Categories = _dao.Categories.GetAll();
            AllProductTypes = _dao.ProductTypes.GetAll();
        }

        public void LoadProduct(string productId)
        {
            Product = _dao.Products.GetById(productId);

            if (Product is ComboProduct)
            {
                _isSingleProduct = false;
            }
        }

        public bool UpdateProduct()
        {
            int result = _dao.Products.UpdateById(Product.Id.ToString(), Product);
            if (result != 1)
            {
                return false;
            }

            return true;
        }

        public void OnCategoryChange(Category category)
        {
            if (_isSingleProduct)
            {
                (Product as SingleProduct)!.Category = category;
                FilterProductTypesByCategory();
            }
        }

        private void FilterProductTypesByCategory()
        {
            var filtered = AllProductTypes.Where(pt => pt.CategoryId == (Product as SingleProduct)!.Category.Id);
            FilteredProductTypes = new FullObservableCollection<ProductType>(filtered);
        }

        internal void OnProductTypeChange(ProductType type)
        {
            (Product as SingleProduct)!.ProductType = type;
        }
    }
}
