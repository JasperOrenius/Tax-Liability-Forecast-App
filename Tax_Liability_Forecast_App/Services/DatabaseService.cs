using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Tax_Liability_Forecast_App.DbContexts;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly AppDbContextFactory dbContextFactory;

        public DatabaseService(AppDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }
        
        public async Task<IEnumerable<Transaction>> GetAllTransactions()
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                return await context.Transactions.ToListAsync();
            }
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByClientId(Guid clientId)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                return await context.Transactions.Where(t => t.ClientId == clientId).Include(t => t.Client).Include(t => t.DeductionType).ToListAsync();
            }
        }

        public async Task CreateTransaction(Transaction transaction)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.Transactions.Add(transaction);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateTransaction(Transaction transaction)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                var updatedTransaction = await context.Transactions.FindAsync(transaction.Id);
                if(updatedTransaction != null)
                {
                    updatedTransaction.Date = transaction.Date;
                    updatedTransaction.Description = transaction.Description;
                    updatedTransaction.Amount = transaction.Amount;
                    updatedTransaction.IncomeType = transaction.IncomeType;
                    updatedTransaction.Type = transaction.Type;
                    updatedTransaction.DeductionTypeId = transaction.DeductionTypeId != null ? transaction.DeductionTypeId : null;

                    await context.SaveChangesAsync();
                }
            }
        }
        
        public async Task DeleteTransaction(Transaction transaction)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.Transactions.Remove(transaction);
                await context.SaveChangesAsync();
            }
        }

        //Client
        public async Task<IEnumerable<Client>> FetchClientTable()
        {
            using(var context = dbContextFactory.CreateDbContext())
            {
                List<Client> result = await context.Clients.ToListAsync();
                return result;
            }
        }

        public async Task AddClient(Client recordToAdd)
        {
            using(var context = dbContextFactory.CreateDbContext())
            {
                context.Clients.Add(recordToAdd);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveClient(Client clientToRemove)
        {
            using(var context = dbContextFactory.CreateDbContext())
            {
                context.Clients.Remove(clientToRemove);
                await context.SaveChangesAsync();
            }
        }


        //Tax Brackets
        public async Task<IEnumerable<TaxBracket>> FetchAllTaxBrackets()
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                return await context.TaxBrackets.ToListAsync();
            }
        }

        public async Task CreateTaxBracket(TaxBracket taxBracket)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.TaxBrackets.Add(taxBracket);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateTaxBracket(TaxBracket taxBracket)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                var updatedtaxBracket = await context.TaxBrackets.FindAsync(taxBracket.Id);
                if(updatedtaxBracket != null)
                {
                    updatedtaxBracket.MinIncome = taxBracket.MinIncome;
                    updatedtaxBracket.MaxIncome = taxBracket.MaxIncome;
                    updatedtaxBracket.TaxRate = taxBracket.TaxRate;

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveTaxBracket(TaxBracket taxBracket)
        {
            using(AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.TaxBrackets.Remove(taxBracket);
                await context.SaveChangesAsync();
            }
        }


        //DeductionTypes
        public async Task<IEnumerable<DeductionType>> FetchAllDeductionTypes()
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                return await context.DeductionTypes.ToListAsync();
            }
        }

        public async Task CreateDeductionType(DeductionType deductionType)
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.DeductionTypes.Add(deductionType);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateDeductionType(DeductionType deductionType)
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                var updatedDeductionType = await context.DeductionTypes.FindAsync(deductionType.Id);
                if (updatedDeductionType != null)
                {
                    updatedDeductionType.Name = deductionType.Name;
                    updatedDeductionType.Amount = deductionType.Amount;
                    updatedDeductionType.IsDeductible = deductionType.IsDeductible;
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveDeductionType(DeductionType deductionType)
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.DeductionTypes.Remove(deductionType);
                await context.SaveChangesAsync();
            }
        }


        //Tax DeadLines
        public async Task<IEnumerable<TaxDeadline>> FetchAllTaxDeadLines()
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                return await context.TaxDeadlines.ToListAsync();
            }
        }

        public async Task CreateTaxDeadLine(TaxDeadline taxDeadline)
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.TaxDeadlines.Add(taxDeadline);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateTaxDeadLine(TaxDeadline taxDeadline)
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                var updatedDeadLine = await context.TaxDeadlines.FindAsync(taxDeadline.Id);
                if (updatedDeadLine != null)
                {
                    updatedDeadLine.Period = updatedDeadLine.Period;
                    updatedDeadLine.DueDate = updatedDeadLine.DueDate;
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveTaxDeadLine(TaxDeadline taxDeadline)
        {
            using (AppDbContext context = dbContextFactory.CreateDbContext())
            {
                context.TaxDeadlines.Remove(taxDeadline);
                await context.SaveChangesAsync();
            }
        }
    }
}
