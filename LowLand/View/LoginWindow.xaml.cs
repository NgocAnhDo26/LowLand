using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Npgsql;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LowLand.Services;
using LowLand.Model.Customer;
using System.Diagnostics;
using DemoListBinding.View.ViewModel;
using System.Security.Cryptography;
using System.Text;
using LowLand.Model.Product;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginWindow : Window
    {
        //  private PostgreDAO _dao;
        public LoginViewModel ViewModel { get; set; }
        public LoginWindow()
        {
            ViewModel = new LoginViewModel();
            this.InitializeComponent();
            SystemBackdrop = new DesktopAcrylicBackdrop();
            //  _dao = new PostgreDAO();
            //test();

            var localStorage = Windows.Storage.ApplicationData.Current.LocalSettings;
            var username = (string)localStorage.Values["Username"];
            var encryptedInBase64 = (string)localStorage.Values["Password"];
            var entropyInBase64 = (string)localStorage.Values["Entropy"];

            if (username == null) return;

            var encryptedInBytes = Convert.FromBase64String(encryptedInBase64);
            var entropyInBytes = Convert.FromBase64String(entropyInBase64);

            var passwordInBytes = ProtectedData.Unprotect(
                encryptedInBytes,
                entropyInBytes,
                DataProtectionScope.CurrentUser
            );
            var password = Encoding.UTF8.GetString(passwordInBytes);
            ViewModel.Password = password;
            ViewModel.Username = username;
        }
        static void test()
        {
            ProductRepository repository = new ProductRepository();

            // ========== Test GetAll ==========
            Debug.WriteLine("===== GetAll Products =====");
            List<Product> products = repository.GetAll();
            foreach (var p in products)
            {
                Debug.WriteLine($"ID: {p.Id}, Name: {p.Name}, Price: {p.SalePrice}");
            }

            // ========== Test GetById ==========
            string testId = "999";
            Debug.WriteLine($"\n===== GetById ({testId}) =====");
            Product product = repository.GetById(testId);
            if (product != null)
            {
                Debug.WriteLine($"ID: {product.Id}, Name: {product.Name}, Price: {product.SalePrice}");
            }
            else
            {
                Debug.WriteLine("Product not found!");
            }

            // ========== Test Insert ==========
            Debug.WriteLine("\n===== Insert New Product =====");
            SingleProduct newProduct = new SingleProduct
            {
             
                Name = "New Product",
                SalePrice = 1999,
                CostPrice = 1050,
                Image = "new_product.jpg",
                Category = new Category { Id = 1, Name = "Electronics" },
                ProductType = new ProductType { Id = 1, Name = "Gadget", Category = new Category { Id = 1, Name = "Electronics" } }
            };
            int insertResult = repository.Insert(newProduct);
            Debug.WriteLine($"Insert Result: {insertResult}");

            // ========== Test Update ==========
            Debug.WriteLine("\n===== Update Product (1000) =====");
            newProduct.Name = "Updated Product Name";
            newProduct.SalePrice = 2499;
            int updateResult = repository.UpdateById("2", newProduct);
            Debug.WriteLine($"Update Result: {updateResult}");

            // ========== Test Delete ==========
            Debug.WriteLine("\n===== Delete Product (1000) =====");
            int deleteResult = repository.DeleteById("3");
            Debug.WriteLine($"Delete Result: {deleteResult}");

            Debug.WriteLine("\n===== Test Completed! =====");

        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ViewModel.Username + " " + ViewModel.Password);

            if (ViewModel.CanLogin())
            { // CanExecute - Look before you leap
                bool success = ViewModel.Login(); // Execute

                if (ViewModel.RememberMe == true)
                {
                    var passwordInBytes = Encoding.UTF8.GetBytes(ViewModel.Password);
                    var entropyInBytes = new byte[20];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(entropyInBytes);
                    }
                    var encryptedInBytes = ProtectedData.Protect(
                        passwordInBytes,
                        entropyInBytes,
                        DataProtectionScope.CurrentUser
                    );
                    var encryptedInBase64 = Convert.ToBase64String(encryptedInBytes);
                    var entropyInBase64 = Convert.ToBase64String(entropyInBytes);

                    var localStorage = Windows.Storage.ApplicationData.Current.LocalSettings;
                    localStorage.Values["Username"] = ViewModel.Username;
                    localStorage.Values["Password"] = encryptedInBase64;
                    localStorage.Values["Entropy"] = entropyInBase64;

                    Debug.WriteLine($"Encrypted password in base 64 is: {encryptedInBase64}");
                }

                if (success)
                {
                    var screen = new DashboardWindow();
                    screen.Activate();

                    this.Close();
                }
            }
        }
      
    }
}
