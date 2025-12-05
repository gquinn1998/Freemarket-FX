namespace Basket.Api.MemoryStore;

public interface IBasketStore
{
    Models.Basket GetOrCreate(Guid id);
    bool TryGet(Guid id, out Models.Basket basket);
    void Save(Models.Basket basket);
    void Delete(Guid id);
}
