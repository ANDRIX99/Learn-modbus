using Learn_modbus.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Polling service to read from a specific slave
//builder.Services.AddHostedService(sp => 
//new ReadPolling(
//        sp.GetRequiredService<ILogger<ReadPolling>>(),
//        slaveId: 1,
//        startAddress: 0,
//        count: 3
//    )
//);
builder.Services.AddHostedService<Polling>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
