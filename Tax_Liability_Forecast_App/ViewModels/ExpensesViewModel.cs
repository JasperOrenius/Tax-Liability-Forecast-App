using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class ExpensesViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public ObservableCollection<Transaction> Expenses { get; set; } = new ObservableCollection<Transaction>();
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private decimal amount;
        public decimal Amount
        {
            get => amount;
            set
            {
                amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private Transaction? editingTransaction;
        public Transaction? EditingTransaction
        {
            get => editingTransaction;
            set
            {
                editingTransaction = value;
                OnPropertyChanged(nameof(EditingTransaction));
            }
        }

        public ICommand AddEntryCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public ExpensesViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            AddEntryCommand = new RelayCommand(AddEntry);
            EditCommand = new TransactionCommand(EditTransaction);
            SaveCommand = new TransactionCommand(SaveTransaction);
            DeleteCommand = new TransactionCommand(DeleteTransaction);
            LoadTransactions();
        }

        private async Task AddEntry()
        {
            if (string.IsNullOrEmpty(Description) || amount <= 0)
            {
                return;
            }
            var newTransaction = new Transaction
            {
                Date = ExpenseDate,
                Description = Description,
                Amount = Amount,
                IncomeType = string.Empty,
                Type = TransactionType.Expense
            };
            await databaseService.CreateTransaction(newTransaction);
            Expenses.Add(newTransaction);
            Description = string.Empty;
            Amount = 0;
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Amount));
        }

        private async Task LoadTransactions()
        {
            var transactions = await databaseService.GetAllTransactions();
            Expenses.Clear();
            foreach (var transaction in transactions)
            {
                if (transaction.Type == TransactionType.Expense)
                {
                    Expenses.Add(transaction);
                }
            }
        }

        private async Task EditTransaction(Transaction transaction)
        {
            if (EditingTransaction == null)
            {
                transaction.IsEditing = true;
                EditingTransaction = transaction;
            }
        }

        private async Task SaveTransaction(Transaction transaction)
        {
            transaction.IsEditing = false;
            EditingTransaction = null;
            await databaseService.UpdateTransaction(transaction);
            await LoadTransactions();
        }

        private async Task DeleteTransaction(Transaction transaction)
        {
            Expenses.Remove(transaction);
            await databaseService.DeleteTransaction(transaction);
            await LoadTransactions();
        }
    }
}
