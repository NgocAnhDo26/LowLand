namespace LowLand.View.ViewModel
{
    public class ProductInfoViewModel //: INotifyPropertyChanged
    {
        //private IDao _dao;
        //private bool _isSingleProduct = true;

        //public event PropertyChangedEventHandler? PropertyChanged;

        //public Product Product { get; set; }

        //public List<ProductType> AllProductTypes { get; set; }

        //private FullObservableCollection<ProductType> _filteredProductTypes;
        //public FullObservableCollection<ProductType> FilteredProductTypes
        //{
        //    get => _filteredProductTypes;
        //    set
        //    {
        //        _filteredProductTypes = value;
        //    }
        //}
        //public FullObservableCollection<ProductOption> ProductOptions { get; set; }

        //public ProductInfoViewModel()
        //{
        //    _dao = Services.Services.GetKeyedSingleton<IDao>();
        //    Categories = _dao.Categories.GetAll();
        //    //   AllProductTypes = _dao.ProductTypes.GetAll();
        //}

        //public void LoadProduct(string productId)
        //{
        //    Product = _dao.Products.GetById(productId);

        //    if (Product is ComboProduct)
        //    {
        //        _isSingleProduct = false;
        //    }
        //    else
        //    {
        //        // Filter options by product id
        //        List<ProductOption> options = _dao.ProductOptions.GetAll();
        //        options = options.Where(o => o.ProductId == Product.Id).ToList();
        //        ProductOptions = new FullObservableCollection<ProductOption>(options);
        //    }
        //}

        //public bool UpdateProduct()
        //{
        //    int result = _dao.Products.UpdateById(Product.Id.ToString(), Product);
        //    if (result != 1)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public bool UpdateProductOption(ProductOption option)
        //{
        //    int result = _dao.ProductOptions.UpdateById(option.OptionId.ToString(), option);

        //    if (result == -1)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public bool AddNewProductOption(ProductOption option)
        //{
        //    int newId = _dao.ProductOptions.Insert(option);

        //    if (newId == -1)
        //    {
        //        return false;
        //    }

        //    // Add to the list
        //    option.OptionId = newId;
        //    ProductOptions.Add(option);

        //    return true;
        //}

        //public bool DeleteProductOption(int optionId)
        //{
        //    int result = _dao.ProductOptions.DeleteById(optionId.ToString());
        //    if (result == -1)
        //    {
        //        return false;
        //    }

        //    var option = ProductOptions.FirstOrDefault(o => o.OptionId == optionId);
        //    if (option != null)
        //    {
        //        ProductOptions.Remove(option);
        //    }

        //    return true;
        //}

        //public void OnCategoryChange(Category category)
        //{
        //    if (_isSingleProduct)
        //    {
        //        (Product as SingleProduct)!.Category = category;
        //        //  FilterProductTypesByCategory();
        //    }
        //}

        //private void FilterProductTypesByCategory()
        //{
        //    var filtered = AllProductTypes.Where(pt => pt.Category.Id == (Product as SingleProduct)!.Category.Id);
        //    FilteredProductTypes = new FullObservableCollection<ProductType>(filtered);
        //}

        //internal void OnProductTypeChange(ProductType type)
        //{
        //    (Product as SingleProduct)!.ProductType = type;
        //}
    }
}
