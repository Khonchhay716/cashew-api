// using System.Globalization;
// using CashewNuts.Application.DTOs;
// using CashewNuts.Domain.Entities;
// using CashewNuts.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;

// namespace CashewNuts.Application.Services;

// public class SaleService
// {
//     private readonly AppDbContext _db;
//     public SaleService(AppDbContext db) => _db = db;

//     private static readonly TimeSpan CamOffset = TimeSpan.FromHours(7);

//     private static (DateTime From, DateTime To) ToUtc(DateTime fromLocal, DateTime toLocal) => (
//         DateTime.SpecifyKind(fromLocal - CamOffset, DateTimeKind.Utc),
//         DateTime.SpecifyKind(toLocal - CamOffset + TimeSpan.FromDays(1), DateTimeKind.Utc)
//     );

//     private static DateTime ParseDate(string? input, DateTime fallback) =>
//         DateTime.TryParseExact(input, "yyyy-MM-dd",
//             CultureInfo.InvariantCulture, DateTimeStyles.None, out var r) ? r : fallback;

//     public async Task<PagedResult<SaleDto>> GetPagedAsync(
//         int page = 1, int pageSize = 10,
//         string? from = null, string? to = null,
//         bool all = false)
//     {
//         // ── Base query (no pagination, no includes) ────────────────────────
//         var baseQuery = _db.Sales.AsQueryable();

//         if (!all)
//         {
//             var nowCam = DateTime.UtcNow.Add(CamOffset);
//             var (fromUtc, toUtc) = ToUtc(ParseDate(from, nowCam.Date), ParseDate(to, nowCam.Date));
//             baseQuery = baseQuery.Where(s => s.SaleDate >= fromUtc && s.SaleDate < toUtc);
//         }

//         // ── Summary — pure SQL subquery, safe at any scale ─────────────────
//         var total = await baseQuery.CountAsync();
//         var summaryPrice = await baseQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
//         var summaryKg = await _db.SaleItems
//             .Where(i => baseQuery.Select(s => s.Id).Contains(i.SaleId))
//             .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

//         // ── Paginated data ─────────────────────────────────────────────────
//         var data = await baseQuery
//             .Include(s => s.Items).ThenInclude(i => i.CashewType)
//             .Include(s => s.User)
//             .OrderByDescending(s => s.SaleDate)
//             .Skip((page - 1) * pageSize)
//             .Take(pageSize)
//             .Select(s => Map(s))
//             .ToListAsync();

//         return new PagedResult<SaleDto>(
//             data, total, page, pageSize,
//             (int)Math.Ceiling((double)total / pageSize),
//             summaryKg,
//             summaryPrice);
//     }

//     public async Task<SaleDto> CreateAsync(CreateSaleRequest req, int userId)
//     {
//         var count = await _db.Sales.CountAsync() + 1;
//         var sale = new Sale
//         {
//             ReferenceNo = $"SO-{count:D4}",
//             CustomerName = req.CustomerName,
//             CustomerPhone = req.CustomerPhone,
//             Note = req.Note,
//             SaleDate = DateTime.SpecifyKind(req.SaleDate, DateTimeKind.Utc),
//             UserId = userId,
//             Items = req.Items.Select(i => new SaleItem
//             {
//                 CashewTypeId = i.CashewTypeId,
//                 QtyKg = i.QtyKg,
//                 PricePerKg = i.PricePerKg,
//                 Total = i.QtyKg * i.PricePerKg
//             }).ToList()
//         };

//         sale.TotalAmount = sale.Items.Sum(i => i.Total);
//         _db.Sales.Add(sale);
//         await _db.SaveChangesAsync();

//         return await _db.Sales
//             .Include(s => s.Items).ThenInclude(i => i.CashewType)
//             .Include(s => s.User)
//             .Where(s => s.Id == sale.Id)
//             .Select(s => Map(s))
//             .FirstAsync();
//     }

//     public async Task<bool> DeleteAsync(int id)
//     {
//         var s = await _db.Sales.FindAsync(id);
//         if (s is null) return false;
//         _db.Sales.Remove(s);
//         await _db.SaveChangesAsync();
//         return true;
//     }

//     private static SaleDto Map(Sale s) => new(
//         s.Id, s.ReferenceNo, s.CustomerName, s.CustomerPhone,
//         s.TotalAmount, s.Note, s.SaleDate, s.User?.Name ?? "",
//         s.Items.Select(i => new ItemDto(
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

public class SaleService
{
    private readonly AppDbContext _db;
    public SaleService(AppDbContext db) => _db = db;

    private static readonly TimeSpan CamOffset = TimeSpan.FromHours(7);

    private static (DateTime From, DateTime To) ToUtc(DateTime fromLocal, DateTime toLocal) => (
        DateTime.SpecifyKind(fromLocal - CamOffset, DateTimeKind.Utc),
        DateTime.SpecifyKind(toLocal - CamOffset + TimeSpan.FromDays(1), DateTimeKind.Utc)
    );

    private static DateTime ParseDate(string? input, DateTime fallback) =>
        DateTime.TryParseExact(input, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var r) ? r : fallback;

    public async Task<PagedResult<SaleDto>> GetPagedAsync(
        int userId,
        int page = 1, int pageSize = 10,
        string? from = null, string? to = null,
        bool all = false)
    {
        // ── Base query — always scoped to current user ─────────────────────
        var baseQuery = _db.Sales
            .Where(s => s.UserId == userId)
            .AsQueryable();

        if (!all)
        {
            var nowCam = DateTime.UtcNow.Add(CamOffset);
            var (fromUtc, toUtc) = ToUtc(ParseDate(from, nowCam.Date), ParseDate(to, nowCam.Date));
            baseQuery = baseQuery.Where(s => s.SaleDate >= fromUtc && s.SaleDate < toUtc);
        }

        // ── Summary — pure SQL subquery ────────────────────────────────────
        var total        = await baseQuery.CountAsync();
        var summaryPrice = await baseQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
        var summaryKg    = await _db.SaleItems
            .Where(i => baseQuery.Select(s => s.Id).Contains(i.SaleId))
            .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

        // ── Paginated data ─────────────────────────────────────────────────
        var data = await baseQuery
            .Include(s => s.Items).ThenInclude(i => i.CashewType)
            .Include(s => s.User)
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => Map(s))
            .ToListAsync();

        return new PagedResult<SaleDto>(
            data, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize),
            summaryKg, summaryPrice);
    }

    public async Task<SaleDto> CreateAsync(CreateSaleRequest req, int userId)
    {
        var count = await _db.Sales.CountAsync() + 1;
        var sale  = new Sale
        {
            ReferenceNo   = $"SO-{count:D4}",
            CustomerName  = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            Note          = req.Note,
            SaleDate      = DateTime.SpecifyKind(req.SaleDate, DateTimeKind.Utc),
            UserId        = userId,
            Items         = req.Items.Select(i => new SaleItem
            {
                CashewTypeId = i.CashewTypeId,
                QtyKg        = i.QtyKg,
                PricePerKg   = i.PricePerKg,
                Total        = i.QtyKg * i.PricePerKg
            }).ToList()
        };

        sale.TotalAmount = sale.Items.Sum(i => i.Total);
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        return await _db.Sales
            .Include(s => s.Items).ThenInclude(i => i.CashewType)
            .Include(s => s.User)
            .Where(s => s.Id == sale.Id)
            .Select(s => Map(s))
            .FirstAsync();
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        // ✅ Only delete if owned by current user
        var s = await _db.Sales.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (s is null) return false;
        _db.Sales.Remove(s);
        await _db.SaveChangesAsync();
        return true;
    }

    private static SaleDto Map(Sale s) => new(
        s.Id, s.ReferenceNo, s.CustomerName, s.CustomerPhone,
        s.TotalAmount, s.Note, s.SaleDate, s.User?.Name ?? "",
        s.Items.Select(i => new ItemDto(
            i.Id, i.CashewTypeId, i.CashewType?.Name ?? "",
            i.QtyKg, i.PricePerKg, i.Total)).ToList()
    );
}