namespace Basket.Api.Dtos;

public record BasketLineDto(
    Guid ItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    bool IsDiscounted,
    decimal LineTotal
);
