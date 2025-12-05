using Basket.Api.Dtos;
using Basket.Api.Mapping;
using Basket.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{v:apiVersion}/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _service;

    public BasketController(IBasketService service) => _service = service;

    /// <summary>
    /// Creates a new basket
    /// </summary>
    [HttpPost]
    public IActionResult Create()
    {
        var basket = _service.GetOrCreate(Guid.NewGuid());

        return CreatedAtAction(nameof(Get), new { basketId = basket.BasketId }, basket.ToDto());
    }

    /// <summary>
    /// Get basket by ID
    /// </summary>
    [HttpGet("{basketId:guid}")]
    public IActionResult Get(Guid basketId)
    {
        if (!_service.TryGetBasket(basketId, out var basket))
            return NotFound(new { message = "Basket not found" });

        return Ok(basket.ToDto());
    }

    /// <summary>
    /// Add a single item to basket
    /// </summary>
    [HttpPost("{basketId:guid}/items")]
    public IActionResult AddItem(Guid basketId, AddItemDto dto)
    {
        _service.AddItem(basketId, dto);

        if (!_service.TryGetBasket(basketId, out var updated))
            return NotFound(new { message = "Basket not found after adding item" });

        return Ok(updated.ToDto());
    }

    /// <summary>
    /// Add multiple items to basket
    /// </summary>
    [HttpPost("{basketId:guid}/items/bulk")]
    public IActionResult AddItems(Guid basketId, [FromBody] List<AddItemDto> items)
    {
        _service.AddItems(basketId, items);

        if (!_service.TryGetBasket(basketId, out var updated))
            return NotFound(new { message = "Basket not found after adding items" });

        return Ok(updated.ToDto());
    }

    /// <summary>
    /// Remove an item from basket
    /// </summary>
    [HttpDelete("{basketId:guid}/items/{lineId:guid}")]
    public IActionResult RemoveItem(Guid basketId, Guid lineId)
    {
        var removed = _service.RemoveLine(basketId, lineId);

        if (!removed) return NotFound(new { message = "Item not found" });

        if (!_service.TryGetBasket(basketId, out var updated))
            return NotFound(new { message = "Basket not found after removing item" });

        return Ok(updated.ToDto());
    }

    /// <summary>
    /// Apply a discount code to basket
    /// </summary>
    [HttpPost("{basketId:guid}/discount")]
    public IActionResult AddDiscount(Guid basketId, DiscountCodeDto dto)
    {
        _service.SetDiscountCode(basketId, dto);

        if (!_service.TryGetBasket(basketId, out var updated))
            return NotFound(new { message = "Basket not found after applying discount" });

        return Ok(updated.ToDto());
    }

    /// <summary>
    /// Set shipping country
    /// </summary>
    [HttpPost("{basketId:guid}/shipping")]
    public IActionResult SetShipping(Guid basketId, ShippingDto dto)
    {
        _service.SetShippingCountry(basketId, dto.Country);

        if (!_service.TryGetBasket(basketId, out var updated))
            return NotFound(new { message = "Basket not found after setting shipping" });

        return Ok(updated.ToDto());
    }

    /// <summary>
    /// Get totals for a basket
    /// </summary>
    [HttpGet("{basketId:guid}/total")]
    public IActionResult GetTotals(Guid basketId, [FromQuery] bool includeVat = true)
    {
        if (!_service.TryGetBasket(basketId, out var basket))
            return NotFound(new { message = "Basket not found" });

        var totals = _service.GetTotals(basketId, includeVat);

        var totalsDto = new BasketTotalsDto(
            SubtotalExcludingVat: totals.SubtotalExcludingDiscounts,
            VatAmount: totals.Vat,
            TotalIncludingVat: totals.Total,
            ShippingCost: totals.Shipping
        );

        return Ok(totalsDto);
    }
}
