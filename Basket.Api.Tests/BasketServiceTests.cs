using Basket.Api.Dtos;
using Basket.Api.MemoryStore;
using Basket.Api.Models;
using Basket.Api.Services;
using FluentAssertions;
using Moq;

namespace Basket.Api.Tests;

[TestClass]
public class BasketServiceTests
{
    private Mock<IBasketStore> _store;
    private BasketService _service;

    [TestInitialize]
    public void Setup()
    {
        _store = new Mock<IBasketStore>();
        _service = new BasketService(_store.Object);
    }

    [TestMethod]
    public void GetOrCreate_ShouldReturnExistingBasket()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new BasketModel { BasketId = id };

        _store.Setup(s => s.GetOrCreate(id)).Returns(existing);

        // Act
        var result = _service.GetOrCreate(id);

        // Assert
        result.Should().Be(existing);
        _store.Verify(s => s.GetOrCreate(id), Times.Once);
    }

    [TestMethod]
    public void GetOrCreate_ShouldNotThrowWhenStoreReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        BasketModel? basket = null;

        _store.Setup(s => s.GetOrCreate(id)).Returns(basket);

        // Act
        Action act = () => _service.GetOrCreate(id);

        // Assert
        act.Should().NotThrow(); // BasketService does not guard nulls here
    }

    [TestMethod]
    public void AddItem_ShouldAddNewLineWhenSkuNotExisting()
    {
        // Arrange
        var id = Guid.NewGuid();
        var basket = new BasketModel { BasketId = id };

        _store.Setup(s => s.GetOrCreate(id)).Returns(basket);

        var dto = new AddItemDto("ABC", "Item", 10m, 2);

        // Act
        _service.AddItem(id, dto);

        // Assert
        basket.Lines.Should().HaveCount(1);
        basket.Lines[0].Sku.Should().Be("ABC");
        basket.Lines[0].Quantity.Should().Be(2);

        _store.Verify(s => s.Save(basket), Times.Once);
    }

    [TestMethod]
    public void AddItem_ShouldIncreaseQuantityWhenSameSkuAndPrice()
    {
        // Arrange
        var id = Guid.NewGuid();
        var basket = new BasketModel { BasketId = id };

        var existingLine = new BasketLineModel
        {
            Sku = "ABC",
            Name = "Item",
            UnitPrice = 10m,
            Quantity = 1
        };

        basket.Lines.Add(existingLine);

        _store.Setup(s => s.GetOrCreate(id)).Returns(basket);

        var dto = new AddItemDto("ABC", "Item", 10m, 3);

        // Act
        _service.AddItem(id, dto);

        // Assert
        existingLine.Quantity.Should().Be(4);
        basket.Lines.Should().HaveCount(1);

        _store.Verify(s => s.Save(basket), Times.Once);
    }

    [TestMethod]
    public void AddItem_ShouldThrowWhenDtoIsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () => _service.AddItem(id, null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void AddItem_ShouldNotAddWhenQuantityIsZero()
    {
        // Arrange
        var id = Guid.NewGuid();
        var basket = new BasketModel { BasketId = id };

        _store.Setup(s => s.GetOrCreate(id)).Returns(basket);

        var dto = new AddItemDto("ABC", "Item", 10m, 0);

        // Act
        _service.AddItem(id, dto);

        // Assert – zero quantity results in no new item
        basket.Lines.Should().BeEmpty();
        _store.Verify(s => s.Save(It.IsAny<BasketModel>()), Times.Never);
    }

    [TestMethod]
    public void AddItem_ShouldThrowWhenUnitPriceIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var basket = new BasketModel { BasketId = id };

        _store.Setup(s => s.GetOrCreate(id)).Returns(basket);

        var dto = new AddItemDto("ABC", "Item", -5m, 1);

        // Act
        Action act = () => _service.AddItem(id, dto);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void RemoveLine_ShouldRemoveLineIfExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lineId = Guid.NewGuid();

        var basket = new BasketModel { BasketId = id };
        basket.Lines.Add(new BasketLineModel
        {
            LineId = lineId,
            Sku = "ABC",
            UnitPrice = 10,
            Quantity = 1
        });

        _store.Setup(s => s.TryGet(id, out basket)).Returns(true);

        // Act
        var result = _service.RemoveLine(id, lineId);

        // Assert
        result.Should().BeTrue();
        basket.Lines.Should().BeEmpty();

        _store.Verify(s => s.Save(basket), Times.Once);
    }

    [TestMethod]
    public void RemoveLine_ShouldReturnFalseWhenBasketDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        BasketModel? basket = null;

        _store.Setup(s => s.TryGet(id, out basket)).Returns(false);

        // Act
        var result = _service.RemoveLine(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _store.Verify(s => s.Save(It.IsAny<BasketModel>()), Times.Never);
    }

    [TestMethod]
    public void RemoveLine_ShouldReturnFalseWhenLineNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        BasketModel basket = new() { BasketId = id };

        _store.Setup(s => s.TryGet(id, out basket)).Returns(true);

        // Act
        var result = _service.RemoveLine(id, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
        _store.Verify(s => s.Save(It.IsAny<BasketModel>()), Times.Never);
    }

    [TestMethod]
    public void GetTotals_ShouldComputeTotalsCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();

        var basket = new BasketModel { BasketId = id };
        basket.Lines.Add(new BasketLineModel { Sku = "A", UnitPrice = 10, Quantity = 2 });
        basket.Lines.Add(new BasketLineModel { Sku = "B", UnitPrice = 5, Quantity = 1 });

        _store.Setup(s => s.TryGet(id, out basket)).Returns(true);

        // Act
        var totals = _service.GetTotals(id, includeVat: true);

        // Assert
        totals.SubtotalExcludingDiscounts.Should().Be(25m);
        totals.Total.Should().BeGreaterThan(25m); // shipping + vat applied
    }

    [TestMethod]
    public void GetTotals_ShouldThrow_WhenBasketDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        BasketModel basket = null;

        _store.Setup(s => s.TryGet(id, out basket)).Returns(false);

        // Act
        Action act = () => _service.GetTotals(id, includeVat: true);

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [TestMethod]
    public void GetTotals_ShouldHandleNoLinesGracefully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var basket = new BasketModel { BasketId = id };

        _store.Setup(s => s.TryGet(id, out basket)).Returns(true);

        // Act
        var totals = _service.GetTotals(id, includeVat: false);

        // Assert
        totals.SubtotalExcludingDiscounts.Should().Be(0m);
        totals.Total.Should().BeGreaterThanOrEqualTo(0m);
    }

    [TestMethod]
    public void GetTotals_ShouldNotFailWhenDiscountCodeIsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        var basket = new BasketModel { BasketId = id };
        basket.Lines.Add(new BasketLineModel { Sku = "A", UnitPrice = 10, Quantity = 1 });

        _store.Setup(s => s.TryGet(id, out basket)).Returns(true);

        // Act
        Action act = () => _service.GetTotals(id, includeVat: false);

        // Assert
        act.Should().NotThrow();
    }

    [TestMethod]
    public void GetTotals_ShouldNotApplyDiscountWhenPercentIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();

        var basket = new BasketModel { BasketId = id };
        basket.Lines.Add(new BasketLineModel { Sku = "A", UnitPrice = 10, Quantity = 2 });

        basket.DiscountCode = new DiscountCodeDto("BADCODE", -10m); // invalid negative %

        _store.Setup(s => s.TryGet(id, out basket)).Returns(true);

        // Act
        var totals = _service.GetTotals(id, includeVat: false);

        // Assert — subtotal should remain unchanged
        totals.SubtotalExcludingDiscounts.Should().Be(20m);
        totals.DiscountFromCode.Should().Be(0m);
    }
}
