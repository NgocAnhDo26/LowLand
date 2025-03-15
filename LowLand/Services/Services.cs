using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLand.Services
{
    public class Services
    {
        static Dictionary<string, object> _singletons = new Dictionary<string, object>();
        public static void AddKeyedSingleton<IParent, Child>()
        {
            Type parent = typeof(IParent);
            Type child = typeof(Child);
            _singletons[parent.Name] = Activator.CreateInstance(child)!;
        }

        public static IParent GetKeyedSingleton<IParent>()
        {
            Type parent = typeof(IParent);
            return (IParent)_singletons[parent.Name];
        }
    }
}
