using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class ProductComboInfoViewModel
    {
        private IDao _dao;
        public ComboProduct Product { get; set; }
        public string OldImage { get; set; }
        public List<SingleProduct> AllSingleProducts { get; set; }
        public List<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
        public ObservableCollection<ComboItem> ObservableChildProducts { get; set; }

        public ProductComboInfoViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            var allProducts = _dao.Products.GetAll();
            AllSingleProducts = allProducts.Where(p => p is SingleProduct).Select(p => (SingleProduct)p).ToList();
            ProductOptions = _dao.ProductOptions.GetAll();
        }

        public void LoadProduct(string productId)
        {
            Product = (ComboProduct)_dao.Products.GetById(productId);
            OldImage = Product.Image;

            var childProducts = _dao.ComboItems.GetAll().Where(c => c.ComboId == Product.Id).ToList();
            Product.ChildProducts = childProducts;
            ObservableChildProducts = new ObservableCollection<ComboItem>(childProducts);

            // Set the options for each child product
            foreach (var item in ObservableChildProducts)
            {
                var options = ProductOptions.Where(o => o.ProductId == item.Product.Id).ToList();
                options.Insert(0, new ProductOption()
                {
                    OptionId = -1,
                    Name = "Không",
                    ProductId = item.Product.Id,
                    CostPrice = item.Product.CostPrice,
                    SalePrice = item.Product.SalePrice,
                });
                item.Product.Options = options;

                if (item.Option != null)
                {
                    item.Product.CostPrice = item.Option.CostPrice;
                    item.Product.SalePrice = item.Option.SalePrice;
                }
            }
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
                    Debug.WriteLine("UpdateComboProduct - Error deleting old image: " + OldImage);
                    return ResponseCode.Error;
                }
            }

            // Save the image to the folder
            if (Product.Image != OldImage)
            {
                string newImageName = FileUtils.SaveImage(Product.Image);
                if (newImageName == "")
                {
                    Debug.WriteLine("UpdateComboProduct - Error saving new image: " + Product.Image);
                    return ResponseCode.Error;
                }

                Product.Image = newImageName;
            }

            // Update the product in the database
            int result = _dao.Products.UpdateById(Product.Id.ToString(), Product);
            if (result == -1)
            {
                Debug.WriteLine("UpdateComboProduct - Error updating product basic info: " + Product.Id);
                return ResponseCode.Error;
            }

            // Update the child products in the database
            // Compare changes in ObservableChildProducts and Product.ChildProducts
            // Delete if not in ObservableChildProducts, and add if not in Product.ChildProducts, update if changed
            foreach (var item in Product.ChildProducts)
            {
                var existingItem = ObservableChildProducts.FirstOrDefault(i => i.ItemId == item.ItemId);
                if (existingItem == null)
                {
                    // Item is not in ObservableChildProducts, delete it
                    result = _dao.ComboItems.DeleteById(item.ItemId.ToString());
                    if (result == -1)
                    {
                        Debug.WriteLine("UpdateComboProduct - Error deleting combo item: " + item.ItemId);
                        return ResponseCode.Error;
                    }
                }
                else
                {
                    // Update the existing item
                    result = _dao.ComboItems.UpdateById(item.ItemId.ToString(), existingItem);
                    if (result == -1)
                    {
                        Debug.WriteLine("UpdateComboProduct - Error updating combo item: " + item.ItemId);
                        return ResponseCode.Error;
                    }
                }
            }

            foreach (var item in ObservableChildProducts)
            {
                var existingItem = Product.ChildProducts.FirstOrDefault(i => i.ItemId == item.ItemId);
                if (existingItem == null)
                {
                    // Item is not in Product.ChildProducts, add it
                    item.ComboId = Product.Id;
                    result = _dao.ComboItems.Insert(item);
                    if (result == -1)
                    {
                        Debug.WriteLine("UpdateComboProduct - Error adding new combo item: " + item.ItemId);
                        return ResponseCode.Error;
                    }
                }
            }

            return ResponseCode.Success;
        }

        internal void UpdateProductImage(string path, string fileName)
        {
            Product.Image = path;
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
                    ItemId = 0,
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

                // Update the cost price of the combo product
                CalculatePrice();
            }
        }

        public void RemoveItemFromCombo(ComboItem item)
        {
            if (ObservableChildProducts.Contains(item))
            {
                ObservableChildProducts.Remove(item);

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

        public ResponseCode UpdateCombo()
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
            if (result == 0)
            {
                return ResponseCode.Error;
            }

            int newComboId = _dao.Products.GetAll().Max(p => p.Id) + 1;

            Product.ChildProducts.ForEach((item) =>
            {
                item.ComboId = newComboId;
                result = _dao.ComboItems.Insert(item);
            });

            if (result == 0)
            {
                return ResponseCode.Error;
            }

            return ResponseCode.Error;
        }
    }
}
