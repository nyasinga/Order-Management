using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Data;

public static class DbInitializer
{
    public static void Initialize(OrderManagementContext context)
    {
        // Make sure the database is created
        context.Database.EnsureCreated();

        // Check if we already have data
        if (context.Customers.Any())
        {
            return; // DB has been seeded
        }

        // Seed Customers
        var customers = new[]
        {
            new Customer { Name = "John Doe", Email = "john.doe@example.com", Segment = CustomerSegment.Standard },
            new Customer { Name = "Jane Smith", Email = "jane.smith@example.com", Segment = CustomerSegment.Premium },
            new Customer { Name = "Bob Johnson", Email = "bob.johnson@example.com", Segment = CustomerSegment.Gold },
            new Customer { Name = "Alice Brown", Email = "alice.brown@example.com", Segment = CustomerSegment.Platinum },
        };
        context.Customers.AddRange(customers);
        context.SaveChanges();

        // Seed Products
        var products = new[]
        {
            new Product { Name = "Laptop", Description = "High performance laptop", Price = 999.99m, StockQuantity = 50 },
            new Product { Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, StockQuantity = 100 },
            new Product { Name = "Headphones", Description = "Noise cancelling headphones", Price = 199.99m, StockQuantity = 75 },
            new Product { Name = "Smartwatch", Description = "Fitness and health tracker", Price = 249.99m, StockQuantity = 60 },
            new Product { Name = "Tablet", Description = "10-inch tablet with stylus", Price = 349.99m, StockQuantity = 40 },
        };
        context.Products.AddRange(products);
        context.SaveChanges();

        // Seed Orders
        var orders = new List<Order>
        {
            new()
            {
                CustomerId = customers[0].Id,
                OrderDate = DateTime.UtcNow.AddDays(-10),
                Status = OrderStatus.Delivered,
                TotalAmount = 1299.98m,
                DiscountAmount = 0,
                FinalAmount = 1299.98m,
                OrderNumber = "ORD" + DateTime.UtcNow.Ticks
            },
            new()
            {
                CustomerId = customers[1].Id,
                OrderDate = DateTime.UtcNow.AddDays(-5),
                Status = OrderStatus.Processing,
                TotalAmount = 899.98m,
                DiscountAmount = 45.00m, // 5% discount for premium
                FinalAmount = 854.98m,
                OrderNumber = "ORD" + (DateTime.UtcNow.Ticks + 1)
            },
            new()
            {
                CustomerId = customers[2].Id,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Status = OrderStatus.Shipped,
                TotalAmount = 549.98m,
                DiscountAmount = 55.00m, // 10% discount for gold
                FinalAmount = 494.98m,
                OrderNumber = "ORD" + (DateTime.UtcNow.Ticks + 2)
            },
            new()
            {
                CustomerId = customers[3].Id,
                OrderDate = DateTime.UtcNow.AddDays(-1),
                Status = OrderStatus.Pending,
                TotalAmount = 1749.97m,
                DiscountAmount = 262.50m, // 15% discount for platinum + bulk discount
                FinalAmount = 1487.47m,
                OrderNumber = "ORD" + (DateTime.UtcNow.Ticks + 3)
            }
        };
        context.Orders.AddRange(orders);
        context.SaveChanges();

        // Seed OrderItems
        var orderItems = new[]
        {
            // Order 1
            new OrderItem { OrderId = orders[0].Id, ProductId = products[0].Id, Quantity = 1, UnitPrice = 999.99m, Discount = 0 },
            new OrderItem { OrderId = orders[0].Id, ProductId = products[2].Id, Quantity = 1, UnitPrice = 199.99m, Discount = 0 },
            
            // Order 2
            new OrderItem { OrderId = orders[1].Id, ProductId = products[1].Id, Quantity = 1, UnitPrice = 699.99m, Discount = 35.00m },
            new OrderItem { OrderId = orders[1].Id, ProductId = products[2].Id, Quantity = 1, UnitPrice = 199.99m, Discount = 10.00m },
            
            // Order 3
            new OrderItem { OrderId = orders[2].Id, ProductId = products[3].Id, Quantity = 1, UnitPrice = 249.99m, Discount = 25.00m },
            new OrderItem { OrderId = orders[2].Id, ProductId = products[2].Id, Quantity = 1, UnitPrice = 199.99m, Discount = 20.00m },
            new OrderItem { OrderId = orders[2].Id, ProductId = products[4].Id, Quantity = 1, UnitPrice = 99.99m, Discount = 10.00m },
            
            // Order 4 (bulk order)
            new OrderItem { OrderId = orders[3].Id, ProductId = products[0].Id, Quantity = 1, UnitPrice = 999.99m, Discount = 150.00m },
            new OrderItem { OrderId = orders[3].Id, ProductId = products[1].Id, Quantity = 1, UnitPrice = 699.99m, Discount = 105.00m },
            new OrderItem { OrderId = orders[3].Id, ProductId = products[3].Id, Quantity = 2, UnitPrice = 249.99m, Discount = 75.00m }
        };
        context.OrderItems.AddRange(orderItems);
        context.SaveChanges();

        // Seed OrderStatusHistory
        var now = DateTime.UtcNow;
        var statusHistories = new List<OrderStatusHistory>
        {
            // Order 1 (Delivered)
            new() { OrderId = orders[0].Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Processing, ChangedAt = now.AddDays(-9), ChangedBy = "System" },
            new() { OrderId = orders[0].Id, OldStatus = OrderStatus.Processing, NewStatus = OrderStatus.Shipped, ChangedAt = now.AddDays(-8), ChangedBy = "System" },
            new() { OrderId = orders[0].Id, OldStatus = OrderStatus.Shipped, NewStatus = OrderStatus.Delivered, ChangedAt = now.AddDays(-6), ChangedBy = "System" },
            
            // Order 2 (Processing)
            new() { OrderId = orders[1].Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Processing, ChangedAt = now.AddDays(-4), ChangedBy = "System" },
            
            // Order 3 (Shipped)
            new() { OrderId = orders[2].Id, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Processing, ChangedAt = now.AddDays(-1), ChangedBy = "System" },
            new() { OrderId = orders[2].Id, OldStatus = OrderStatus.Processing, NewStatus = OrderStatus.Shipped, ChangedAt = now, ChangedBy = "System" },
            
            // Order 4 (Pending) - No status changes yet
        };
        context.OrderStatusHistories.AddRange(statusHistories);
        context.SaveChanges();
    }
}
