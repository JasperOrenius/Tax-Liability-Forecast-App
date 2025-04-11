using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class IncomeViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();

        public DateTime IncomeDate { get; set; } = DateTime.Today;
        public List<string> IncomeTypes { get; } = new List<string>
        {
            "Wages", "Salary", "Commission", "Interest", "Sales", "Investments", "Gifts", "Allowance/Pocket Money", "Government Payments"
        };

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

        private string selectedIncomeType;
        public string SelectedIncomeType
        {
            get => selectedIncomeType;
            set
            {
                selectedIncomeType = value;
                OnPropertyChanged(nameof(SelectedIncomeType));
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

        public IncomeViewModel(IDatabaseService databaseService)
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
            if(string.IsNullOrEmpty(Description) || string.IsNullOrEmpty(SelectedIncomeType) || amount <= 0)
            {
                return;
            }
            var newTransaction = new Transaction
            {
                Date = IncomeDate,
                Description = Description,
                Amount = Amount,
                IncomeType = SelectedIncomeType,
                Type = TransactionType.Income
            };
            await databaseService.CreateTransaction(newTransaction);
            Transactions.Add(newTransaction);
            Description = string.Empty;
            Amount = 0;
            SelectedIncomeType = string.Empty;
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Amount));
            OnPropertyChanged(nameof(selectedIncomeType));
        }

        private async Task LoadTransactions()
        {
            var transactions = await databaseService.GetAllTransactions();
            Transactions.Clear();
            foreach(var transaction in transactions)
            {
                Transactions.Add(transaction);
            }
        }

        private async Task EditTransaction(Transaction transaction)
        {
            if(EditingTransaction == null)
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
            Transactions.Remove(transaction);
            await databaseService.DeleteTransaction(transaction);
            await LoadTransactions();
        }
    }
}
