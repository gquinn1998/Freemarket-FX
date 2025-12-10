using Basket.Api.MemoryStore;
using Basket.Api.Models;
using Basket.Api.Services;
using FluentAssertions;
using Moq;

namespace Basket.Api.Tests;

[TestClass]
public class TotalsTests
{
    [TestMethod]
    public void GetTotals_ShouldCalculateSubtotalShippingVatAndTotalCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();

        var basket = new BasketModel { BasketId = id };
        basket.Lines.Add(new BasketLineModel { Sku = "A", UnitPrice = 10m, Quantity = 2 });
        basket.Lines.Add(new BasketLineModel { Sku = "B", UnitPrice = 5m, Quantity = 1 });

        var storeMock = new Mock<IBasketStore>();
        // setup TryGet to return true and output the basket instance
        storeMock.Setup(s => s.TryGet(id, out basket)).Returns(true);

        var service = new BasketService(storeMock.Object);

        // Act
        var totals = service.GetTotals(id, includeVat: true);

        // Assert
        // Subtotal: (10*2) + (5*1) = 25
        totals.SubtotalExcludingDiscounts.Should().Be(25m);

        // Shipping: default country is "UK" and subtotal < 50 => £5
        totals.Shipping.Should().Be(5m);

        // VAT calculation:
        // - VAT on items: 20% of 25 = 5.00
        // - VAT on shipping: 20% of 5 = 1.00
        // total VAT = 6.00
        totals.Vat.Should().Be(6.00m);

        // Total (subtotalAfterDiscount + shipping + vat) = 25 + 5 + 6 = 36
        totals.Total.Should().Be(36.00m);
    }
}
