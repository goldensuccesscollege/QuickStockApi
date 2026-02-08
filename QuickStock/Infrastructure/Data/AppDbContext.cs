using Microsoft.EntityFrameworkCore;
using QuickStock.Domain;

namespace QuickStock.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        
    }
}
