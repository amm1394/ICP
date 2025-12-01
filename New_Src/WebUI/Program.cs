using MudBlazor.Services; // ضروری برای MudBlazor
using WebUI.Services;     // ضروری برای AuthService

var builder = WebApplication.CreateBuilder(args);

// 1. سرویس‌های Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 2. سرویس‌های MudBlazor (رفع خطای CS1061)
builder.Services.AddMudServices();

// 3. تنظیم HttpClient برای اتصال به API داخلی (Thin Client Architecture)
// این کلاینت از داخل سرور لینوکس به API شما درخواست می‌فرستد
builder.Services.AddHttpClient("Api", client =>
{
    // فرض بر این است که API روی پورت 5268 در حال اجراست
    // اگر هر دو روی یک سرور هستند، localhost بهترین گزینه است
    client.BaseAddress = new Uri("http://localhost:5268/api/");
});

// 4. تزریق سرویس احراز هویت (رفع خطای CS0246 در Login.razor)
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// ریدایرکت HTTPS برای امنیت بیشتر در محیط پروداکشن
// اگر پشت Nginx هستید و SSL را آنجا مدیریت می‌کنید، ممکن است این خط نیاز به تنظیمات ForwardedHeaders داشته باشد
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();