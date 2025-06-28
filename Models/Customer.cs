using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.Models;

public class Customer
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum CustomerSegment
{
    Standard,
    Premium,
    Gold,
    Platinum
}
