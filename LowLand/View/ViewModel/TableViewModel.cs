using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LowLand.Model.Table;
using LowLand.Services;
using LowLand.Utils;

namespace LowLand.View.ViewModel
{
    public class TableViewModel : INotifyPropertyChanged
    {
        private readonly IDao _dao;

        public ObservableCollection<Table> Tables { get; set; }
        public Table SelectedTable { get; set; } = new Table();

        public TableViewModel()
        {
            _dao = Services.Services.GetKeyedSingleton<IDao>();
            Tables = new ObservableCollection<Table>(_dao.Tables.GetAll());
        }

        public void AddTable(Table newTable)
        {
            int newId = _dao.Tables.Insert(newTable);
            if (newId > 0)
            {
                newTable.Id = newId;
                Tables.Add(newTable);
            }
        }


        public void UpdateTable(Table updatedTable)
        {
            if (!TableStatuses.All.Contains(updatedTable.Status))
                return;

            int result = _dao.Tables.UpdateById(updatedTable.Id.ToString(), updatedTable);
            if (result == 1)
            {
                var index = Tables.IndexOf(Tables.FirstOrDefault(t => t.Id == updatedTable.Id));
                if (index != -1)
                {
                    Tables[index] = updatedTable;
                }
            }


        }

        public void DeleteTable(int tableId)
        {
            int result = _dao.Tables.DeleteById(tableId.ToString());
            if (result == 1)
            {
                var table = Tables.FirstOrDefault(t => t.Id == tableId);
                if (table != null)
                {
                    Tables.Remove(table);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
