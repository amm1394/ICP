using Application;
using Infrastructure;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ===============================
//  1. تنظیمات سرویس‌ها (DI)
// ===============================

// تنظیمات JSON (جلوگیری از لوپ و فرمت‌دهی)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// فعال‌سازی فشرده‌سازی Gzip/Brotli (بهینه‌سازی سرعت)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// OpenAPI (مخصوص .NET 9)
builder.Services.AddOpenApi();

// لایه‌های پروژه (Application & Infrastructure)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// --- تنظیمات امنیت (JWT Authentication) ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKeyString = jwtSettings["Secret"];

// اطمینان از وجود کلید امنیتی
if (string.IsNullOrEmpty(secretKeyString))
    throw new Exception("JWT Secret key is not configured in appsettings.json");

var secretKey = Encoding.UTF8.GetBytes(secretKeyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});
// -------------------------------------------

var app = builder.Build();

// ===============================
//  2. پایپ‌لاین درخواست‌ها (Middleware)
// ===============================

// فشرده‌سازی باید اولین باشد
app.UseResponseCompression();

// مستندات API
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Isatis ICP API";
});

// امنیت (ترتیب مهم است)
app.UseAuthentication(); // 1. تشخیص هویت
app.UseAuthorization();  // 2. بررسی دسترسی

// کنترلرها
app.MapControllers();

// تست ساده
app.MapGet("/", () => Results.Ok("Isatis API is running securely (Auth + Gzip) 🚀"));

app.Run();