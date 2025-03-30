namespace LowLand.View.ViewModel
{
    public class ProductInfoViewModel //: INotifyPropertyChanged
    {
        private IDao _dao;
        private bool _isSingleProduct = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Product Product { get; set; }
        public string OldImage { get; set; }
        public List<Category> Categories { get; }
        public FullObservableCollection<ProductOption> ProductOptions { get; set; }

        public ProductInfoViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Categories = _dao.Categories.GetAll();
        }

        public void LoadProduct(string productId)
        {
            Product = _dao.Products.GetById(productId);
            OldImage = Product.Image;

            if (Product is ComboProduct)
            {
                _isSingleProduct = false;
            }
            else
            {
                // Filter options by product id
                List<ProductOption> options = _dao.ProductOptions.GetAll();
                options = options.Where(o => o.ProductId == Product.Id).ToList();
                ProductOptions = new FullObservableCollection<ProductOption>(options);
            }
        }

        public ResponseCode UpdateProduct()
        {
            // Validate if name is not empty
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                return ResponseCode.EmptyName;
            }

            // Validate if price is not negative
            if (Product.CostPrice < 0 || Product.SalePrice < 0)
            {
                return ResponseCode.NegativeValueNotAllowed;
            }

            // Validate if cost price is not greater than sale price
            if (Product.CostPrice > Product.SalePrice)
            {
                return ResponseCode.InvalidValue;
            }

            // Delete the old image (if it's not the default image, and the image has changed)
            if (OldImage != "product_default.jpg" && Product.Image != OldImage)
            {
                var status = FileUtils.DeleteImage(OldImage);
                if (!status)
                {
                    return ResponseCode.Error;
                }
            }

            // Save the image to the folder
            if (Product.Image != OldImage)
            {
                string newImageName = FileUtils.SaveImage(Product.Image);
                if (newImageName == "")
                {
                    return ResponseCode.Error;
                }

                Product.Image = newImageName;
            }

            // Update the product in the database
            int result = _dao.Products.UpdateById(Product.Id.ToString(), Product);
            if (result != 1)
            {
                return ResponseCode.Error;
            }

            return ResponseCode.Success;
        }

        internal void UpdateProductImage(string path, string fileName)
        {
            Product.Image = path;
        }

        public void OnCategoryChange(Category category)
        {
            (Product as SingleProduct)!.Category = category;
        }

        public bool UpdateProductOption(ProductOption option)
        {
            int result = _dao.ProductOptions.UpdateById(option.OptionId.ToString(), option);

            if (result == -1)
            {
                return false;
            }

            return true;
        }

        public bool AddNewProductOption(ProductOption option)
        {
            int newId = _dao.ProductOptions.Insert(option);

            if (newId == -1)
            {
                return false;
            }

            // Add to the list
            option.OptionId = newId;
            ProductOptions.Add(option);

            return true;
        }

        public bool DeleteProductOption(int optionId)
        {
            int result = _dao.ProductOptions.DeleteById(optionId.ToString());
            if (result == -1)
            {
                return false;
            }

            var option = ProductOptions.FirstOrDefault(o => o.OptionId == optionId);
            if (option != null)
            {
                ProductOptions.Remove(option);
            }

            return true;
        }
    }
}
