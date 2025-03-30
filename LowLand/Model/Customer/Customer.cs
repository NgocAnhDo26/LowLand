using System;
using System.ComponentModel;

namespace LowLand.Model.Customer
{
    public class Customer : INotifyPropertyChanged
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        public string? Phone { get; set; }
        required public int Point { get; set; } = 0;
        required public DateOnly RegistrationDate { get; set; }

        // Rank Details
        public required CustomerRank Rank { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
