using System;
using System.Linq;
using HospitalMS.Business;
using HospitalMS.Common.Extensions;
using HospitalMS.Data.Extensions;
using HospitalMS.ServiceDefaults.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();
builder.Services.AddHospitalDataAccess(builder.Configuration);
builder.Services.AddHospitalSecurity(builder.Configuration);
builder.Services.AddHospitalBusinessServices();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient("HospitalAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5002");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (HasHttpsEndpoint(builder.Configuration))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseHospitalSecurity();
app.UseSession();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapDefaultHealthChecks();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeHospitalDatabaseAsync();
}

app.Run();

static bool HasHttpsEndpoint(IConfiguration configuration)
{
    if (!string.IsNullOrWhiteSpace(configuration["ASPNETCORE_HTTPS_PORT"]) ||
        !string.IsNullOrWhiteSpace(configuration["HTTPS_PORT"]))
    {
        return true;
    }

    var urls = configuration["ASPNETCORE_URLS"] ?? configuration["urls"];

    return !string.IsNullOrWhiteSpace(urls) &&
           urls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
}
