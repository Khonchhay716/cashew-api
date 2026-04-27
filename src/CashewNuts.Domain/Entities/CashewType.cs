using CashewNuts.Domain.Entities;

public class CashewType
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal DefaultPrice { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UserId { get; set; }   // ← nullable
    public User? User { get; set; }
}