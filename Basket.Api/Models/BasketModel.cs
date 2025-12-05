using Basket.Api.Dtos;

namespace Basket.Api.Models;

public class BasketLine
{
    public Guid LineId { get; init; } = Guid.NewGuid();
    public string Sku { get; init; }
    public string Name { get; init; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public bool IsDiscountedItem { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public bool IsVatExempt { get; set; }

    public decimal EffectiveUnitPrice => IsDiscountedItem && DiscountedPrice.HasValue ? DiscountedPrice.Value : UnitPrice;

    public decimal LineSubtotal => EffectiveUnitPrice * Quantity;
}

public class Basket
{
    public Guid BasketId { get; init; } = Guid.NewGuid();
    public List<BasketLine> Lines { get; } = new();
    public DiscountCodeDto? DiscountCode { get; set; }
    public string ShippingCountry { get; set; } = "UK";
}
