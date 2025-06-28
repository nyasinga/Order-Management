using OrderManagementSystem.Models;

namespace OrderManagementSystem.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<Order> CreateAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<OrderStatusHistory>> GetOrderHistoryAsync(int orderId);
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? changedBy = null, string? notes = null);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<OrderAnalytics> GetOrderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

public record OrderAnalytics
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public TimeSpan AverageFulfillmentTime { get; set; }
    public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new();
    public Dictionary<CustomerSegment, int> OrdersByCustomerSegment { get; set; } = new();
}
