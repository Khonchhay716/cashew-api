// using System.Security.Claims;
// using CashewNuts.Application.DTOs;
// using CashewNuts.Application.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace CashewNuts.API.Controllers;

// [ApiController][Route("api/[controller]")]
// public class AuthController : ControllerBase
// {
//     private readonly AuthService _s;
//     public AuthController(AuthService s) => _s = s;

//     [HttpPost("login")]
//     public async Task<IActionResult> Login(LoginRequest r)
//     {
//         var res = await _s.LoginAsync(r);
//         return res == null ? Unauthorized(new { message = "Email ឬ Password ខុស!" }) : Ok(res);
//     }

//     [HttpPost("register")]
//     public async Task<IActionResult> Register(RegisterRequest r)
//     {
//         if (!await _s.RegisterAsync(r)) return BadRequest(new { message = "Email មានហើយ!" });
//         return Ok(new { message = "Register ជោគជ័យ!" });
//     }
// }

// [ApiController][Route("api/[controller]")][Authorize]
// public class MasterController : ControllerBase
// {
//     private readonly CashewTypeService _s;
//     public MasterController(CashewTypeService s) => _s = s;

//     [HttpGet]    public async Task<IActionResult> GetAll()                               => Ok(await _s.GetAllAsync());
//     [HttpPost]   public async Task<IActionResult> Create(CreateCashewTypeRequest r)      => Ok(await _s.CreateAsync(r));
//     [HttpPut("{id}")] public async Task<IActionResult> Update(int id, UpdateCashewTypeRequest r)
//     {
//         var res = await _s.UpdateAsync(id, r);
//         return res == null ? NotFound() : Ok(res);
//     }
//     [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
//     {
//         if (!await _s.DeleteAsync(id)) return NotFound();
//         return Ok();
//     }
// }

// [ApiController][Route("api/[controller]")][Authorize]
// public class PurchaseController : ControllerBase
// {
//     private readonly PurchaseService _s;
//     public PurchaseController(PurchaseService s) => _s = s;

//     [HttpGet]
//     public async Task<IActionResult> GetAll(
//         [FromQuery] int page     = 1,
//         [FromQuery] int pageSize = 10,
//         [FromQuery] string? from = null,
//         [FromQuery] string? to   = null,
//         [FromQuery] bool all     = false)   // ← added
//         => Ok(await _s.GetPagedAsync(page, pageSize, from, to, all));

//     [HttpPost]
//     public async Task<IActionResult> Create(CreatePurchaseRequest r)
//     {
//         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
//         return Ok(await _s.CreateAsync(r, userId));
//     }

//     [HttpDelete("{id}")]
//     public async Task<IActionResult> Delete(int id)
//     {
//         if (!await _s.DeleteAsync(id)) return NotFound();
//         return Ok();
//     }
// }

// [ApiController][Route("api/[controller]")][Authorize]
// public class SaleController : ControllerBase
// {
//     private readonly SaleService _s;
//     public SaleController(SaleService s) => _s = s;

//     [HttpGet]
//     public async Task<IActionResult> GetAll(
//         [FromQuery] int page     = 1,
//         [FromQuery] int pageSize = 10,
//         [FromQuery] string? from = null,
//         [FromQuery] string? to   = null,
//         [FromQuery] bool all     = false)   // ← added
//         => Ok(await _s.GetPagedAsync(page, pageSize, from, to, all));

//     [HttpPost]
//     public async Task<IActionResult> Create(CreateSaleRequest r)
//     {
//         var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
//         return Ok(await _s.CreateAsync(r, userId));
//     }

//     [HttpDelete("{id}")]
//     public async Task<IActionResult> Delete(int id)
//     {
//         if (!await _s.DeleteAsync(id)) return NotFound();
//         return Ok();
//     }
// }

// [ApiController][Route("api/[controller]")][Authorize]
// public class DashboardController : ControllerBase
// {
//     private readonly DashboardService _s;
//     public DashboardController(DashboardService s) => _s = s;

//     [HttpGet]
//     public async Task<IActionResult> Get(
//         [FromQuery] string? from = null,
//         [FromQuery] string? to   = null,
//         [FromQuery] bool all     = false)
//     {
//         if (all) return Ok(await _s.GetAsync(null, null));

//         var nowCam = DateTime.UtcNow.Add(TimeSpan.FromHours(7));

//         if (!TryParseDate(from, nowCam.Date, out var f))
//             return BadRequest("Invalid 'from' date. Use yyyy-MM-dd.");

//         if (!TryParseDate(to, nowCam.Date, out var t))
//             return BadRequest("Invalid 'to' date. Use yyyy-MM-dd.");

//         if (f > t)
//             return BadRequest("'from' must not be after 'to'.");

//         return Ok(await _s.GetAsync(f, t));
//     }

//     private static bool TryParseDate(string? input, DateTime fallback, out DateTime result)
//     {
//         if (string.IsNullOrWhiteSpace(input)) { result = fallback; return true; }
//         return DateTime.TryParseExact(input, "yyyy-MM-dd",
//             System.Globalization.CultureInfo.InvariantCulture,
//             System.Globalization.DateTimeStyles.None, out result);
//     }
// }




using System.Security.Claims;
using CashewNuts.Application.DTOs;
using CashewNuts.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashewNuts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _s;
    public AuthController(AuthService s) => _s = s;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var res = await _s.LoginAsync(r);
        return res == null ? Unauthorized(new { message = "Email ឬ Password ខុស!" }) : Ok(res);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest r)
    {
        if (!await _s.RegisterAsync(r)) return BadRequest(new { message = "Email មានហើយ!" });
        return Ok(new { message = "Register ជោគជ័យ!" });
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest r)
    {
        await _s.ForgotPasswordAsync(r.Email);
        // Always return OK — don't reveal if email exists
        return Ok(new { message = "ប្រសិនបើ Email នេះមាន យើងនឹងផ្ញើ Link Reset។" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest r)
    {
        if (r.NewPassword.Length < 6)
            return BadRequest(new { message = "Password យ៉ាងហោច 6 តួអក្សរ!" });

        if (!await _s.ResetPasswordAsync(r.Token, r.NewPassword))
            return BadRequest(new { message = "Token មិនត្រឹមត្រូវ ឬផុតកំណត់!" });

        return Ok(new { message = "Reset Password ជោគជ័យ!" });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MasterController : ControllerBase
{
    private readonly CashewTypeService _s;
    public MasterController(CashewTypeService s) => _s = s;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _s.GetAllAsync(UserId));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCashewTypeRequest r)
        => Ok(await _s.CreateAsync(r, UserId));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCashewTypeRequest r)
    {
        var res = await _s.UpdateAsync(id, r, UserId);
        return res is null ? NotFound() : Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _s.DeleteAsync(id, UserId)) return NotFound();
        return Ok();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly PurchaseService _s;
    public PurchaseController(PurchaseService s) => _s = s;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        [FromQuery] bool all = false)
        => Ok(await _s.GetPagedAsync(UserId, page, pageSize, from, to, all));

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseRequest r)
        => Ok(await _s.CreateAsync(r, UserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _s.DeleteAsync(id, UserId)) return NotFound();
        return Ok();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly SaleService _s;
    public SaleController(SaleService s) => _s = s;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        [FromQuery] bool all = false)
        => Ok(await _s.GetPagedAsync(UserId, page, pageSize, from, to, all));

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleRequest r)
        => Ok(await _s.CreateAsync(r, UserId));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _s.DeleteAsync(id, UserId)) return NotFound();
        return Ok();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _s;
    public DashboardController(DashboardService s) => _s = s;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? from = null,
        [FromQuery] string? to = null,
        [FromQuery] bool all = false)
    {
        if (all) return Ok(await _s.GetAsync(UserId, null, null));

        var nowCam = DateTime.UtcNow.Add(TimeSpan.FromHours(7));

        if (!TryParseDate(from, nowCam.Date, out var f))
            return BadRequest("Invalid 'from' date. Use yyyy-MM-dd.");

        if (!TryParseDate(to, nowCam.Date, out var t))
            return BadRequest("Invalid 'to' date. Use yyyy-MM-dd.");

        if (f > t)
            return BadRequest("'from' must not be after 'to'.");

        return Ok(await _s.GetAsync(UserId, f, t));
    }

    private static bool TryParseDate(string? input, DateTime fallback, out DateTime result)
    {
        if (string.IsNullOrWhiteSpace(input)) { result = fallback; return true; }
        return DateTime.TryParseExact(input, "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out result);
    }
}