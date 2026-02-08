using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuickStock.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // 👇 Put your actual connection string here
            optionsBuilder.UseMySql(
                "server=localhost;database=Quickstock;user=root;password=Unknownuser;",
                new MySqlServerVersion(new Version(8, 0, 36)) // adjust to your MySQL version
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
