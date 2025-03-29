using System.Collections.Generic;

namespace LowLand.Services
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        T GetById(string id);
        int Insert(T info);
        int DeleteById(string id);
        int UpdateById(string id, T info);
    }
}
