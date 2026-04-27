using System.Text;
using CashewNuts.Application.Services;
using CashewNuts.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CashewNuts.API")));

builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CashewTypeService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cashew API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Name = "Authorization", Type = SecuritySchemeType.ApiKey });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.Run();
