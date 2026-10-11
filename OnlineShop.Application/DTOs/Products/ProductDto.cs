namespace OnlineShop.Application.DTOs.Products;

public class ProductDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string EditionType { get; set; } = string.Empty;
    public int CategoryId { get; set; }

}
