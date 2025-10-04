using AuthSegura.Models;

public class Product{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime? DiscountStartDate { get; set; }
    public DateTime? DiscountEndDate { get; set; }

    public decimal GetFinalPrice()
    {
        if (DiscountPercentage.HasValue &&
            DiscountPercentage.Value > 0 && // Añadir esta condición para verificar que sea mayor que cero
            DiscountStartDate.HasValue && DiscountEndDate.HasValue &&
            DateTime.UtcNow >= DiscountStartDate.Value &&
            DateTime.UtcNow <= DiscountEndDate.Value)
        {
            return Price * (1 - (DiscountPercentage.Value / 100));
        }
        return Price;
    }
    
    public bool IsDiscountActive()
    {
        return DiscountPercentage.HasValue &&
               DiscountPercentage.Value > 0 && // Añadir esta condición para verificar que sea mayor que cero
               DiscountStartDate.HasValue && DiscountEndDate.HasValue &&
               DateTime.UtcNow >= DiscountStartDate.Value &&
               DateTime.UtcNow <= DiscountEndDate.Value;
    }
}