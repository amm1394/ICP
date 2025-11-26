using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Isatis.Client;
using Telerik.Blazor.Services; // Add this
using Isatis.Client.Services; // Add this

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✅ اتصال به Gateway (پورت 5001)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://192.168.0.103:5001")
});

// ثبت سرویس‌های Telerik
builder.Services.AddTelerikBlazor();

// ثبت سرویس‌های برنامه ما
builder.Services.AddScoped<IsatisApiService>();
builder.Services.AddSingleton<UserState>();

await builder.Build().RunAsync();