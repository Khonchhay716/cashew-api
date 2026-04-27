// using System.Globalization;
// using CashewNuts.Application.DTOs;
// using CashewNuts.Domain.Entities;
// using CashewNuts.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;

// namespace CashewNuts.Application.Services;

// public class PurchaseService
// {
//     private readonly AppDbContext _db;
//     public PurchaseService(AppDbContext db) => _db = db;

//     private static readonly TimeSpan CamOffset = TimeSpan.FromHours(7);

//     private static (DateTime From, DateTime To) ToUtc(DateTime fromLocal, DateTime toLocal) => (
//         DateTime.SpecifyKind(fromLocal - CamOffset, DateTimeKind.Utc),
//         DateTime.SpecifyKind(toLocal - CamOffset + TimeSpan.FromDays(1), DateTimeKind.Utc)
//     );

//     private static DateTime ParseDate(string? input, DateTime fallback) =>
//         DateTime.TryParseExact(input, "yyyy-MM-dd",
//             CultureInfo.InvariantCulture, DateTimeStyles.None, out var r) ? r : fallback;

//     public async Task<PagedResult<PurchaseDto>> GetPagedAsync(
//         int page = 1, int pageSize = 10,
//         string? from = null, string? to = null,
//         bool all = false)
//     {
//         // ── Base query (no pagination, no includes) ────────────────────────
//         var baseQuery = _db.Purchases.AsQueryable();

//         if (!all)
//         {
//             var nowCam = DateTime.UtcNow.Add(CamOffset);
//             var (fromUtc, toUtc) = ToUtc(ParseDate(from, nowCam.Date), ParseDate(to, nowCam.Date));
//             baseQuery = baseQuery.Where(p => p.PurchaseDate >= fromUtc && p.PurchaseDate < toUtc);
//         }

//         // ── Summary — pure SQL subquery, safe at any scale ─────────────────
//         var total = await baseQuery.CountAsync();
//         var summaryPrice = await baseQuery.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
//         var summaryKg = await _db.PurchaseItems
//             .Where(i => baseQuery.Select(p => p.Id).Contains(i.PurchaseId))
//             .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

//         // ── Paginated data ─────────────────────────────────────────────────
//         var data = await baseQuery
//             .Include(p => p.Items).ThenInclude(i => i.CashewType)
//             .Include(p => p.User)
//             .OrderByDescending(p => p.PurchaseDate)
//             .Skip((page - 1) * pageSize)
//             .Take(pageSize)
//             .Select(p => Map(p))
//             .ToListAsync();

//         return new PagedResult<PurchaseDto>(
//             data, total, page, pageSize,
//             (int)Math.Ceiling((double)total / pageSize),
//             summaryKg,
//             summaryPrice);
//     }

//     public async Task<PurchaseDto> CreateAsync(CreatePurchaseRequest req, int userId)
//     {
//         var count = await _db.Purchases.CountAsync() + 1;
//         var purchase = new Purchase
//         {
//             ReferenceNo = $"PO-{count:D4}",
//             SupplierName = req.SupplierName,
//             SupplierPhone = req.SupplierPhone,
//             Note = req.Note,
//             PurchaseDate = DateTime.SpecifyKind(req.PurchaseDate, DateTimeKind.Utc),
//             UserId = userId,
//             Items = req.Items.Select(i => new PurchaseItem
//             {
//                 CashewTypeId = i.CashewTypeId,
//                 QtyKg = i.QtyKg,
//                 PricePerKg = i.PricePerKg,
//                 Total = i.QtyKg * i.PricePerKg
//             }).ToList()
//         };

//         purchase.TotalAmount = purchase.Items.Sum(i => i.Total);
//         _db.Purchases.Add(purchase);
//         await _db.SaveChangesAsync();

//         return await _db.Purchases
//             .Include(p => p.Items).ThenInclude(i => i.CashewType)
//             .Include(p => p.User)
//             .Where(p => p.Id == purchase.Id)
//             .Select(p => Map(p))
//             .FirstAsync();
//     }

//     public async Task<bool> DeleteAsync(int id)
//     {
//         var p = await _db.Purchases.FindAsync(id);
//         if (p is null) return false;
//         _db.Purchases.Remove(p);
//         await _db.SaveChangesAsync();
//         return true;
//     }

//     private static PurchaseDto Map(Purchase p) => new(
//         p.Id, p.ReferenceNo, p.SupplierName, p.SupplierPhone,
//         p.TotalAmount, p.Note, p.PurchaseDate, p.User?.Name ?? "",
//         p.Items.Select(i => new ItemDto(
//             i.Id, i.CashewTypeId, i.CashewType?.Name ?? "",
//             i.QtyKg, i.PricePerKg, i.Total)).ToList()
//     );
// }




using System.Globalization;
using CashewNuts.Application.DTOs;
using CashewNuts.Domain.Entities;
using CashewNuts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashewNuts.Application.Services;

public class PurchaseService
{
    private readonly AppDbContext _db;
    public PurchaseService(AppDbContext db) => _db = db;

    private static readonly TimeSpan CamOffset = TimeSpan.FromHours(7);

    private static (DateTime From, DateTime To) ToUtc(DateTime fromLocal, DateTime toLocal) => (
        DateTime.SpecifyKind(fromLocal - CamOffset, DateTimeKind.Utc),
        DateTime.SpecifyKind(toLocal - CamOffset + TimeSpan.FromDays(1), DateTimeKind.Utc)
    );

    private static DateTime ParseDate(string? input, DateTime fallback) =>
        DateTime.TryParseExact(input, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var r) ? r : fallback;

    public async Task<PagedResult<PurchaseDto>> GetPagedAsync(
        int userId,
        int page = 1, int pageSize = 10,
        string? from = null, string? to = null,
        bool all = false)
    {
        // ── Base query — always scoped to current user ─────────────────────
        var baseQuery = _db.Purchases
            .Where(p => p.UserId == userId)
            .AsQueryable();

        if (!all)
        {
            var nowCam = DateTime.UtcNow.Add(CamOffset);
            var (fromUtc, toUtc) = ToUtc(ParseDate(from, nowCam.Date), ParseDate(to, nowCam.Date));
            baseQuery = baseQuery.Where(p => p.PurchaseDate >= fromUtc && p.PurchaseDate < toUtc);
        }

        // ── Summary — pure SQL subquery ────────────────────────────────────
        var total = await baseQuery.CountAsync();
        var summaryPrice = await baseQuery.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
        var summaryKg = await _db.PurchaseItems
            .Where(i => baseQuery.Select(p => p.Id).Contains(i.PurchaseId))
            .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

        // ── Paginated data ─────────────────────────────────────────────────
        var data = await baseQuery
            .Include(p => p.Items).ThenInclude(i => i.CashewType)
            .Include(p => p.User)
            .OrderByDescending(p => p.PurchaseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => Map(p))
            .ToListAsync();

        return new PagedResult<PurchaseDto>(
            data, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize),
            summaryKg, summaryPrice);
    }

    public async Task<PurchaseDto> CreateAsync(CreatePurchaseRequest req, int userId)
    {
        var count = await _db.Purchases.CountAsync() + 1;
        var purchase = new Purchase
        {
            ReferenceNo = $"PO-{count:D4}",
            SupplierName = req.SupplierName,
            SupplierPhone = req.SupplierPhone,
            Note = req.Note,
            PurchaseDate = DateTime.SpecifyKind(req.PurchaseDate, DateTimeKind.Utc),
            UserId = userId,
            Items = req.Items.Select(i => new PurchaseItem
            {
                CashewTypeId = i.CashewTypeId,
                QtyKg = i.QtyKg,
                PricePerKg = i.PricePerKg,
                Total = i.QtyKg * i.PricePerKg
            }).ToList()
        };

        purchase.TotalAmount = purchase.Items.Sum(i => i.Total);
        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();

        return await _db.Purchases
            .Include(p => p.Items).ThenInclude(i => i.CashewType)
            .Include(p => p.User)
            .Where(p => p.Id == purchase.Id)
            .Select(p => Map(p))
            .FirstAsync();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        // ✅ Only delete if owned by current user
        var p = await _db.Purchases.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (p is null) return false;
        _db.Purchases.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }

    private static PurchaseDto Map(Purchase p) => new(
        p.Id, p.ReferenceNo, p.SupplierName, p.SupplierPhone,
        p.TotalAmount, p.Note, p.PurchaseDate, p.User?.Name ?? "",
        p.Items.Select(i => new ItemDto(
            i.Id, i.CashewTypeId, i.CashewType?.Name ?? "",
            i.QtyKg, i.PricePerKg, i.Total)).ToList()
    );
}