using IPLMerchandise.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using IPLMerchandise.Infrastructure.Repositories;
using IPLMerchandise.Application.Interfaces;
using IPLMerchandise.Api.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IPL Merchandise Store API",
        Version = "v1",
        Description = "API for IPL Franchise Merchandise Store"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });

    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";

        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "IPL Merchandise Store API v1");
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("ReactDevelopment");

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    Application = "IPL Merchandise Store",
    Status = "Healthy"
}));

app.Run();