using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using TBalans.Application.Services;
using TBalans.Domain.Entities;
using TBalans.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI ayarları
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TBalans API",
        Version = "v1",
        Description = "Web API for TBalans Assignment and Calendar Management"
    });
});

// DbContext kaydı (Sqlite)
builder.Services.AddDbContext<TBalansDbContext>();

// Identity Ayarları
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<TBalansDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication Ayarları
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
        ValidIssuer = "TBalansApp",
        ValidAudience = "TBalansUsers",
        // Gerçek projede bu anahtar appsettings.json'dan alınmalıdır!
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TBalans_Super_Secret_Key_For_Jwt_Auth_2026!"))
    };
});

// Application Services kaydı (Dependency Injection)
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();
app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// Yetkilendirme middleware'leri (Sıralama önemlidir!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
