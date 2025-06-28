using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data.Repositories;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Interfaces;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDiscountService _discountService;
    private readonly ILogger<OrderService> _logger;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        IDiscountService discountService,
        ILogger<OrderService> logger,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _discountService = discountService;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<OrderDto> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {id} not found.");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        // In a real application, we would validate the DTO and check product availability
        var order = _mapper.Map<Order>(createOrderDto);
        
        // Calculate total amount
        order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);
        
        // Apply discounts
        order.DiscountAmount = await _discountService.CalculateDiscountAsync(order);
        order.FinalAmount = order.TotalAmount - order.DiscountAmount;

        // Set initial status
        order.Status = OrderStatus.Pending;
        order.OrderDate = DateTime.UtcNow;

        // Save the order
        var createdOrder = await _orderRepository.CreateAsync(order);
        
        // Record the initial status
        await _orderRepository.UpdateOrderStatusAsync(
            createdOrder.Id, 
            OrderStatus.Pending, 
            "System", 
            "Order created");

        return _mapper.Map<OrderDto>(createdOrder);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto statusDto)
    {
        return await _orderRepository.UpdateOrderStatusAsync(
            orderId, 
            statusDto.NewStatus, 
            statusDto.ChangedBy, 
            statusDto.Notes);
    }

    public async Task<OrderAnalyticsDto> GetOrderAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var analytics = await _orderRepository.GetOrderAnalyticsAsync(startDate, endDate);
        return _mapper.Map<OrderAnalyticsDto>(analytics);
    }

    public async Task<IEnumerable<OrderStatusHistoryDto>> GetOrderHistoryAsync(int orderId)
    {
        var history = await _orderRepository.GetOrderHistoryAsync(orderId);
        return _mapper.Map<IEnumerable<OrderStatusHistoryDto>>(history);
    }
}
