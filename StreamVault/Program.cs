using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.IO;
using SV.Common.Utilities;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Collections.Generic;
using SV.Common.DTOs.Role;
using SV.Data.Connections;
using SV.Service.Abstractions;
using SV.Service.Implementations;
using SV.Store.Abstractions;
using SV.Store.Implementations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<SV.Common.Validators.SubscriptionCreateRequestValidator>();

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StreamVault API", Version = "v1" });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);


    var bearerRef = new OpenApiSecuritySchemeReference("Bearer", null, null);

    // Add the security requirement using the overload that accepts a factory for the OpenApiDocument
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { bearerRef, new List<string>() }
    });
    // Include XML comments (controller and model summaries) in Swagger UI
    var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile ?? string.Empty);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Database
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<JwtHelper>();

// Store & Service registrations
builder.Services.AddScoped<IErrorStore, ErrorStore>();
builder.Services.AddScoped<IAuthStore, AuthStore>();
builder.Services.AddScoped<IAuthService, AuthService>();

// TODO: register other stores and services (UserStore, MovieStore, PlanStore, etc.)
builder.Services.AddScoped<IPlanStore, PlanStore>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IGenreStore, GenreStore>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IMovieStore, MovieStore>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IUserStore, UserStore>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubscriptionStore, SubscriptionStore>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IWatchHistoryStore, WatchHistoryStore>();
builder.Services.AddScoped<IWatchHistoryService, WatchHistoryService>();
builder.Services.AddScoped<IPaymentStore, PaymentStore>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IWatchlistStore, WatchlistStore>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IReviewStore, ReviewStore>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IRoleStore, RoleStore>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Dashboard
builder.Services.AddScoped<IDashboardStore, DashboardStore>();
builder.Services.AddScoped<IDashboardService, DashboardService>();


var app = builder.Build();

// Exception middleware must be resolved from app.Services
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("GlobalExceptionLogger");

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
// Middleware Pipeline

// Use global exception middleware (concrete implementation lives in the Web project)
app.UseMiddleware<ProjectFileStructure.Controllers.Middleware.ExceptionMiddlewareImpl>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();