using Microsoft.EntityFrameworkCore;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Client> Clients { get; set; }
    }
}
