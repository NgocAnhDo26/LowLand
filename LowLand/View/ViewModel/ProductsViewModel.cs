using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class ProductsViewModel : INotifyPropertyChanged
    {
        private IDao _dao;
        //  public FullObservableCollection<Product> Products { get; set; }
        private readonly PagingViewModel<Product> _paging;
        public event PropertyChangedEventHandler PropertyChanged;
        public PagingViewModel<Product> Paging => _paging;

        public ProductsViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            try
            {
                _paging = new PagingViewModel<Product>(
                    async (page, size, keyword) => await Task.Run(() => _dao.Products.GetAll(page, size, keyword)),
                    pageSize: 10
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProductsViewModel: Failed to initialize Paging - {ex.Message}");
                throw;
            }
        }

        public async Task<ResponseCode> RemoveProduct(int productId)
        {
            bool isChildProduct = _dao.ComboItems.GetAll()
                .Any(item => item.Product.Id == productId);

            if (isChildProduct)
            {
                return ResponseCode.ItemHaveDependency;
            }

            //var product = Products.FirstOrDefault(p => p.Id == productId);
            var product = _dao.Products.GetById(productId.ToString());
            if (product != null)
            {
                // If the product is a combo, remove all child products
                if (product is ComboProduct)
                {
                    var childProducts = _dao.ComboItems.GetAll()
                        .Where(item => item.ComboId == productId)
                        .ToList();

                    foreach (var item in childProducts)
                    {
                        _dao.ComboItems.DeleteById(item.ItemId.ToString());
                    }
                }

                int result = await Task.Run(() => _dao.Products.DeleteById(productId.ToString()));

                if (result != -1)
                {
                    // Remove the product + image from the folder

                    if (!FileUtils.DeleteImage(product.Image))
                    {
                        return ResponseCode.Error;
                    }
                    await _paging.RefreshAsync();
                }

                return ResponseCode.Success;
            }
            return ResponseCode.NotFound;
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
