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

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",                    // CRA portu (yedek)
                "http://localhost:5173",                    // Vite (geliştirme)
                "http://localhost:8080",                    // Capacitor dev server
                "https://localhost",                        // Capacitor Android WebView (androidScheme: https)
                "capacitor://localhost",                    // Capacitor native (fallback)
                "ionic://localhost",                        // Ionic fallback
                "https://tbalans-app-2026.web.app",        // Firebase Hosting (production)
                "https://tbalans-app-2026.firebaseapp.com" // Firebase alternatif domain
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

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

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Lütfen Bearer [boşluk] {token} formatında JWT girin. Örnek: \"Bearer eyJhbGci...\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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

// CORS en önce gelmelil - UseHttpsRedirection'dan önce!
app.UseCors("ReactApp");

// Sadece production ortamında HTTPS yönlendirmesi yap (dev'de CORS sorununa neden olur)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Yetkilendirme middleware'leri (Sıralama önemlidir!)
app.UseAuthentication();
app.UseAuthorization();

// Statik dosyaları ( wwwroot/uploads ) serve et
app.UseStaticFiles();

app.MapControllers();

app.Run();
