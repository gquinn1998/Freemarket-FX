namespace Basket.Api.Models;

public class BasketTotalsModel
{
    public decimal SubtotalExcludingDiscounts { get; set; }
    public decimal DiscountFromCode { get; set; }
    public decimal Shipping { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
}
