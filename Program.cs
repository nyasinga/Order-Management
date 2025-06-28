using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Data;
using OrderManagementSystem.Data.Repositories;
using OrderManagementSystem.DTOs;
using OrderManagementSystem.Interfaces;
using OrderManagementSystem.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<OrderManagementContext>(options =>
    options.UseInMemoryDatabase("OrderManagementDb"));

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Register services
builder.Services.AddScoped<IOrderService, OrderService>();

// Register discount rules and service
builder.Services.AddSingleton<ICustomerDiscountRule, PremiumCustomerDiscountRule>();
builder.Services.AddSingleton<ICustomerDiscountRule, BulkOrderDiscountRule>();
builder.Services.AddSingleton<ICustomerDiscountRule, HighValueOrderDiscountRule>();
builder.Services.AddScoped<IDiscountService, DiscountService>();

// Add AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Order Management API", Version = "v1" });
    
    // Enable XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
        c.RoutePrefix = string.Empty; // Serve the Swagger UI at the root
    });
    
    // Seed the in-memory database with test data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<OrderManagementContext>();
        DbInitializer.Initialize(context);
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Add global exception handling
app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    return Results.Problem(
        title: "An unexpected error occurred",
        statusCode: StatusCodes.Status500InternalServerError,
        detail: exception?.Message);
});

app.MapControllers();

app.Run();
