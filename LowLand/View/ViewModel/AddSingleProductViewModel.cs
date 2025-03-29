using System.Collections.ObjectModel;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class AddSingleProductViewModel
    {
        private IDao _dao;
        public SingleProduct Product { get; set; }
        public ObservableCollection<Category> Categories { get; set; }
        public AddSingleProductViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Categories = new ObservableCollection<Category>(_dao.Categories.GetAll());
            Product = new SingleProduct()
            {
                Name = "Sản phẩm mới",
            };
        }

        internal void UpdateProductImage(string path, string fileName)
        {
            Product.Image = path;
        }

        public void ChangeProductCategory(Category category)
        {
            Product.Category = category;
        }

        public ResponseCode AddProduct()
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

            // Save the image to the folder
            string newImageName = FileUtils.SaveImage(Product.Image);
            if (newImageName == "")
            {
                return ResponseCode.Error;
            }

            Product.Image = newImageName;

            // Add the product to the database
            int result = _dao.Products.Insert(Product);
            if (result != -1)
            {
                return ResponseCode.Success;
            }

            return ResponseCode.Error;
        }
    }
}
