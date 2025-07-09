using DeliveryApp.Api;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Primitives;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
var connectionString = builder.Configuration["CONNECTION_STRING"];

// БД
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseNpgsql(connectionString));

// Domain Service
builder.Services.AddScoped<IDispatchService, DispatchService>();

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repo
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddScoped<IRequestHandler<CreateOrderCommand, bool>, CreateOrderHandler>();
builder.Services.AddScoped<IRequestHandler<MoveCouriersCommand, bool>, MoveCouriersHandler>();
builder.Services.AddScoped<IRequestHandler<AssignOrderCommand, bool>, AssignOrderHandler>();

// Query
builder.Services.AddScoped<IRequestHandler<GetCreatedAndAssignedOrdersQuery, GetCreatedAndAssignedOrdersModel>, GetCreatedAndAssignedOrdersHandler>();
builder.Services.AddScoped<IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersModel>, GetBusyCouriersHandler>();

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

// Apply Migrations
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     db.Database.Migrate();
// }

app.Run();