using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowLand.Model;

namespace LowLand.Services
{
    public interface IDao
    {
        IRepository<Drink> Drinks { get; set; }
    }
}
