namespace Basket.Api.Dtos;

public record BasketTotalsDto(
    decimal SubtotalExcludingVat,
    decimal VatAmount,
    decimal TotalIncludingVat,
    decimal ShippingCost
);
