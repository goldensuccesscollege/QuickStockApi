using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickStock.Applications.Accounts.Handler;
using QuickStock.CQRS;
using QuickStock.Infrastructure; // Accesses your new static extension method
using QuickStock.Infrastructure.Data;
using QuickStock.Middlewares;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// Controllers & SignalR
// ======================================================

builder.Services.AddControllers();
builder.Services.AddSignalR();

// ======================================================
// Infrastructure Services Registration (The Clean Way)
// ======================================================

// This single line handles your ConnectionString verification, MySQL registration,
// EmailSettings binding, and Scoped application services (Auth, Image, Notification)!
builder.Services.AddInfrastructure(builder.Configuration);

// ======================================================
// Profile Application Services
// ======================================================

builder.Services.AddScoped<QuickStock.Applications.Profile.Handler.UpdateProfileHandler>();
builder.Services.AddScoped<QuickStock.Applications.Profile.Handler.GetProfileHandler>();

// ======================================================
// CORS Configuration
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:7058")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Crucial for SignalR WebSockets
    });
});

// ======================================================
// CQRS & Http Accessor
// ======================================================

builder.Services.AddCQRS(typeof(LoginCommandHandler).Assembly);
builder.Services.AddHttpContextAccessor();

// ======================================================
// JWT Authentication Pipeline
// ======================================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key 'Jwt:Key' is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer 'Jwt:Issuer' is missing from configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience 'Jwt:Audience' is missing from configuration.");

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

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        // Extracts token from query string for incoming SignalR WebSocket connections
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },

        OnTokenValidated = async context =>
        {
            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();

            var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var account = await dbContext.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == userId);

                if (account == null || (account.Status != null && !account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)))
                {
                    context.Fail("Account is disabled or no longer exists.");
                }
            }
        }
    };
});

// ======================================================
// Swagger UI Configuration
// ======================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "QuickStock API",
        Version = "v1",
        Description = "Premium API for QuickStock Inventory Management System"
    });

    var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// ======================================================
// Build App Engine
// ======================================================

var app = builder.Build();

// ======================================================
// Automatic Database Migration Startup
// ======================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (app.Environment.IsDevelopment())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// ======================================================
// Middleware Pipeline Execution
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickStock API v1");
    });
}

app.ConfigureCustomExceptionMiddleware();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CORS evaluated after routing, before Auth
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// ======================================================
// API Endpoint Routing Maps
// ======================================================

app.MapControllers();
app.MapHub<QuickStock.Controllers.NotificationHub>("/notificationHub");

// ======================================================
// Run Application
// ======================================================

app.Run();
