namespace CashewNuts.Domain.Entities;

public class Purchase
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string? SupplierName { get; set; }   // Optional
    public string? SupplierPhone { get; set; }  // Optional
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}

public class PurchaseItem
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public int CashewTypeId { get; set; }
    public CashewType? CashewType { get; set; }
    public decimal QtyKg { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal Total { get; set; }
}
