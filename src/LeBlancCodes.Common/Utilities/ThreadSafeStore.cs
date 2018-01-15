using System;
using System.Collections.Concurrent;

namespace LeBlancCodes.Common.Utilities
{
    public class ThreadSafeStore<TKey, TValue>
    {
        private readonly Func<TKey, TValue> _creator;
        private readonly ConcurrentDictionary<TKey, TValue> _store = new ConcurrentDictionary<TKey, TValue>();

        public ThreadSafeStore(Func<TKey, TValue> creator) => _creator = Ensure.NotNull(creator, nameof(creator));

        public TValue Get(TKey key) => _store.GetOrAdd(key, _creator);
    }
}
