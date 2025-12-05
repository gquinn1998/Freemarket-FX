namespace Basket.Api.Dtos;

public record BasketResponseDto(
    Guid BasketId,
    IEnumerable<BasketLineDto> Lines,
    BasketTotalsDto Totals
);
