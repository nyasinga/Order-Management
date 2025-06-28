using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Data;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Models;
using Xunit;

namespace OrderManagementSystem.Tests;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly OrderManagementContext _context;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing context configuration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<OrderManagementContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<OrderManagementContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<OrderManagementContext>();
                    
                    // Ensure the database is created
                    db.Database.EnsureCreated();
                    
                    // Seed the database with test data
                    DbInitializer.Initialize(db);
                }
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<OrderManagementContext>();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOrder()
    {
        // Arrange
        var testOrder = await _context.Orders.FirstAsync();

        // Act
        var response = await _client.GetAsync($"api/orders/{testOrder.Id}");
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(order);
        Assert.Equal(testOrder.Id, order.Id);
        Assert.Equal(testOrder.OrderNumber, order.OrderNumber);
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"api/orders/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedOrder()
    {
        // Arrange
        var customer = await _context.Customers.FirstAsync();
        var product = await _context.Products.FirstAsync();
        
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customer.Id,
            OrderItems = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    UnitPrice = product.Price
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/orders", createOrderDto);
        var createdOrder = await response.Content.ReadFromJsonAsync<OrderDto>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdOrder);
        Assert.True(createdOrder.Id > 0);
        Assert.Equal(2 * product.Price, createdOrder.TotalAmount);
        Assert.Single(createdOrder.OrderItems);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_UpdatesStatus()
    {
        // Arrange
        var order = await _context.Orders
            .FirstAsync(o => o.Status != OrderStatus.Delivered);
        
        var updateStatusDto = new UpdateOrderStatusDto
        {
            NewStatus = OrderStatus.Delivered,
            ChangedBy = "testuser",
            Notes = "Delivered by test"
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"api/orders/{order.Id}/status", 
            updateStatusDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify the status was updated
        var updatedOrder = await _context.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Delivered, updatedOrder?.Status);
        
        // Verify the status history was recorded
        var history = _context.OrderStatusHistories
            .Where(h => h.OrderId == order.Id)
            .OrderByDescending(h => h.ChangedAt)
            .First();
            
        Assert.Equal(OrderStatus.Delivered, history.NewStatus);
        Assert.Equal("testuser", history.ChangedBy);
        Assert.Equal("Delivered by test", history.Notes);
    }

    [Fact]
    public async Task GetOrderAnalytics_ReturnsAnalytics()
    {
        // Act
        var response = await _client.GetAsync("api/orders/analytics");
        var analytics = await response.Content.ReadFromJsonAsync<OrderAnalyticsDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(analytics);
        Assert.True(analytics.TotalOrders > 0);
        Assert.True(analytics.TotalRevenue > 0);
        Assert.True(analytics.AverageOrderValue > 0);
        Assert.NotEmpty(analytics.OrdersByStatus);
        Assert.NotEmpty(analytics.OrdersByCustomerSegment);
    }

    [Fact]
    public async Task GetOrderHistory_ReturnsHistory()
    {
        // Arrange
        var order = await _context.Orders
            .Include(o => o.StatusHistory)
            .FirstAsync(o => o.StatusHistory.Any());

        // Act
        var response = await _client.GetAsync($"api/orders/{order.Id}/history");
        var history = await response.Content.ReadFromJsonAsync<List<OrderStatusHistoryDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(history);
        Assert.NotEmpty(history);
        Assert.Equal(order.StatusHistory.Count, history.Count);
    }
}
