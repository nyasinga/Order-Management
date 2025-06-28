using OrderManagementSystem.Models;

namespace OrderManagementSystem.Interfaces;

public interface IDiscountService
{
    Task<decimal> CalculateDiscountAsync(Order order);
}

public interface ICustomerDiscountRule
{
    bool IsApplicable(Customer customer, Order order);
    decimal CalculateDiscount(Customer customer, Order order);
    int Priority { get; }
}
