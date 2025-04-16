using AuthSegura.Models;

public class Order{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
    public DateTime OrderDate { get; set; }
    public required decimal TotalAmount { get; set; }
    public required ICollection<OrderItem> OrderItems { get; set; }
}