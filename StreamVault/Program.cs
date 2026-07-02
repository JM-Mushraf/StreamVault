using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models; // removed to avoid missing-type errors
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
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

// Enable request buffering so the stream can be read multiple times
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});


builder.Services.Configure<FormOptions>(options =>
{
    
    options.MultipartBodyLengthLimit = 2147483648; // 2 GB


    options.MultipartHeadersLengthLimit = 524288000; // 500 MB

    options.ValueLengthLimit = 1024 * 1024 * 1024; // 1 GB per form value

    options.MemoryBufferThreshold = 67108864; // 64 MB

    options.BufferBody = true;
});


builder.WebHost.ConfigureKestrel(serverOptions =>
{

    serverOptions.Limits.MaxRequestBodySize = 2147483648; // 2 GB
});


builder.Services.AddValidatorsFromAssemblyContaining<SV.Common.Validators.SubscriptionCreateRequestValidator>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    try
    {

        var swaggerGenAssembly = typeof(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions).Assembly;
        var openApiRef = System.Linq.Enumerable.FirstOrDefault(
            swaggerGenAssembly.GetReferencedAssemblies(), 
            a => a.Name == "Microsoft.OpenApi"
        );
        var openApiAssembly = System.Reflection.Assembly.Load(openApiRef);

        Func<string, Type> getOpenApiType = name => 
            openApiAssembly.GetType($"Microsoft.OpenApi.Models.{name}") ?? 
            openApiAssembly.GetType($"Microsoft.OpenApi.{name}");

        var schemeType = getOpenApiType("OpenApiSecurityScheme");
        var requirementType = getOpenApiType("OpenApiSecurityRequirement");
        var referenceType = getOpenApiType("OpenApiReference");
        var schemeEnum = getOpenApiType("SecuritySchemeType");
        var locationEnum = getOpenApiType("ParameterLocation");
        var refTypeEnum = getOpenApiType("ReferenceType");

        var secSchemeRefType = openApiAssembly.GetType("Microsoft.OpenApi.OpenApiSecuritySchemeReference");

        if (schemeType == null || requirementType == null || schemeEnum == null || locationEnum == null || refTypeEnum == null)
        {
            throw new Exception("One or more core OpenAPI types could not be found in the loaded Microsoft.OpenApi assembly.");
        }

        // 1. Create OpenApiSecurityScheme
        var scheme = Activator.CreateInstance(schemeType);
        schemeType.GetProperty("Name").SetValue(scheme, "Authorization");
        schemeType.GetProperty("Type").SetValue(scheme, Enum.Parse(schemeEnum, "ApiKey"));
        schemeType.GetProperty("Scheme").SetValue(scheme, "Bearer");
        schemeType.GetProperty("BearerFormat").SetValue(scheme, "JWT");
        schemeType.GetProperty("In").SetValue(scheme, Enum.Parse(locationEnum, "Header"));
        schemeType.GetProperty("Description").SetValue(scheme, "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"");

        var extType = swaggerGenAssembly.GetTypes()
            .FirstOrDefault(t => t.GetMethods().Any(m => m.Name == "AddSecurityDefinition"));
        Console.WriteLine($"[SWAGGER REFLECTION DEBUG] Found extension class: {extType?.FullName}");

        if (extType != null)
        {
            foreach (var m in extType.GetMethods().Where(m => m.Name == "AddSecurityDefinition" || m.Name == "AddSecurityRequirement"))
            {
                Console.WriteLine($"[SWAGGER REFLECTION DEBUG] Extension method: {m.Name} parameters: {string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.AssemblyQualifiedName} {p.Name}"))}");
            }
        }

        // Call options.AddSecurityDefinition("Bearer", scheme)
        var addSecDefMethod = extType?.GetMethod("AddSecurityDefinition", new Type[] { typeof(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions), typeof(string), schemeType });
        if (addSecDefMethod == null && extType != null)
        {
            addSecDefMethod = extType.GetMethods()
                .FirstOrDefault(m => m.Name == "AddSecurityDefinition" && m.GetParameters().Length == 3);
        }
        addSecDefMethod.Invoke(null, new object[] { options, "Bearer", scheme });

        // Resolve AddSecurityRequirement extension method
        var addSecReqMethod = extType?.GetMethod("AddSecurityRequirement", new Type[] { typeof(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions), requirementType });
        if (addSecReqMethod == null && extType != null)
        {
            addSecReqMethod = extType.GetMethods()
                .FirstOrDefault(m => m.Name == "AddSecurityRequirement" && m.GetParameters().Length == 2);
        }

        if (secSchemeRefType != null)
        {
            var docType = openApiAssembly.GetType("Microsoft.OpenApi.OpenApiDocument") 
                       ?? openApiAssembly.GetType("Microsoft.OpenApi.Models.OpenApiDocument");
            var funcType = typeof(System.Func<,>).MakeGenericType(docType, requirementType);

            // Construct expression tree to compile:
            // (OpenApiDocument doc) => {
            //     var req = new OpenApiSecurityRequirement();
            //     var reqSchemeRef = new OpenApiSecuritySchemeReference("Bearer", doc, null);
            //     req.Add(reqSchemeRef, new List<string>());
            //     return req;
            // }
            var docParam = System.Linq.Expressions.Expression.Parameter(docType, "doc");
            var reqVar = System.Linq.Expressions.Expression.Variable(requirementType, "req");
            var reqSchemeRefVar = System.Linq.Expressions.Expression.Variable(secSchemeRefType, "reqSchemeRef");

            var secSchemeRefCtor = secSchemeRefType.GetConstructor(new Type[] { typeof(string), docType, typeof(string) });
            var newReq = System.Linq.Expressions.Expression.New(requirementType);
            var newRef = System.Linq.Expressions.Expression.New(secSchemeRefCtor, 
                System.Linq.Expressions.Expression.Constant("Bearer"), 
                docParam, 
                System.Linq.Expressions.Expression.Constant(null, typeof(string)));

            var addMethod = requirementType.GetMethod("Add", new Type[] { secSchemeRefType, typeof(System.Collections.Generic.List<string>) })
                         ?? requirementType.GetMethod("Add", new Type[] { secSchemeRefType, typeof(System.Collections.Generic.IList<string>) });

            var listType = addMethod.GetParameters()[1].ParameterType;
            if (listType.IsInterface)
            {
                listType = typeof(System.Collections.Generic.List<string>);
            }
            var newList = System.Linq.Expressions.Expression.New(listType);

            var assignReq = System.Linq.Expressions.Expression.Assign(reqVar, newReq);
            var assignRef = System.Linq.Expressions.Expression.Assign(reqSchemeRefVar, newRef);
            var callAdd = System.Linq.Expressions.Expression.Call(reqVar, addMethod, reqSchemeRefVar, newList);

            var blockExpr = System.Linq.Expressions.Expression.Block(
                new[] { reqVar, reqSchemeRefVar },
                assignReq,
                assignRef,
                callAdd,
                reqVar
            );

            var lambdaExpr = System.Linq.Expressions.Expression.Lambda(funcType, blockExpr, docParam);
            var compiledDelegate = lambdaExpr.Compile();

            addSecReqMethod.Invoke(null, new object[] { options, compiledDelegate });
        }
        else
        {
            var reference = Activator.CreateInstance(referenceType);
            referenceType.GetProperty("Type").SetValue(reference, Enum.Parse(refTypeEnum, "SecurityScheme"));
            referenceType.GetProperty("Id").SetValue(reference, "Bearer");

            var reqScheme = Activator.CreateInstance(schemeType);
            schemeType.GetProperty("Reference").SetValue(reqScheme, reference);

            var requirement = Activator.CreateInstance(requirementType);
            var addMethod = requirementType.GetMethod("Add", new Type[] { schemeType, typeof(System.Collections.Generic.IList<string>) })
                         ?? requirementType.GetMethod("Add", new Type[] { schemeType, typeof(System.Collections.Generic.List<string>) });
            addMethod.Invoke(requirement, new object[] { reqScheme, new System.Collections.Generic.List<string>() });

            addSecReqMethod.Invoke(null, new object[] { options, requirement });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SWAGGER JWT CONFIG] Failed to configure: {ex.ToString()}");
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<ProjectFileStructure.Helpers.QueuedHostedService>();

// Cloudinary service registration 
builder.Services.AddSingleton<SV.Common.Abstractions.ICloudinaryService, SV.Service.Implementations.CloudinaryService>();
builder.Services.AddScoped<SV.Common.Abstractions.IEmailService, SV.Service.Implementations.EmailService>();

// Store & Service registrations
builder.Services.AddScoped<IErrorStore, ErrorStore>();
builder.Services.AddScoped<IAuthStore, AuthStore>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddScoped<IPlanStore, PlanStore>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IGenreStore, GenreStore>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IMovieStore, MovieStore>();
builder.Services.AddScoped<IMovieService, MovieService>();
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
builder.Services.AddScoped<IProfileStore, ProfileStore>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<INotificationStore, NotificationStore>();
builder.Services.AddScoped<INotificationService, ProjectFileStructure.Services.NotificationService>();
builder.Services.AddSignalR();

// Register user service and store
builder.Services.AddScoped<SV.Store.Abstractions.IUserStore, SV.Store.Implementations.UserStore>();
builder.Services.AddScoped<SV.Service.Abstractions.IUserService, SV.Service.Implementations.UserService>();

// Dashboard
builder.Services.AddScoped<IDashboardStore, DashboardStore>();
builder.Services.AddScoped<IDashboardService, DashboardService>();


var app = builder.Build();

// Exception middleware must be resolved from app.Services
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("GlobalExceptionLogger");   

app.Use(async (context, next) =>
{

    if (context.Request.Body.CanSeek)
    {
        context.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
    }

    context.Request.EnableBuffering();

    await next();

    if (context.Request.Body.CanSeek)
    {
        context.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
    }
});

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StreamVault API v1"));
}

app.UseCors("AllowReact");

app.UseMiddleware<ProjectFileStructure.Middleware.ExceptionMiddlewareImpl>();

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ProjectFileStructure.Hubs.NotificationHub>("/hubs/notifications");

app.Run();