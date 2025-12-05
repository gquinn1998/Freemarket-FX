using System.Collections.Concurrent;

namespace Basket.Api.MemoryStore;

public class InMemoryBasketStore : IBasketStore
{
    private readonly ConcurrentDictionary<Guid, Models.Basket> _map = new();

    public Models.Basket GetOrCreate(Guid id)
    {
        return _map.GetOrAdd(id, _ => new Models.Basket { BasketId = id });
    }

    public bool TryGet(Guid id, out Models.Basket basket) => _map.TryGetValue(id, out basket);

    public void Save(Models.Basket basket) => _map[basket.BasketId] = basket;

    public void Delete(Guid id) => _map.TryRemove(id, out _);
}
