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

        public ObservableCollection<Transaction> Incomes { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

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

        private Client selectedClient;
        public Client SelectedClient
        {
            get => selectedClient;
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                LoadTransactions();
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
            LoadClients();
            SelectedIncomeType = IncomeTypes[0];
        }

        private async Task AddEntry()
        {
            if(SelectedClient == null || string.IsNullOrEmpty(Description) || string.IsNullOrEmpty(SelectedIncomeType) || amount <= 0)
            {
                return;
            }
            var newTransaction = new Transaction
            {
                Date = IncomeDate,
                Description = Description,
                Amount = Amount,
                IncomeType = SelectedIncomeType,
                Type = TransactionType.Income,
                ClientId = SelectedClient.Id,
            };
            await databaseService.CreateTransaction(newTransaction);
            Incomes.Add(newTransaction);
            Description = string.Empty;
            Amount = 0;
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Amount));
        }

        private async Task LoadClients()
        {
            var clients = await databaseService.FetchClientTable();
            Clients.Clear();
            Clients = new ObservableCollection<Client>(clients);
            if(Clients.Count > 0)
            {
                SelectedClient = Clients[0];
            }
        }

        private async Task LoadTransactions()
        {
            if (SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            Incomes.Clear();
            foreach(var transaction in transactions)
            {
                if(transaction.Type == TransactionType.Income)
                {
                    Incomes.Add(transaction);
                }
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
            Incomes.Remove(transaction);
            await databaseService.DeleteTransaction(transaction);
            await LoadTransactions();
        }
    }
}
