using Microsoft.AspNetCore.Authentication.JwtBearer;
using QuickStock.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickStock.Applications.Accounts.Handler;
using QuickStock.Infrastructure.Config;
using QuickStock.Infrastructure.Data;
using QuickStock.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Controllers
// -------------------------
builder.Services.AddControllers();
builder.Services.AddSignalR();

// -------------------------
// Database
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// -------------------------
// Email service
// -------------------------
builder.Services.Configure<EmailSettings>(
builder.Configuration.GetSection("EmailSettings")
);
builder.Services.AddScoped<EmailService>();

// -------------------------
// Auth service
// -------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();

// -------------------------
// Profile Service
// -------------------------
builder.Services.AddScoped<QuickStock.Applications.Profile.Handler.UpdateProfileHandler>();
builder.Services.AddScoped<QuickStock.Applications.Profile.Handler.GetProfileHandler>();

// -------------------------
// CORS
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
        .WithOrigins("https://localhost:7058")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Required for SignalR
    });
});

// -------------------------
// MediatR
// -------------------------
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<LoginCommandHandler>();
});

// -------------------------
// JWT Authentication
// -------------------------
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
    )
    };
});

// -------------------------
// Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------------
// Build App
// -------------------------

var app = builder.Build();

// -------------------------
// Middleware Pipeline
// -------------------------
app.ConfigureCustomExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve files from wwwroot (for profile images)
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<QuickStock.Controllers.ChatHub>("/chatHub");

// -------------------------
// Database Initialization
// -------------------------
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

app.Run();
