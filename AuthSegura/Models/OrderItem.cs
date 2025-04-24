public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Precio original del producto
    public decimal DiscountedPrice { get; set; } // Precio aplicado con descuento
    public decimal? DiscountPercentage { get; set; } // Porcentaje de descuento aplicado
}