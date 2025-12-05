using Basket.Api.Dtos;
using Basket.Api.MemoryStore;
using Basket.Api.Models;

namespace Basket.Api.Services;

public class BasketService : IBasketService
{
    private readonly IBasketStore _store;
    public BasketService(IBasketStore store) => _store = store;

    public Models.Basket GetOrCreate(Guid id) => _store.GetOrCreate(id);

    public void AddItem(Guid basketId, AddItemDto dto)
    {
        var basket = _store.GetOrCreate(basketId);

        // try to find existing line with same SKU and same effective price
        var existing = basket.Lines.FirstOrDefault(l => l.Sku == dto.Sku && l.EffectiveUnitPrice == (dto.IsDiscountedItem && dto.DiscountedPrice.HasValue ? dto.DiscountedPrice.Value : dto.UnitPrice));
        
        if (existing != null) existing.Quantity += dto.Quantity;
        else
        {
            basket.Lines.Add(new BasketLine
            {
                Sku = dto.Sku,
                Name = dto.Name,
                UnitPrice = dto.UnitPrice,
                Quantity = dto.Quantity,
                IsDiscountedItem = dto.IsDiscountedItem,
                DiscountedPrice = dto.DiscountedPrice,
                IsVatExempt = dto.IsVatExempt
            });
        }

        _store.Save(basket);
    }

    public void AddItems(Guid basketId, IEnumerable<AddItemDto> items)
    {
        foreach (var i in items) AddItem(basketId, i);
    }

    public bool RemoveLine(Guid basketId, Guid lineId)
    {
        if (!_store.TryGet(basketId, out var basket)) return false;

        var line = basket.Lines.FirstOrDefault(l => l.LineId == lineId);
        
        if (line == null) return false;
        
        basket.Lines.Remove(line);
        _store.Save(basket);
        
        return true;
    }

    public void SetDiscountCode(Guid basketId, DiscountCodeDto code)
    {
        var basket = _store.GetOrCreate(basketId);

        basket.DiscountCode = code;
        _store.Save(basket);
    }

    public void SetShippingCountry(Guid basketId, string country)
    {
        var basket = _store.GetOrCreate(basketId);

        basket.ShippingCountry = country;
        _store.Save(basket);
    }

    public BasketTotalsModel GetTotals(Guid basketId, bool includeVat)
    {
        if (!_store.TryGet(basketId, out var basket)) throw new KeyNotFoundException("Basket not found");

        decimal subtotal = 0m;
        decimal discountedItemsTotal = 0m;
        decimal nonDiscountedTotal = 0m;

        foreach (var line in basket.Lines)
        {
            subtotal += line.LineSubtotal;
            if (line.IsDiscountedItem) discountedItemsTotal += line.LineSubtotal;
            else nonDiscountedTotal += line.LineSubtotal;
        }

        decimal discountFromCode = 0m;
        if (basket.DiscountCode != null)
        {
            // discount applies only to non-discounted items
            discountFromCode = Math.Round(nonDiscountedTotal * (basket.DiscountCode.Percent / 100m), 2, MidpointRounding.AwayFromZero);
        }

        decimal subtotalAfterCode = subtotal - discountFromCode;

        decimal shipping = ComputeShipping(basket.ShippingCountry, subtotalAfterCode);

        // VAT: compute VATable amounts (assume all items are VATable except IsVatExempt lines). For shipping assume it's VATable.
        decimal vat = 0m;
        // compute VAT on item lines
        foreach (var line in basket.Lines)
        {
            var lineTotal = line.LineSubtotal;
            // if discount code applies to this line (i.e., not IsDiscountedItem), proportionally reduce VAT base
            decimal lineDiscount = 0m;
            if (basket.DiscountCode != null && !line.IsDiscountedItem && nonDiscountedTotal > 0)
            {
                // proportional share of code discount
                lineDiscount = Math.Round(discountFromCode * (line.LineSubtotal / nonDiscountedTotal), 2, MidpointRounding.AwayFromZero);
            }
            var lineTaxable = line.IsVatExempt ? 0m : (lineTotal - lineDiscount);
            vat += Math.Round(lineTaxable * 0.20m, 2, MidpointRounding.AwayFromZero);
        }
        // shipping VAT
        vat += Math.Round(shipping * 0.20m, 2, MidpointRounding.AwayFromZero);

        decimal total = subtotalAfterCode + shipping + (includeVat ? vat : 0m);

        return new BasketTotalsModel
        {
            SubtotalExcludingDiscounts = subtotal,
            DiscountFromCode = discountFromCode,
            Shipping = shipping,
            Vat = includeVat ? vat : 0m,
            Total = total
        };
    }

    public bool TryGetBasket(Guid basketId, out Models.Basket basket) => _store.TryGet(basketId, out basket);


    private decimal ComputeShipping(string country, decimal subtotalAfterDiscount)
    {
        if (string.Equals(country ?? "UK", "UK", StringComparison.OrdinalIgnoreCase))
        {
            if (subtotalAfterDiscount >= 50m) return 0m;
            return 5m;
        }
        return 15m; // flat for other countries
    }
}
