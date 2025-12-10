using Basket.Api.Models;

namespace Basket.Api.MemoryStore;

public interface IBasketStore
{
    BasketModel GetOrCreate(Guid id);
    bool TryGet(Guid id, out BasketModel basket);
    void Save(BasketModel basket);
    void Delete(Guid id);
}
