using Application;        // برای AddApplicationServices
using Infrastructure;     // برای AddInfrastructureServices
using Scalar.AspNetCore;  // برای Scalar UI

var builder = WebApplication.CreateBuilder(args);

// ===============================
//  ثبت سرویس‌ها
// ===============================
builder.Services.AddControllers();

// OpenAPI (مخصوص .NET 9 / 10)
builder.Services.AddOpenApi();

// لایه Application
builder.Services.AddApplicationServices();

// لایه Infrastructure (دیتابیس، فایل، ریپازیتوری و ...)
builder.Services.AddInfrastructureServices(builder.Configuration);

// ===============================

var app = builder.Build();

// ===============================
//  Pipeline
// ===============================

// ✅ هم در Development هم در Production فعال است
app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.Title = "My API";
    // تنظیمات اضافه اگر خواستی
});

// اگر بعداً HTTPS خواستی، این خط را فعال کن
// app.UseHttpsRedirection();

app.UseAuthorization();

// کنترلرها
app.MapControllers();

// ✅ یک روت ساده برای تست
app.MapGet("/", () => Results.Ok("API is running on port 5000"));

app.Run();
