using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LowLand.View;
using LowLand.Services;
using LowLand.View.ViewModel;
using LowLand.Model.Product;
using Microsoft.UI.Xaml.Controls;

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
            var args = new NavigationViewItemInvokedEventArgs(); // Removed assignment to IsSettingsInvoked

            // Act
            dashboardPage.GetType().GetMethod("navigation_ItemInvoked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(dashboardPage, new object[] { navigationView, args });

            // Assert
            // Add assertions based on expected behavior
        }

        [TestMethod]
        public void TestMockDAO_ProductRepository_CRUD()
        {
            // Arrange
            var mockDao = new MockDAO();
            var productRepo = mockDao.Products;
            var newProduct = new Product { Id = 21, Name = "Test Product", ProductTypeId = 1, SalePrice = 100, CostPrice = 50, Image = "test.jpg", Size = "M" };

            // Act
            productRepo.Insert(newProduct);
            var insertedProduct = productRepo.GetById("21");
            insertedProduct.Name = "Updated Product";
            productRepo.UpdateById("21", insertedProduct);
            var updatedProduct = productRepo.GetById("21");
            productRepo.DeleteById("21");
            var deletedProduct = productRepo.GetById("21");

            // Assert
            Assert.IsNotNull(insertedProduct);
            Assert.AreEqual("Updated Product", updatedProduct.Name);
            Assert.IsNull(deletedProduct);
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
    }
}
