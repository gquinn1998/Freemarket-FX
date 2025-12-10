using Basket.Api.Models;
using System.Collections.Concurrent;

namespace Basket.Api.MemoryStore;

public class InMemoryBasketStore : IBasketStore
{
    private readonly ConcurrentDictionary<Guid, BasketModel> _map = new();

    public BasketModel GetOrCreate(Guid id)
    {
        return _map.GetOrAdd(id, _ => new BasketModel { BasketId = id });
    }

    public bool TryGet(Guid id, out BasketModel basket) => _map.TryGetValue(id, out basket);

    public void Save(BasketModel basket) => _map[basket.BasketId] = basket;

    public void Delete(Guid id) => _map.TryRemove(id, out _);
}
