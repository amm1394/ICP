var builder = WebApplication.CreateBuilder(args);

// ثبت سرویس YARP و خواندن تنظیمات از فایل کانفیگ
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// تنظیم Kestrel برای اجازه آپلود فایل‌های بزرگ (مثلاً تا 500 مگابایت)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000; // 500 MB
});

var app = builder.Build();

// فعال‌سازی میدل‌ویر YARP
app.MapReverseProxy();

app.Run();