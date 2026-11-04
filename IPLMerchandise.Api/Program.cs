
//using IPLMerchandise.Api.ExceptionHandlers;
using IPLMerchandise.Api.Exceptions;
using IPLMerchandise.Infrastructure;
using IPLMerchandise.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Controllers and JSON serialization
// --------------------------------------------------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------------------------------------
// CORS
// --------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --------------------------------------------------
// Exception handling
// --------------------------------------------------

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// --------------------------------------------------
// Cosmos DB and Infrastructure
// --------------------------------------------------

builder.Services.AddInfrastructure(builder.Configuration);

// --------------------------------------------------
// JWT configuration and authentication
// --------------------------------------------------

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) ||
    Encoding.UTF8.GetByteCount(jwtOptions.SecretKey) < 32)
{
    throw new InvalidOperationException(
        "JWT SecretKey must contain at least 32 bytes.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
    string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
    jwtOptions.ExpiryMinutes <= 0)
{
    throw new InvalidOperationException(
        "JWT Issuer, Audience and ExpiryMinutes must be configured correctly.");
}

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// --------------------------------------------------
// Rate limiting
// --------------------------------------------------

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // General API: 100 requests per minute per client IP.
    options.AddPolicy("ApiPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Authentication: 5 requests per minute per client IP.
    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Checkout: 5 requests per minute per client IP.
    options.AddPolicy("CheckoutPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                status = StatusCodes.Status429TooManyRequests,
                title = "Too many requests",
                detail = "Request limit exceeded. Please try again later."
            },
            cancellationToken);
    };
});

// --------------------------------------------------
// Application pipeline
// --------------------------------------------------

var app = builder.Build();

// Global exception handling
app.UseExceptionHandler();

// Swagger
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

app.UseHttpsRedirection();

app.UseRouting();

// CORS
app.UseCors("ReactClient");

// Rate limiter must run before authentication/authorization
// when the policy is based on the client IP.
app.UseRateLimiter();

// JWT authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    application = "IPL Merchandise Store API",
    timestampUtc = DateTime.UtcNow
}));

app.Run();
