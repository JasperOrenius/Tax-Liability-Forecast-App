using Microsoft.EntityFrameworkCore;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Client)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
