using System.Linq;
using HospitalMS.API.Infrastructure;
using HospitalMS.Business;
using HospitalMS.Common.Extensions;
using HospitalMS.Data.Extensions;
using HospitalMS.ServiceDefaults.Extensions;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddHospitalDataAccess(builder.Configuration);
builder.Services.AddHospitalSecurity(builder.Configuration);
builder.Services.AddHospitalBusinessServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HospitalMS API",
        Version = "v1",
        Description = "Hospital Management System API."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT token returned by POST /api/auth/login."
    });

    options.OperationFilter<BearerSecurityOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HospitalMS API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler(errorApp =>
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = "An unexpected error occurred.",
                status = 500
            });
        }));
    app.UseHsts();
}

if (HasHttpsEndpoint(builder.Configuration))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseHospitalSecurity();
app.MapControllers();
app.MapDefaultHealthChecks();

await app.Services.InitializeHospitalDatabaseAsync();

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
