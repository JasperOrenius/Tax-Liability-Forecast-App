using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Services
{
    public interface IDatabaseService
    {
        Task<IEnumerable<Transaction>> GetAllTransactions();
        Task<IEnumerable<Transaction>> GetTransactionsByClientId(Guid clientId);
        Task CreateTransaction(Transaction transaction);
        Task UpdateTransaction(Transaction transaction);
        Task DeleteTransaction(Transaction transaction);

        //Client

        Task <IEnumerable<Client>> FetchClientTable();
        Task AddClient(Client recordToAdd);
        Task RemoveClient(Client recordToRemove);
    }
}
