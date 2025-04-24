public class OrderItemResponse
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; } // Precio original
    public decimal DiscountedPrice { get; set; } // Precio con descuento
    public decimal? DiscountPercentage { get; set; } // Porcentaje de descuento aplicado
}