// // using CashewNuts.Application.DTOs;
// // using CashewNuts.Domain.Entities;
// // using CashewNuts.Infrastructure.Data;
// // using Microsoft.EntityFrameworkCore;

// // namespace CashewNuts.Application.Services;

// // public class CashewTypeService
// // {
// //     private readonly AppDbContext _db;
// //     public CashewTypeService(AppDbContext db) => _db = db;

// //     public async Task<List<CashewTypeDto>> GetAllAsync()
// //         => await _db.CashewTypes.Where(t => t.IsActive)
// //             .Select(t => new CashewTypeDto(t.Id, t.Name, t.DefaultPrice, t.Description, t.IsActive))
// //             .ToListAsync();

// //     public async Task<CashewTypeDto> CreateAsync(CreateCashewTypeRequest req)
// //     {
// //         var t = new CashewType { Name = req.Name, DefaultPrice = req.DefaultPrice, Description = req.Description };
// //         _db.CashewTypes.Add(t);
// //         await _db.SaveChangesAsync();
// //         return new CashewTypeDto(t.Id, t.Name, t.DefaultPrice, t.Description, t.IsActive);
// //     }

// //     public async Task<CashewTypeDto?> UpdateAsync(int id, UpdateCashewTypeRequest req)
// //     {
// //         var t = await _db.CashewTypes.FindAsync(id);
// //         if (t == null) return null;
// //         t.Name = req.Name; t.DefaultPrice = req.DefaultPrice; t.Description = req.Description;
// //         await _db.SaveChangesAsync();
// //         return new CashewTypeDto(t.Id, t.Name, t.DefaultPrice, t.Description, t.IsActive);
// //     }

// //     public async Task<bool> DeleteAsync(int id)
// //     {
// //         var t = await _db.CashewTypes.FindAsync(id);
// //         if (t == null) return false;
// //         t.IsActive = false;
// //         await _db.SaveChangesAsync();
// //         return true;
// //     }
// // }

using CashewNuts.Application.DTOs;
using CashewNuts.Domain.Entities;
using CashewNuts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashewNuts.Application.Services;

public class CashewTypeService
{
    private readonly AppDbContext _db;
    public CashewTypeService(AppDbContext db) => _db = db;

    public async Task<List<CashewTypeDto>> GetAllAsync(int userId)
        => await _db.CashewTypes
            .Where(t => t.IsActive && t.UserId == userId)
            .Select(t => new CashewTypeDto(t.Id, t.Name, t.DefaultPrice, t.Description, t.IsActive))
            .ToListAsync();

    public async Task<CashewTypeDto> CreateAsync(CreateCashewTypeRequest req, int userId)
    {
        var t = new CashewType
        {
            Name         = req.Name,
            DefaultPrice = req.DefaultPrice,
            Description  = req.Description,
            UserId       = userId
        };
        _db.CashewTypes.Add(t);
        await _db.SaveChangesAsync();
        return new CashewTypeDto(t.Id, t.Name, t.DefaultPrice, t.Description, t.IsActive);
    }

    public async Task<CashewTypeDto?> UpdateAsync(int id, UpdateCashewTypeRequest req, int userId)
    {
        var t = await _db.CashewTypes.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (t is null) return null;
        t.Name = req.Name; t.DefaultPrice = req.DefaultPrice; t.Description = req.Description;
        await _db.SaveChangesAsync();
        return new CashewTypeDto(t.Id, t.Name, t.DefaultPrice, t.Description, t.IsActive);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var t = await _db.CashewTypes.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (t is null) return false;
        t.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }
}