public class OrderItemResponse
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public required string ProductName { get; set; }
    public string? ProductImage { get; set; } // URL de la imagen del producto
    public string? ProductDescription { get; set; } // Descripción del producto
    public required string Category { get; set; } // Categoría del producto
    public required int CategoryId { get; set; } // Marca del producto
    public decimal? Price { get; set; } // Precio original
    public decimal? DiscountedPrice { get; set; } // Precio con descuento
    public decimal? DiscountPercentage { get; set; } // Porcentaje de descuento aplicado
}