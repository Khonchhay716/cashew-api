namespace CashewNuts.Domain.Entities;

public class Sale
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string? CustomerName { get; set; }  // Optional
    public string? CustomerPhone { get; set; } // Optional
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int CashewTypeId { get; set; }
    public CashewType? CashewType { get; set; }
    public decimal QtyKg { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal Total { get; set; }
}
