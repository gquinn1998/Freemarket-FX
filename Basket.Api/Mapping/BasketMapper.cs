using Basket.Api.Dtos;
using Basket.Api.Models;

namespace Basket.Api.Mapping;

public static class BasketMapper
{
    public static BasketResponseDto ToDto(this Models.Basket basket)
    {
        return new BasketResponseDto(
            BasketId: basket.BasketId,
            Lines: basket.Lines.Select(l => new BasketLineDto(
                ItemId: l.LineId,
                Name: l.Name,
                Quantity: l.Quantity,
                UnitPrice: l.UnitPrice,
                IsDiscounted: l.IsDiscountedItem,
                LineTotal: l.LineSubtotal
            )),
            Totals: new BasketTotalsDto(
            SubtotalExcludingVat: basket.Lines.Sum(l => l.LineSubtotal),
            VatAmount: basket.Lines.Where(l => !l.IsVatExempt).Sum(l => l.LineSubtotal * 0.20m),
            TotalIncludingVat: basket.Lines.Sum(l => l.LineSubtotal) * 1.20m,
            ShippingCost: 0m
        )
        );
    }
}
