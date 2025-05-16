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
        Task UpdateClient(Client client);
        Task RemoveClient(Client recordToRemove);

        //Tax Brackets
        Task<IEnumerable<TaxBracket>> FetchAllTaxBrackets();
        Task CreateTaxBracket(TaxBracket taxBracket);
        Task UpdateTaxBracket(TaxBracket taxBracket);
        Task RemoveTaxBracket(TaxBracket taxBracket);

        //Deduction Type
        Task<IEnumerable<DeductionType>> FetchAllDeductionTypes();
        Task CreateDeductionType(DeductionType deductionType);
        Task UpdateDeductionType(DeductionType deductionType);
        Task RemoveDeductionType(DeductionType deductionType);

        //Tax Deadline
        Task<IEnumerable<TaxDeadline>> FetchAllTaxDeadLines();
        Task CreateTaxDeadLine(TaxDeadline taxDeadline);
        Task UpdateTaxDeadLine(TaxDeadline taxDeadline);
        Task RemoveTaxDeadLine(TaxDeadline taxDeadline);
    }
}
