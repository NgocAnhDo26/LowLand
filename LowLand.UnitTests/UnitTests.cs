using System;
using System.Collections.Generic;
using LowLand.Model.Product;
using LowLand.Services;
using LowLand.View;
using LowLand.View.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LowLand.UnitTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestDashboardPageNavigation_ItemInvoked()
        {
            // Arrange
            var dashboardPage = new DashboardPage();
            var navigationView = new NavigationView();
            var navigationViewItem = new NavigationViewItem { Tag = "DashboardPage" };
            navigationView.SelectedItem = navigationViewItem;
            var args = new NavigationViewItemInvokedEventArgs();
            bool navigationCalled = false;

            // Mock the Frame navigation
            dashboardPage.Frame = new Frame();
            dashboardPage.Frame.GetType()
                .GetProperty("NavigationCalled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(dashboardPage.Frame, (Action)(() => navigationCalled = true));

            // Act
            dashboardPage.GetType().GetMethod("navigation_ItemInvoked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dashboardPage, new object[] { navigationView, args });

            // Assert
            Assert.IsTrue(navigationCalled, "Navigation should have been called");
        }

        [TestMethod]
        public void TestMockDAO_ProductRepository_CRUD()
        {
            // Arrange
            var mockDao = new MockDAO();
            var productRepo = mockDao.Products;
            var newProduct = new SingleProduct
            {
                Id = 21,
                Name = "Test Product",
                ProductType = new ProductType { Id = 1, Name = "Cà Phê" },
                Category = new Category { Id = 1, Name = "Thức uống" },
                SalePrice = 100,
                CostPrice = 50,
                Image = "test.jpg"
            };

            // Act
            productRepo.Insert(newProduct);
            var insertedProduct = productRepo.GetById("21");
            insertedProduct.Name = "Updated Product";
            productRepo.UpdateById("21", insertedProduct);
            var updatedProduct = productRepo.GetById("21");
            productRepo.DeleteById("21");
            var deletedProduct = productRepo.GetById("21");

            // Assert
            Assert.IsNotNull(insertedProduct, "Inserted product should not be null");
            Assert.AreEqual("Updated Product", updatedProduct.Name, "Product name should be updated");
            Assert.IsNull(deletedProduct, "Deleted product should be null");
        }

        [TestMethod]
        public void TestProductsViewModelInitialization()
        {
            // Arrange
            var viewModel = new ProductsViewModel();

            // Act
            var products = viewModel.Products;

            // Assert
            Assert.IsNotNull(products);
            Assert.IsTrue(products.Count > 0);
        }

        [TestMethod]
        public void TestProductsPage_ViewDetails_Click()
        {
            // Arrange
            var productsPage = new ProductsPage();
            var menuFlyoutItem = new MenuFlyoutItem();
            var testProduct = new SingleProduct()
            {
                Id = 1,
                ProductType = new ProductType()
                {
                    Id = 1,
                    Name = "Cà Phê",
                    Category = new Category() { Id = 1, Name = "Thức uống" }
                },
                Category = new Category() { Id = 1, Name = "Thức uống" },
                Name = "Phin Sữa Đá",
                SalePrice = 29000,
                CostPrice = 15000,
                Image = "phin_sua_da.jpg"
            };
            menuFlyoutItem.DataContext = testProduct;
            bool navigationCalled = false;

            // Mock the Frame navigation
            productsPage.Frame = new Frame();
            productsPage.Frame.GetType()
                .GetProperty("NavigationCalled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(productsPage.Frame, (Action)(() => navigationCalled = true));

            // Act
            var viewDetailsMethod = productsPage.GetType().GetMethod("ViewDetails_Click",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            viewDetailsMethod?.Invoke(productsPage, new object[] { menuFlyoutItem, new RoutedEventArgs() });

            // Assert
            Assert.IsTrue(navigationCalled, "Navigation to product details should have been called");
        }

        [TestMethod]
        public void TestProductsPage_DeleteProduct_Confirmation()
        {
            // Arrange
            var productsPage = new ProductsPage();
            var viewModel = new ProductsViewModel();
            productsPage.DataContext = viewModel;
            var testProduct = new SingleProduct { Id = 1, Name = "Test Product" };
            var menuFlyoutItem = new MenuFlyoutItem { DataContext = testProduct };
            bool dialogShown = false;

            // Mock dialog showing
            productsPage.GetType()
                .GetProperty("ShowDialogCalled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(productsPage, (Action)(() => dialogShown = true));

            // Act
            var deleteMethod = productsPage.GetType().GetMethod("DeleteProduct_Click",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            deleteMethod?.Invoke(productsPage, new object[] { menuFlyoutItem, new RoutedEventArgs() });

            // Assert
            Assert.IsTrue(dialogShown, "Confirmation dialog should have been shown");
        }

        [TestMethod]
        public void TestProductInfoPage_LoadProduct()
        {
            // Arrange
            var productInfoPage = new ProductInfoPage();
            var productId = "1";
            var navigationArgs = new NavigationEventArgs
            {
                Parameter = productId
            };

            // Act
            productInfoPage.GetType().GetMethod("OnNavigatedTo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(productInfoPage, new object[] { navigationArgs });

            // Assert
            var viewModel = productInfoPage.DataContext as ProductInfoViewModel;
            Assert.IsNotNull(viewModel, "ViewModel should not be null");
            Assert.IsNotNull(viewModel.Product, "Product should not be null");
            Assert.AreEqual(productId, viewModel.Product.Id.ToString(), "Product ID should match");
        }

        [TestMethod]
        public void TestProductInfoPage_CategoryChange()
        {
            // Arrange
            var productInfoPage = new ProductInfoPage();
            var viewModel = new ProductInfoViewModel();
            productInfoPage.DataContext = viewModel;
            var testCategory = new Category { Id = 1, Name = "Test Category" };
            var comboBox = new ComboBox { SelectedItem = testCategory };

            // Act
            var categoryChangeMethod = productInfoPage.GetType().GetMethod("Category_SelectionChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            categoryChangeMethod?.Invoke(productInfoPage, new object[] { comboBox, new SelectionChangedEventArgs(new List<object>(), new List<object>()) });

            // Assert
            Assert.IsNotNull(viewModel.FilteredProductTypes, "Filtered product types should not be null");
        }

        [TestMethod]
        public async void TestProductInfoPage_UpdateProduct()
        {
            // Arrange
            var productInfoPage = new ProductInfoPage();
            var viewModel = new ProductInfoViewModel();
            productInfoPage.DataContext = viewModel;
            bool updateCalled = false;

            // Mock the update operation
            viewModel.GetType()
                .GetProperty("UpdateCalled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(viewModel, (Action)(() => updateCalled = true));

            // Act
            var updateMethod = productInfoPage.GetType().GetMethod("ApplyButton_Click",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            updateMethod?.Invoke(productInfoPage, new object[] { null, new RoutedEventArgs() });

            // Assert
            Assert.IsTrue(updateCalled, "Product update should have been called");
        }
    }
}
