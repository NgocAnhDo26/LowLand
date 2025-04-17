using System.Collections.Generic;
using LowLand.Model;

namespace LowLand.Services
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        PagedResult<T> GetAll(int pageNumber, int pageSize, string? keyword = null);
        T GetById(string id);
        int Insert(T info);
        int DeleteById(string id);
        int UpdateById(string id, T info);
    }
}
