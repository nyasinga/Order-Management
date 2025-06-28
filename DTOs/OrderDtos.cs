using System.ComponentModel.DataAnnotations;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    public ICollection<OrderStatusHistoryDto> StatusHistory { get; set; } = new List<OrderStatusHistoryDto>();
}

public class CreateOrderDto
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required, MinLength(1, ErrorMessage = "At least one order item is required")]
    public ICollection<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
}

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus NewStatus { get; set; }
    
    [StringLength(100)]
    public string ChangedBy { get; set; } = "System";
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class OrderAnalyticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public string AverageFulfillmentTime { get; set; } = string.Empty;
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public Dictionary<string, int> OrdersByCustomerSegment { get; set; } = new();
}

public class OrderStatusHistoryDto
{
    public int Id { get; set; }
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
    public decimal UnitPrice { get; set; }
}
