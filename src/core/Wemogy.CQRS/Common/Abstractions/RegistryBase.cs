using System.Collections.Concurrent;

namespace Wemogy.CQRS.Common.Abstractions;

public abstract class RegistryBase<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _entriesCache = new ConcurrentDictionary<TKey, TValue>();

    protected TValue GetRegistryEntry(TKey key)
    {
        return _entriesCache.GetOrAdd(key, InitializeEntry);
    }

    protected abstract TValue InitializeEntry(TKey key);
}
