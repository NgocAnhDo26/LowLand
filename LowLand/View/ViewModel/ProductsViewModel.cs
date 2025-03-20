using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    }
}
