namespace CashewNuts.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Name, string Role);
public record RegisterRequest(string Name, string Email, string Password, string Role);

public record CashewTypeDto(int Id, string Name, decimal DefaultPrice, string? Description, bool IsActive);
public record CreateCashewTypeRequest(string Name, decimal DefaultPrice, string? Description);
public record UpdateCashewTypeRequest(string Name, decimal DefaultPrice, string? Description);

// Purchase — multiple items, no paid amount, optional name/phone
public record CreatePurchaseRequest(
    string? SupplierName,
    string? SupplierPhone,
    string? Note,
    DateTime PurchaseDate,
    List<CreateItemRequest> Items
);

public record CreateItemRequest(int CashewTypeId, decimal QtyKg, decimal PricePerKg);

public record PurchaseDto(
    int Id, string ReferenceNo,
    string? SupplierName, string? SupplierPhone,
    decimal TotalAmount, string? Note,
    DateTime PurchaseDate, string CreatedBy,
    List<ItemDto> Items
);

public record ItemDto(
    int Id, int CashewTypeId, string CashewTypeName,
    decimal QtyKg, decimal PricePerKg, decimal Total
);

// Sale — multiple items, no paid amount, optional name/phone
public record CreateSaleRequest(
    string? CustomerName,
    string? CustomerPhone,
    string? Note,
    DateTime SaleDate,
    List<CreateItemRequest> Items
);

public record SaleDto(
    int Id, string ReferenceNo,
    string? CustomerName, string? CustomerPhone,
    decimal TotalAmount, string? Note,
    DateTime SaleDate, string CreatedBy,
    List<ItemDto> Items
);

// Dashboard
public record DashboardDto(
    decimal TotalPurchaseKg,
    decimal TotalPurchasePrice,
    decimal TotalSaleKg,
    decimal TotalSalePrice,
    decimal Profit,
    int PurchaseCount,
    int SaleCount,
    List<ChartData> PurchaseChart,
    List<ChartData> SaleChart
);
public record ChartData(string Label, decimal Value);

// Paginated
public record PagedResult<T>(
    List<T> Data,
    int Total,
    int Page,
    int PageSize,
    int TotalPages,
    decimal SummaryKg,      // ← new
    decimal SummaryPrice    // ← new
);

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
