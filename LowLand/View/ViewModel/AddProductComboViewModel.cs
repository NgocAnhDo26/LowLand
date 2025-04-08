using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class AddProductComboViewModel
    {
        private IDao _dao;
        public ComboProduct Product { get; set; }
        public List<SingleProduct> AllSingleProducts { get; set; }
        public List<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
        public ObservableCollection<ComboItem> ObservableChildProducts { get; set; }

        public AddProductComboViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            var allProducts = _dao.Products.GetAll();
            AllSingleProducts = allProducts.Where(p => p is SingleProduct).Select(p => (SingleProduct)p).ToList();
            ProductOptions = _dao.ProductOptions.GetAll();

            ObservableChildProducts = new ObservableCollection<ComboItem>();
            Product = new ComboProduct()
            {
                Name = "Combo mới",
                CostPrice = 0,
                SalePrice = 0,
                ChildProducts = [],
            };
        }

        public List<string> SearchForProducts(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                // Return all products when prompt is empty
                return AllSingleProducts
                    .Select(item => $"{item.Id} - {item.Name}")
                    .ToList();
            }

            // Use StringComparison for case-insensitive comparison
            var results = AllSingleProducts
                .Where(item => item.Name.Contains(prompt, StringComparison.OrdinalIgnoreCase))
                .Select(item => $"{item.Id} - {item.Name}")
                .ToList();

            // Return results or a message if none found
            return results.Count > 0
                ? results
                : new List<string> { "Không tìm thấy sản phẩm nào..." };
        }

        public void CalculatePrice()
        {
            // Calculate the total cost price of the combo
            int totalCostPrice = 0;
            int originalSalePrice = 0;
            foreach (var item in ObservableChildProducts)
            {
                totalCostPrice += item.Product.CostPrice * item.Quantity;
                originalSalePrice += item.Product.SalePrice * item.Quantity;
            }

            Product.CostPrice = totalCostPrice;
            Product.OriginalSalePrice = originalSalePrice;
        }

        internal void UpdateProductImage(string path, string fileName)
        {
            Product.Image = path;
        }

        public void AddProductToCombo(int productId)
        {
            var product = AllSingleProducts.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                // Add options to the product
                var options = ProductOptions.Where(o => o.ProductId == productId).ToList();
                options.Insert(0, new ProductOption()
                {
                    OptionId = -1,
                    Name = "Không",
                    ProductId = productId,
                    CostPrice = product.CostPrice,
                    SalePrice = product.SalePrice,
                });

                // Add the new item to the combo
                ComboItem item = new ComboItem()
                {
                    ItemId = ObservableChildProducts.Count > 0 ? ObservableChildProducts.Max(p => p.ItemId) + 1 : 1,
                    Product = new SingleProduct()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Category = product.Category,
                        CostPrice = product.CostPrice,
                        SalePrice = product.SalePrice,
                        Options = options,
                    },
                    Quantity = 1,
                };

                ObservableChildProducts.Add(item);
                Product.ChildProducts.Add(item);

                // Update the cost price of the combo product
                CalculatePrice();
            }
        }

        public void RemoveItemFromCombo(ComboItem item)
        {
            if (ObservableChildProducts.Contains(item))
            {
                ObservableChildProducts.Remove(item);
                Product.ChildProducts.Remove(item);

                // Update the cost price of the combo product
                CalculatePrice();
            }
        }

        public void SetProductOption(ComboItem item, ProductOption option)
        {
            if (item == null || option == null)
            {
                return;
            }

            // Set the price of the product based on the selected option
            item.Option = option.OptionId == -1 ? null : option;
            item.Product.CostPrice = option.CostPrice;
            item.Product.SalePrice = option.SalePrice;

            // Update the cost price of the combo product
            CalculatePrice();
        }

        public ResponseCode AddCombo()
        {
            // Check if the combo name is empty
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                return ResponseCode.EmptyName;
            }

            // Check if the combo has at least one product
            if (Product.ChildProducts.Count == 0)
            {
                return ResponseCode.NoChildProduct;
            }

            // Check if the sale price is lower than cost price
            if (Product.SalePrice < Product.CostPrice)
            {
                return ResponseCode.InvalidValue;
            }

            // Add the combo to the database
            int result;

            result = _dao.Products.Insert(Product);
            int newComboId = _dao.Products.GetAll().Max(p => p.Id);

            Product.ChildProducts.ForEach((item) =>
            {
                item.ComboId = newComboId;
                result = _dao.ComboItems.Insert(item);
            });

            if (result == -1)
            {
                return ResponseCode.Error;
            }

            return ResponseCode.Success;
        }
    }
}
