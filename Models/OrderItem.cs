namespace OrderManagementSystem.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice => (UnitPrice * Quantity) - Discount;
    
    // Foreign keys
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
