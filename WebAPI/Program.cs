using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Text;
using HospitalManagementSystem.Application.Common.Behaviors;
using HospitalManagementSystem.Application.Common.Extensions;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Infrastructure.Identity;
using HospitalManagementSystem.WebAPI.Middleware;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Domain.Interfaces;
using HospitalManagementSystem.WebAPI.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Hospital Management System API", 
        Version = "v1",
        Description = "API for Hospital Management System"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireDoctorRole", policy => policy.RequireRole("Doctor", "Administrator"));
    options.AddPolicy("RequireNurseRole", policy => policy.RequireRole("Nurse", "Administrator"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff", "Administrator", "Doctor", "Nurse"));
});

// Application Services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddAutoMapper(typeof(Program));

// Infrastructure Services
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<IPatientRepository, PatientRepository>();
//builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
//builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
//builder.Services.AddScoped<IMedicationRepository, MedicationRepository>();
//builder.Services.AddScoped<IBillingRepository, BillingRepository>();
//builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

//// Application Services
//builder.Services.AddScoped<IPatientService, PatientService>();
//builder.Services.AddScoped<IDoctorService, DoctorService>();
//builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();
//builder.Services.AddScoped<IMedicationService, MedicationService>();
//builder.Services.AddScoped<IBillingService, BillingService>();
//builder.Services.AddScoped<IAppointmentService, AppointmentService>();
//builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// SignalR
builder.Services.AddSignalR();

builder.Services.AddApplication();
// MediatR pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
// AutoMapper
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
//app.MapHub<HospitalHub>("/hospitalHub");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    
    //var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    //await initializer.InitializeAsync();
}

app.Run();


