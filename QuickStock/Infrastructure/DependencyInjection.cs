using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;
using QuickStock.Infrastructure.Services;
using QuickStock.Infrastructure.Config;

namespace QuickStock.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Database Configuration Setup
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySQL(connectionString)
            );

            // 2. Email Service Infrastructure Registration
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<EmailService>();

            // 3. Application Security & Core Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
