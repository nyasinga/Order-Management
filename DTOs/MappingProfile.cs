using AutoMapper;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Models;

namespace OrderManagementSystem.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name));
            
        CreateMap<CreateOrderDto, Order>();
        
        // Order item mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
            
        CreateMap<CreateOrderItemDto, OrderItem>();
        
        // Status history mappings
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();
        
        // Analytics mappings
        CreateMap<Interfaces.OrderAnalytics, OrderAnalyticsDto>()
            .ForMember(dest => dest.AverageFulfillmentTime, 
                opt => opt.MapFrom(src => FormatTimeSpan(src.AverageFulfillmentTime)))
            .ForMember(dest => dest.OrdersByStatus, 
                opt => opt.MapFrom(src => src.OrdersByStatus.ToDictionary(
                    k => k.Key.ToString(), 
                    v => v.Value)))
            .ForMember(dest => dest.OrdersByCustomerSegment, 
                opt => opt.MapFrom(src => src.OrdersByCustomerSegment.ToDictionary(
                    k => k.Key.ToString(), 
                    v => v.Value)));
    }
    
    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
    }
}
