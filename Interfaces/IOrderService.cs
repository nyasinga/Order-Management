using OrderManagementSystem.DTOs;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto statusDto);
    Task<OrderAnalyticsDto> GetOrderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<OrderStatusHistoryDto>> GetOrderHistoryAsync(int orderId);
}
