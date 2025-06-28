using System.Collections.Immutable;
using OrderManagementSystem.Interfaces;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.Services;

public class DiscountService : IDiscountService
{
    private readonly IEnumerable<ICustomerDiscountRule> _discountRules;

    public DiscountService(IEnumerable<ICustomerDiscountRule> discountRules)
    {
        _discountRules = discountRules;
    }

    public async Task<decimal> CalculateDiscountAsync(Order order)
    {
        if (order.Customer == null)
            return 0;

        var applicableRules = _discountRules
            .Where(r => r.IsApplicable(order.Customer, order))
            .OrderBy(r => r.Priority)
            .ToList();

        decimal totalDiscount = 0;
        var remainingAmount = order.TotalAmount;

        foreach (var rule in applicableRules)
        {
            var discount = rule.CalculateDiscount(order.Customer, order);
            totalDiscount += discount;
            remainingAmount = order.TotalAmount - totalDiscount;
            
            // Prevent negative remaining amount
            if (remainingAmount <= 0)
                return order.TotalAmount;
        }

        return totalDiscount > order.TotalAmount ? order.TotalAmount : totalDiscount;
    }
}

// Example discount rules
public class PremiumCustomerDiscountRule : ICustomerDiscountRule
{
    public int Priority => 1;

    public bool IsApplicable(Customer customer, Order order)
    {
        return customer.Segment == CustomerSegment.Premium || 
               customer.Segment == CustomerSegment.Gold ||
               customer.Segment == CustomerSegment.Platinum;
    }

    public decimal CalculateDiscount(Customer customer, Order order)
    {
        if (!IsApplicable(customer, order)) return 0;
        
        return customer.Segment switch
        {
            CustomerSegment.Premium => order.TotalAmount * 0.05m, // 5% discount
            CustomerSegment.Gold => order.TotalAmount * 0.10m,    // 10% discount
            CustomerSegment.Platinum => order.TotalAmount * 0.15m, // 15% discount
            _ => 0
        };
    }
}

public class BulkOrderDiscountRule : ICustomerDiscountRule
{
    public int Priority => 2;

    public bool IsApplicable(Customer customer, Order order)
    {
        return order.OrderItems.Sum(oi => oi.Quantity) > 10;
    }

    public decimal CalculateDiscount(Customer customer, Order order)
    {
        if (!IsApplicable(customer, order)) return 0;
        
        var totalItems = order.OrderItems.Sum(oi => oi.Quantity);
        var discountPercentage = Math.Min(0.1m, totalItems * 0.01m); // 1% per item, max 10%
        return order.TotalAmount * discountPercentage;
    }
}

public class HighValueOrderDiscountRule : ICustomerDiscountRule
{
    public int Priority => 3;

    public bool IsApplicable(Customer customer, Order order)
    {
        return order.TotalAmount > 500;
    }

    public decimal CalculateDiscount(Customer customer, Order order)
    {
        if (!IsApplicable(customer, order)) return 0;
        
        // $50 flat discount for orders over $500
        return 50;
    }
}
