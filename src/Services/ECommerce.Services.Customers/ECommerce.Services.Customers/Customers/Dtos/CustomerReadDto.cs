namespace ECommerce.Services.Customers.Customers.Dtos;

public record CustomerReadDto
{
    public Guid Id { get; set; }
    public long CustomerId { get; set; }
    public Guid IdentityId { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DetailAddress { get; set; }
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PhoneNumber { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
