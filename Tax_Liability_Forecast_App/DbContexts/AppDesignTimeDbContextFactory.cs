using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace Tax_Liability_Forecast_App.DbContexts
{
    public class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "taxforecast.db")}").Options;
            return new AppDbContext(options);
        }
    }
}