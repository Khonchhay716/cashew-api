// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using CashewNuts.Application.DTOs;
// using CashewNuts.Domain.Entities;
// using CashewNuts.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Microsoft.IdentityModel.Tokens;

// namespace CashewNuts.Application.Services;

// public class AuthService
// {
//     private readonly AppDbContext _db;
//     private readonly IConfiguration _config;
//     public AuthService(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

//     public async Task<LoginResponse?> LoginAsync(LoginRequest req)
//     {
//         var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive);
//         if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return null;
//         return new LoginResponse(GenerateToken(user), user.Name, user.Role);
//     }

//     public async Task<bool> RegisterAsync(RegisterRequest req)
//     {
//         if (await _db.Users.AnyAsync(u => u.Email == req.Email)) return false;
//         _db.Users.Add(new User { Name = req.Name, Email = req.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password), Role = req.Role });
//         await _db.SaveChangesAsync();
//         return true;
//     }

//     private string GenerateToken(User user)
//     {
//         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
//         var token = new JwtSecurityToken(
//             issuer: _config["JwtSettings:Issuer"],
//             audience: _config["JwtSettings:Audience"],
//             claims: new[] {
//                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//                 new Claim(ClaimTypes.Name, user.Name),
//                 new Claim(ClaimTypes.Email, user.Email),
//                 new Claim(ClaimTypes.Role, user.Role)
//             },
//             expires: DateTime.UtcNow.AddHours(12),
//             signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
//         );
//         return new JwtSecurityTokenHandler().WriteToken(token);
//     }
// }



using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CashewNuts.Application.DTOs;
using CashewNuts.Domain.Entities;
using CashewNuts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CashewNuts.Application.Services;

public class AuthService
{
    private readonly AppDbContext  _db;
    private readonly IConfiguration _config;
    private readonly EmailService  _email;

    public AuthService(AppDbContext db, IConfiguration config, EmailService email)
    {
        _db     = db;
        _config = config;
        _email  = email;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return null;
        return new LoginResponse(GenerateToken(user), user.Name, user.Role);
    }

    public async Task<bool> RegisterAsync(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email)) return false;
        _db.Users.Add(new User
        {
            Name         = req.Name,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role         = req.Role
        });
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Forgot password — send email ───────────────────────────────────────
    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user is null) return false; // silently fail — don't reveal if email exists

        // Invalidate old tokens
        var oldTokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync();
        oldTokens.ForEach(t => t.IsUsed = true);

        // Create new token
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId    = user.Id,
            Token     = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            IsUsed    = false
        });
        await _db.SaveChangesAsync();

        // Send email
        await _email.SendResetPasswordEmailAsync(user.Email, user.Name, token);
        return true;
    }

    // ── Reset password — verify token + update password ───────────────────
    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow);

        if (resetToken is null) return false;

        resetToken.User!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        resetToken.IsUsed             = true;
        await _db.SaveChangesAsync();
        return true;
    }

    private string GenerateToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
        var token = new JwtSecurityToken(
            issuer:             _config["JwtSettings:Issuer"],
            audience:           _config["JwtSettings:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Name),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role)
            },
            expires:            DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}