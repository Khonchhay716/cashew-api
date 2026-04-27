// using CashewNuts.Application.DTOs;
// using CashewNuts.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;

// namespace CashewNuts.Application.Services;

// public class DashboardService
// {
//     private readonly AppDbContext _db;
//     public DashboardService(AppDbContext db) => _db = db;

//     private static readonly TimeSpan CamOffset = TimeSpan.FromHours(7);

//     private static (DateTime From, DateTime To) ToUtc(DateTime fromLocal, DateTime toLocal) => (
//         DateTime.SpecifyKind(fromLocal - CamOffset, DateTimeKind.Utc),
//         DateTime.SpecifyKind(toLocal - CamOffset + TimeSpan.FromDays(1), DateTimeKind.Utc)
//     );

//     public async Task<DashboardDto> GetAsync(DateTime? fromLocal, DateTime? toLocal)
//     {
//         bool isAll = fromLocal is null || toLocal is null;

//         // ── Base queryables (lean — no includes) ──────────────────────────
//         var pQuery = _db.Purchases.AsQueryable();
//         var sQuery = _db.Sales.AsQueryable();

//         if (!isAll)
//         {
//             var (from, to) = ToUtc(fromLocal!.Value, toLocal!.Value);
//             pQuery = pQuery.Where(p => p.PurchaseDate >= from && p.PurchaseDate < to);
//             sQuery = sQuery.Where(s => s.SaleDate     >= from && s.SaleDate     < to);
//         }

//         // ── Summary: pure SQL subquery — safe at any scale ─────────────────
//         var pPrice = await pQuery.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
//         var pCount = await pQuery.CountAsync();
//         var pKg    = await _db.PurchaseItems
//             .Where(i => pQuery.Select(p => p.Id).Contains(i.PurchaseId))
//             .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

//         var sPrice = await sQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
//         var sCount = await sQuery.CountAsync();
//         var sKg    = await _db.SaleItems
//             .Where(i => sQuery.Select(s => s.Id).Contains(i.SaleId))
//             .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

//         // ── Chart ──────────────────────────────────────────────────────────
//         var nowCam = DateTime.UtcNow.Add(CamOffset);
//         var pChart = new List<ChartData>();
//         var sChart = new List<ChartData>();

//         if (isAll)
//         {
//             // Last 6 months grouped by month — 2 DB calls, max 6 rows each
//             var monthFrom = ToUtc(
//                 new DateTime(nowCam.Year, nowCam.Month, 1).AddMonths(-5),
//                 new DateTime(nowCam.Year, nowCam.Month, 1).AddMonths(-5)
//             ).From;
//             var monthTo = ToUtc(nowCam.Date, nowCam.Date).To;

//             var pMonthly = await _db.Purchases
//                 .Where(p => p.PurchaseDate >= monthFrom && p.PurchaseDate < monthTo)
//                 .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
//                 .Select(g => new
//                 {
//                     g.Key.Year,
//                     g.Key.Month,
//                     Total = g.Sum(p => (decimal?)p.TotalAmount) ?? 0
//                 })
//                 .OrderBy(g => g.Year).ThenBy(g => g.Month)
//                 .ToListAsync();

//             var sMonthly = await _db.Sales
//                 .Where(s => s.SaleDate >= monthFrom && s.SaleDate < monthTo)
//                 .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
//                 .Select(g => new
//                 {
//                     g.Key.Year,
//                     g.Key.Month,
//                     Total = g.Sum(s => (decimal?)s.TotalAmount) ?? 0
//                 })
//                 .OrderBy(g => g.Year).ThenBy(g => g.Month)
//                 .ToListAsync();

//             for (int i = 5; i >= 0; i--)
//             {
//                 var month = nowCam.AddMonths(-i);
//                 var label = month.ToString("MM/yy");
//                 var pv    = pMonthly.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Total ?? 0;
//                 var sv    = sMonthly.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Total ?? 0;
//                 pChart.Add(new ChartData(label, pv));
//                 sChart.Add(new ChartData(label, sv));
//             }
//         }
//         else
//         {
//             // Last 7 days grouped by day — 2 DB calls, max 7 rows each
//             var chartFrom = ToUtc(nowCam.Date.AddDays(-6), nowCam.Date.AddDays(-6)).From;
//             var chartTo   = ToUtc(nowCam.Date, nowCam.Date).To;

//             var pByDay = await _db.Purchases
//                 .Where(p => p.PurchaseDate >= chartFrom && p.PurchaseDate < chartTo)
//                 .GroupBy(p => p.PurchaseDate.Date)
//                 .Select(g => new { Date = g.Key, Total = g.Sum(p => (decimal?)p.TotalAmount) ?? 0 })
//                 .ToListAsync();

//             var sByDay = await _db.Sales
//                 .Where(s => s.SaleDate >= chartFrom && s.SaleDate < chartTo)
//                 .GroupBy(s => s.SaleDate.Date)
//                 .Select(g => new { Date = g.Key, Total = g.Sum(s => (decimal?)s.TotalAmount) ?? 0 })
//                 .ToListAsync();

//             for (int i = 6; i >= 0; i--)
//             {
//                 // ✅ Convert UTC date back to Cambodia local before matching
//                 var day = nowCam.Date.AddDays(-i);
//                 var pv  = pByDay.FirstOrDefault(x => x.Date.Add(CamOffset).Date == day)?.Total ?? 0;
//                 var sv  = sByDay.FirstOrDefault(x => x.Date.Add(CamOffset).Date == day)?.Total ?? 0;
//                 pChart.Add(new ChartData(day.ToString("dd/MM"), pv));
//                 sChart.Add(new ChartData(day.ToString("dd/MM"), sv));
//             }
//         }

//         return new DashboardDto(pKg, pPrice, sKg, sPrice, sPrice - pPrice, pCount, sCount, pChart, sChart);
//     }
// }



using CashewNuts.Application.DTOs;
using CashewNuts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashewNuts.Application.Services;

public class DashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    private static readonly TimeSpan CamOffset = TimeSpan.FromHours(7);

    private static (DateTime From, DateTime To) ToUtc(DateTime fromLocal, DateTime toLocal) => (
        DateTime.SpecifyKind(fromLocal - CamOffset, DateTimeKind.Utc),
        DateTime.SpecifyKind(toLocal - CamOffset + TimeSpan.FromDays(1), DateTimeKind.Utc)
    );

    public async Task<DashboardDto> GetAsync(int userId, DateTime? fromLocal, DateTime? toLocal)
    {
        bool isAll = fromLocal is null || toLocal is null;

        // ── Base queryables — always scoped to current user ────────────────
        var pQuery = _db.Purchases.Where(p => p.UserId == userId).AsQueryable();
        var sQuery = _db.Sales.Where(s => s.UserId == userId).AsQueryable();

        if (!isAll)
        {
            var (from, to) = ToUtc(fromLocal!.Value, toLocal!.Value);
            pQuery = pQuery.Where(p => p.PurchaseDate >= from && p.PurchaseDate < to);
            sQuery = sQuery.Where(s => s.SaleDate     >= from && s.SaleDate     < to);
        }

        // ── Summary ────────────────────────────────────────────────────────
        var pPrice = await pQuery.SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
        var pCount = await pQuery.CountAsync();
        var pKg    = await _db.PurchaseItems
            .Where(i => pQuery.Select(p => p.Id).Contains(i.PurchaseId))
            .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

        var sPrice = await sQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
        var sCount = await sQuery.CountAsync();
        var sKg    = await _db.SaleItems
            .Where(i => sQuery.Select(s => s.Id).Contains(i.SaleId))
            .SumAsync(i => (decimal?)i.QtyKg) ?? 0;

        // ── Chart ──────────────────────────────────────────────────────────
        var nowCam = DateTime.UtcNow.Add(CamOffset);
        var pChart = new List<ChartData>();
        var sChart = new List<ChartData>();

        if (isAll)
        {
            var monthFrom = ToUtc(
                new DateTime(nowCam.Year, nowCam.Month, 1).AddMonths(-5),
                new DateTime(nowCam.Year, nowCam.Month, 1).AddMonths(-5)
            ).From;
            var monthTo = ToUtc(nowCam.Date, nowCam.Date).To;

            var pMonthly = await _db.Purchases
                .Where(p => p.UserId == userId && p.PurchaseDate >= monthFrom && p.PurchaseDate < monthTo)
                .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => (decimal?)p.TotalAmount) ?? 0 })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var sMonthly = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= monthFrom && s.SaleDate < monthTo)
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(s => (decimal?)s.TotalAmount) ?? 0 })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            for (int i = 5; i >= 0; i--)
            {
                var month = nowCam.AddMonths(-i);
                var label = month.ToString("MM/yy");
                var pv    = pMonthly.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Total ?? 0;
                var sv    = sMonthly.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Total ?? 0;
                pChart.Add(new ChartData(label, pv));
                sChart.Add(new ChartData(label, sv));
            }
        }
        else
        {
            var chartFrom = ToUtc(nowCam.Date.AddDays(-6), nowCam.Date.AddDays(-6)).From;
            var chartTo   = ToUtc(nowCam.Date, nowCam.Date).To;

            var pByDay = await _db.Purchases
                .Where(p => p.UserId == userId && p.PurchaseDate >= chartFrom && p.PurchaseDate < chartTo)
                .GroupBy(p => p.PurchaseDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(p => (decimal?)p.TotalAmount) ?? 0 })
                .ToListAsync();

            var sByDay = await _db.Sales
                .Where(s => s.UserId == userId && s.SaleDate >= chartFrom && s.SaleDate < chartTo)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(s => (decimal?)s.TotalAmount) ?? 0 })
                .ToListAsync();

            for (int i = 6; i >= 0; i--)
            {
                var day = nowCam.Date.AddDays(-i);
                var pv  = pByDay.FirstOrDefault(x => x.Date.Add(CamOffset).Date == day)?.Total ?? 0;
                var sv  = sByDay.FirstOrDefault(x => x.Date.Add(CamOffset).Date == day)?.Total ?? 0;
                pChart.Add(new ChartData(day.ToString("dd/MM"), pv));
                sChart.Add(new ChartData(day.ToString("dd/MM"), sv));
            }
        }

        return new DashboardDto(pKg, pPrice, sKg, sPrice, sPrice - pPrice, pCount, sCount, pChart, sChart);
    }
}