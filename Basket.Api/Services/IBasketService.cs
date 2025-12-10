using Basket.Api.Dtos;
using Basket.Api.Models;

namespace Basket.Api.Services;

public interface IBasketService
{
    BasketModel GetOrCreate(Guid id);
    void AddItem(Guid basketId, AddItemDto dto);
    void AddItems(Guid basketId, IEnumerable<AddItemDto> items);
    bool RemoveLine(Guid basketId, Guid lineId);
    void SetDiscountCode(Guid basketId, DiscountCodeDto code);
    void SetShippingCountry(Guid basketId, string country);
    BasketTotalsModel GetTotals(Guid basketId, bool includeVat);
    bool TryGetBasket(Guid basketId, out BasketModel basket);
}
