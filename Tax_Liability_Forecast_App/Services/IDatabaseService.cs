using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Services
{
    public interface IDatabaseService
    {
        Task<IEnumerable<Transaction>> GetAllTransactions();
        Task CreateTransaction(Transaction transaction);
        Task UpdateTransaction(Transaction transaction);
        Task DeleteTransaction(Transaction transaction);

        //Client

        Task <IEnumerable<Client>> FetchClientTable();
    }
}
