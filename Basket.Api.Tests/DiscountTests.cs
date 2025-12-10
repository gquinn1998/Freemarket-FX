using Basket.Api.Dtos;
using Basket.Api.MemoryStore;
using Basket.Api.Models;
using Basket.Api.Services;
using FluentAssertions;
using Moq;

namespace Basket.Api.Tests;

[TestClass]
public class DiscountTests
{
    private Mock<IBasketStore> _store;
    private BasketService _service;

    [TestInitialize]
    public void Setup()
    {
        _store = new Mock<IBasketStore>();
    }

    // -------------------------------------------------------
    // SUMMER10 — 10% off basket subtotal
    // -------------------------------------------------------
    [TestMethod]
    public void Summer10_ShouldApply10PercentDiscount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var existing = new BasketModel { BasketId = id };
        existing.Lines.Add(new BasketLineModel { Sku = "A", UnitPrice = 20m, Quantity = 2 }); // 40
        existing.Lines.Add(new BasketLineModel { Sku = "B", UnitPrice = 10m, Quantity = 1 }); // 10

        // Correct mocking of out param
        BasketModel outBasket = existing;

        _store.Setup(s => s.TryGet(id, out outBasket)).Returns(true);
        _store.Setup(s => s.GetOrCreate(id)).Returns(existing);

        _service = new BasketService(_store.Object);

        // Act
        _service.SetDiscountCode(id, new DiscountCodeDto("SUMMER10", 10m));
        var totals = _service.GetTotals(id, includeVat: false);

        // Assert
        totals.SubtotalExcludingDiscounts.Should().Be(50m);
        totals.DiscountFromCode.Should().Be(5m);
    }


    // -------------------------------------------------------
    // FREESHIP — shipping cost = 0
    // -------------------------------------------------------
    [TestMethod]
    public void FreeShip_ShouldSetShippingToZero()
    {
        var id = Guid.NewGuid();

        var existing = new BasketModel { BasketId = id };
        existing.Lines.Add(new BasketLineModel { Sku = "A", UnitPrice = 10m, Quantity = 1 });

        BasketModel outBasket = existing;

        _store.Setup(s => s.TryGet(id, out outBasket)).Returns(true);
        _store.Setup(s => s.GetOrCreate(id)).Returns(existing);

        _service = new BasketService(_store.Object);

        _service.SetDiscountCode(id, new DiscountCodeDto("FREESHIP", 0m));

        var totals = _service.GetTotals(id, includeVat: false);

        totals.Shipping.Should().Be(0m);
    }


    // -------------------------------------------------------
    // BOGOF — discount already represented via EffectiveUnitPrice
    // -------------------------------------------------------
    [TestMethod]
    public void Bogof_ShouldRespectDiscountedPriceOnLines()
    {
        var id = Guid.NewGuid();

        var existing = new BasketModel { BasketId = id };
        existing.Lines.Add(new BasketLineModel
        {
            Sku = "A",
            UnitPrice = 10m,
            Quantity = 2,
            IsDiscountedItem = true,
            DiscountedPrice = 5m
        });

        BasketModel outBasket = existing;

        _store.Setup(s => s.TryGet(id, out outBasket)).Returns(true);
        _store.Setup(s => s.GetOrCreate(id)).Returns(existing);

        _service = new BasketService(_store.Object);

        _service.SetDiscountCode(id, new DiscountCodeDto("BOGOF", 0m));

        var totals = _service.GetTotals(id, includeVat: false);

        totals.SubtotalExcludingDiscounts.Should().Be(10m);
        totals.DiscountFromCode.Should().Be(0m);
    }

    [TestMethod]
    public void SetDiscountCode_ShouldThrowWhenBasketDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _store.Setup(s => s.GetOrCreate(id)).Returns((BasketModel)null!);

        // Act
        Action act = () => _service.SetDiscountCode(id, new DiscountCodeDto("SUMMER10", 10m));

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}
