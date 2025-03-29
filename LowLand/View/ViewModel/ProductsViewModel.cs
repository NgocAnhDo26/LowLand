using System.Linq;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class ProductsViewModel
    {
        private IDao _dao;
        public FullObservableCollection<Product> Products { get; set; }

        public ProductsViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Products = new FullObservableCollection<Product>(_dao.Products.GetAll());
        }

        public ResponseCode RemoveProduct(int productId)
        {
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                int result = _dao.Products.DeleteById(productId.ToString());

                if (result != -1)
                {
                    // Remove the product + image from the folder
                    Products.Remove(product);
                    if (!FileUtils.DeleteImage(product.Image))
                    {
                        return ResponseCode.Error;
                    }

                    return ResponseCode.Success;
                }
            }

            return ResponseCode.NotFound;
        }

    }
}
