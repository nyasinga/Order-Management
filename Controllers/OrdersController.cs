using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Interfaces;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get an order by ID
    /// </summary>
    /// <param name="id">The ID of the order to retrieve</param>
    /// <returns>The requested order</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", id);
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    /// <param name="customerId">The ID of the customer</param>
    /// <returns>List of customer's orders</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(int customerId)
    {
        var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="createOrderDto">Order details</param>
    /// <returns>The created order</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(createOrderDto);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "An error occurred while creating the order.");
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="statusDto">Status update details</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        int id, 
        [FromBody] UpdateOrderStatusDto statusDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, statusDto);
            if (!success)
                return NotFound($"Order with ID {id} not found.");
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
            return StatusCode(500, "An error occurred while updating the order status.");
        }
    }

    /// <summary>
    /// Get order status history
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>List of status changes</returns>
    [HttpGet("{id}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderStatusHistoryDto>>> GetOrderHistory(int id)
    {
        try
        {
            var history = await _orderService.GetOrderHistoryAsync(id);
            return Ok(history);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", id);
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get order analytics
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Order analytics data</returns>
    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrderAnalyticsDto>> GetAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        if (endDate.HasValue && startDate.HasValue && endDate < startDate)
        {
            return BadRequest("End date must be greater than or equal to start date");
        }

        var analytics = await _orderService.GetOrderAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }
}
