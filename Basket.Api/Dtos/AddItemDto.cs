namespace Basket.Api.Dtos;

public record AddItemDto(string Sku, string Name, decimal UnitPrice, int Quantity = 1, bool IsDiscountedItem = false, decimal? DiscountedPrice = null, bool IsVatExempt = false);
