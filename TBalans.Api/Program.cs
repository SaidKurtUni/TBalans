using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TBalans.Application.Services;
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

// Application Services kaydı (Dependency Injection)
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
