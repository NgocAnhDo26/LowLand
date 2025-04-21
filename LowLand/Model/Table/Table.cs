using System.ComponentModel;

namespace LowLand.Model.Table
{

    public partial class Table : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Capacity { get; set; }
        public object OrderId { get; internal set; }

        //    public DateTime CreatedAt { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
