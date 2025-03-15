using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Services
{
    public interface IRepository<T>
    {
        List<T> GetAll(); // Phân trang, Sắp xếp, Lọc, Tìm kiếm
        T GetById(string id);
        int Insert(T info);
        int DeleteById(string id);
        int UpdateById(string id, T info);
    }
}
