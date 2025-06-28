using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Interfaces;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderManagementContext _context;

    public OrderRepository(OrderManagementContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetOrderHistoryAsync(int orderId)
    {
        return await _context.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? changedBy = null, string? notes = null)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return false;

        var oldStatus = order.Status;
        order.Status = newStatus;

        // Record the status change
        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = changedBy ?? "System",
            Notes = notes,
            ChangedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistories.Add(history);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Where(o => o.Status == status)
            .Include(o => o.Customer)
            .ToListAsync();
    }

    public async Task<OrderAnalytics> GetOrderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.StatusHistory)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .ToListAsync();

        var completedOrders = orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .ToList();

        var fulfillmentTimes = completedOrders
            .Select(o => 
            {
                var created = o.StatusHistory
                    .OrderBy(h => h.ChangedAt)
                    .FirstOrDefault(h => h.NewStatus == OrderStatus.Processing);
                var completed = o.StatusHistory
                    .OrderBy(h => h.ChangedAt)
                    .FirstOrDefault(h => h.NewStatus == OrderStatus.Delivered);

                if (created == null || completed == null) 
                    return TimeSpan.Zero;
                    
                return completed.ChangedAt - created.ChangedAt;
            })
            .Where(t => t > TimeSpan.Zero)
            .ToList();

        var averageFulfillmentTime = fulfillmentTimes.Any() 
            ? TimeSpan.FromTicks((long)fulfillmentTimes.Average(t => t.Ticks))
            : TimeSpan.Zero;

        return new OrderAnalytics
        {
            TotalOrders = orders.Count,
            TotalRevenue = completedOrders.Sum(o => o.FinalAmount),
            AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.FinalAmount) : 0,
            AverageFulfillmentTime = averageFulfillmentTime,
            OrdersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            OrdersByCustomerSegment = orders
                .GroupBy(o => o.Customer.Segment)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
