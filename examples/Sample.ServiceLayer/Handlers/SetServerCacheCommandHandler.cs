using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agatha.ServiceLayer;
using Sample.Common.RequestsAndResponses;

namespace Sample.ServiceLayer.Handlers
{
    public static class FakeCacheStore
    {
        private static object _syncLock = new object();

        private static Dictionary<string, string> _items = new Dictionary<string, string>();

        public static void Add(string key, string value)
        {
            lock (_syncLock)
            {
                if (!_items.ContainsKey(key))
                    _items.Add(key, value);
                else
                    _items[key] = value;
            }
        }

        public static string Get(string key)
        {
            lock (_syncLock)
            {
                if (!_items.ContainsKey(key))
                    return string.Empty;
                return _items[key];
            }
        }
    }

    public class SetServerCacheCommandHandler : OneWayRequestHandler<SetCacheCommand>
    {
        public override void Handle(SetCacheCommand request)
        {
            FakeCacheStore.Add(request.CacheKey, request.CacheValue);
        }
    }
}
