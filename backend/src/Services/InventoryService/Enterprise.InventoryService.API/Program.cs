using Enterprise.InventoryService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Enterprise.InventoryService.Application.Interfaces;
using Enterprise.InventoryService.Application.Services;
using Enterprise.InventoryService.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<InventoryDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("InventoryDb")));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddScoped<IInventoryService, InventoryService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();