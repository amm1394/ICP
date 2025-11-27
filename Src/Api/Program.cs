using Application;
using Infrastructure;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression; // 👈 برای Gzip
using System.IO.Compression; // 👈 برای تنظیمات فشرده‌سازی
using System.Text.Json.Serialization; // 👈 برای تنظیمات JSON

var builder = WebApplication.CreateBuilder(args);

// ===============================
//  1. ثبت سرویس‌ها (DI)
// ===============================

// ✅ تنظیمات JSON برای جلوگیری از خطای Circular Reference و جایگزینی نیاز به Newtonsoft
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // اگر شیء A به B و B به A اشاره کند، ارور نمی‌دهد و نادیده می‌گیرد
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // فرمت خروجی تمیز باشد (اختیاری - در پروداکشن می‌توان حذف کرد)
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ✅ فعال‌سازی سرویس فشرده‌سازی (Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // فشرده‌سازی در HTTPS هم فعال باشد
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>(); // Brotli معمولاً فشرده‌تر از Gzip است
});

// تنظیم سطح فشرده‌سازی (Fastest سرعت بالا، Optimal فشرده‌سازی بیشتر)
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddOpenApi();

// لایه‌های پروژه
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// تنظیمات JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// چک کردن نال بودن کلید برای جلوگیری از خطا در اجرا
var secretKeyString = jwtSettings["Secret"];
if (string.IsNullOrEmpty(secretKeyString))
{
    throw new Exception("JWT Secret key is not configured in appsettings.json");
}
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

// ===============================

var app = builder.Build();

// ===============================
//  2. پایپ‌لاین (Middleware)
// ===============================

// ✅ میدل‌ویر فشرده‌سازی باید در ابتدای پایپ‌لاین باشد
app.UseResponseCompression();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Isatis ICP API";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok("Isatis API is running securely with Gzip compression 🚀"));

app.Run();