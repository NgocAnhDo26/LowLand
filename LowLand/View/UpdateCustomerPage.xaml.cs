using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using LowLand.View.ViewModel;
using LowLand.Model.Customer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LowLand.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateCustomerPage : Page
    {
        private CustomerViewModel ViewModel = new CustomerViewModel();
        private Customer EditingCustomer;

        public UpdateCustomerPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is int customerId)
            {
                EditingCustomer = ViewModel.Customers.FirstOrDefault(c => c.Id == customerId);
                if (EditingCustomer != null)
                {
                    NameBox.Text = EditingCustomer.Name;
                    PhoneBox.Text = EditingCustomer.Phone;
                    PointBox.Text = EditingCustomer.Point.ToString();
                    RankNameBox.Text = EditingCustomer.RankName;
                }
            }
        }



        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditingCustomer != null)
            {
                EditingCustomer.Name = NameBox.Text;
                EditingCustomer.Phone = PhoneBox.Text;
                EditingCustomer.Point = int.TryParse(PointBox.Text, out int point) ? point : 0;

                ViewModel.Update(EditingCustomer);
            }

            Frame.GoBack();
        

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
