using Basket.Api.Models;

namespace Basket.Api.IntegrationTests;

public class BasketBuilder
{
    private readonly BasketModel _basket = new();

    public BasketBuilder WithItem(string name, decimal price, int qty)
    {
        _basket.Lines.Add(new BasketLineModel
        {
            Name = name,
            UnitPrice = price,
            Quantity = qty
        });
        return this;
    }

    public BasketModel Build() => _basket;
}
