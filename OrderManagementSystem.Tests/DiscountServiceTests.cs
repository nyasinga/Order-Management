using Microsoft.Extensions.Logging;
using Moq;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Interfaces;
using OrderManagementSystem.Models;
using OrderManagementSystem.Services;
using Xunit;

namespace OrderManagementSystem.Tests;

public class DiscountServiceTests
{
    private readonly IDiscountService _discountService;
    private readonly List<ICustomerDiscountRule> _discountRules;
    private readonly Mock<ILogger<DiscountService>> _loggerMock;

    public DiscountServiceTests()
    {
        _loggerMock = new Mock<ILogger<DiscountService>>();
        _discountRules = new List<ICustomerDiscountRule>
        {
            new PremiumCustomerDiscountRule(),
            new BulkOrderDiscountRule(),
            new HighValueOrderDiscountRule()
        };
        _discountService = new DiscountService(_discountRules);
    }

    [Fact]
    public async Task CalculateDiscountAsync_StandardCustomer_NoDiscount()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Standard };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 100,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 1, UnitPrice = 100 }
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        Assert.Equal(0, discount);
    }


    [Fact]
    public async Task CalculateDiscountAsync_PremiumCustomer_5PercentDiscount()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Premium };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 200,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 1, UnitPrice = 200 }
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        Assert.Equal(10, discount); // 5% of 200
    }

    [Fact]
    public async Task CalculateDiscountAsync_GoldCustomer_10PercentDiscount()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Gold };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 200,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 1, UnitPrice = 200 }
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        Assert.Equal(20, discount); // 10% of 200
    }


    [Fact]
    public async Task CalculateDiscountAsync_PlatinumCustomer_15PercentDiscount()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Platinum };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 200,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 1, UnitPrice = 200 }
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        Assert.Equal(30, discount); // 15% of 200
    }


    [Fact]
    public async Task CalculateDiscountAsync_BulkOrder_AdditionalDiscount()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Standard };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 1000,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 11, UnitPrice = 90.91m } // 11 items > 10
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        Assert.True(discount > 0);
        // 11% discount (1% per item, max 10%) of 1000 = 100
        Assert.Equal(100, discount);
    }

    [Fact]
    public async Task CalculateDiscountAsync_HighValueOrder_FlatDiscount()
    {
        // Arrange
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Standard };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 600,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 1, UnitPrice = 600 }
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        Assert.Equal(50, discount); // $50 flat discount for orders over $500
    }

    [Fact]
    public async Task CalculateDiscountAsync_MultipleDiscounts_CombinedCorrectly()
    {
        // Arrange - Platinum customer (15%) + High value order ($50) + Bulk order (10%)
        var customer = new Customer { Id = 1, Name = "Test", Segment = CustomerSegment.Platinum };
        var order = new Order
        {
            Customer = customer,
            TotalAmount = 1000,
            OrderItems = new List<OrderItem>
            {
                new() { Quantity = 11, UnitPrice = 90.91m } // 11 items > 10
            }
        };

        // Act
        var discount = await _discountService.CalculateDiscountAsync(order);

        // Assert
        // 15% (platinum) + 10% (bulk) = 25% of 1000 = 250 + $50 (high value) = 300
        // But since we're applying discounts sequentially, the actual calculation is different
        Assert.True(discount > 0);
        // First Platinum discount: 15% of 1000 = 150 (new total: 850)
        // Then Bulk discount: 10% of 850 = 85
        // Then High value: $50
        // Total: 150 + 85 + 50 = 285
        Assert.Equal(285, discount);
    }
}
