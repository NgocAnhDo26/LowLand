using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

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

    public static class DispatcherQueueExtensions
    {
        public static Task TryEnqueueAsync(this DispatcherQueue dispatcher, Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcher.TryEnqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
