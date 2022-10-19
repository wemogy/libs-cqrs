using System.Collections.Concurrent;

namespace Wemogy.CQRS.Common.Abstractions;

public abstract class RegistryBase<TKey, TValue>
{
    private static readonly ConcurrentDictionary<TKey, TValue> EntriesCache = new ConcurrentDictionary<TKey, TValue>();

    protected TValue GetRegistryEntry(TKey key)
    {
        return EntriesCache.GetOrAdd(key, InitializeEntry);
    }

    protected abstract TValue InitializeEntry(TKey key);
}
