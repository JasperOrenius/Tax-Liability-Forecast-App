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
        public ObservableCollection<Transaction> FilteredIncomes { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<DeductionType> DeductionTypes { get; set; } = new ObservableCollection<DeductionType>();

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
                LoadIncomes();
            }
        }

        private DeductionType selectedDeductionType;
        public DeductionType SelectedDeductionType
        {
            get => selectedDeductionType;
            set
            {
                selectedDeductionType = value;
                OnPropertyChanged(nameof(SelectedDeductionType));
                LoadIncomes();
            }
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterIncomes();
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
            EditCommand = new TransactionCommand(EditIncome);
            SaveCommand = new TransactionCommand(SaveIncome);
            DeleteCommand = new TransactionCommand(DeleteIncome);
            LoadData();
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
                Id = Guid.NewGuid(),
                Date = IncomeDate,
                Description = Description,
                Amount = Amount,
                IncomeType = SelectedIncomeType,
                Type = TransactionType.Income,
                ClientId = SelectedClient.Id,
                DeductionTypeId = SelectedDeductionType.Id == Guid.Empty ? null : SelectedDeductionType.Id,
            };
            await databaseService.CreateTransaction(newTransaction);
            Incomes.Add(newTransaction);
            Description = string.Empty;
            Amount = 0;
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Amount));
            LoadIncomes();
        }

        private async Task LoadData()
        {
            await LoadIncomes();
            await LoadClients();
            await LoadDeductionTypes();
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

        private async Task LoadDeductionTypes()
        {
            var deductionTypes = await databaseService.FetchAllDeductionTypes();
            var notDeductible = new DeductionType { Name = "Not Deductible", Amount = 0, IsDeductible = false, AppliesTo = DeductionAppliesTo.Both };
            DeductionTypes.Clear();
            DeductionTypes.Add(notDeductible);
            foreach(var deductionType in deductionTypes)
            {
                if(deductionType.AppliesTo == DeductionAppliesTo.Income || deductionType.AppliesTo == DeductionAppliesTo.Both)
                {
                    DeductionTypes.Add(deductionType);
                }
            }
            SelectedDeductionType = DeductionTypes[0];
        }

        private async Task LoadIncomes()
        {
            if (SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            Incomes.Clear();
            var notDeductible = DeductionTypes.FirstOrDefault(d => d.Id == Guid.Empty);
            foreach(var transaction in transactions)
            {
                if(transaction.Type == TransactionType.Income)
                {
                    if(transaction.DeductionType == null)
                    {
                        transaction.DeductionType = notDeductible;
                    }
                    Incomes.Add(transaction);
                }
            }
            FilterIncomes();
        }

        private async Task FilterIncomes()
        {
            var filteredIncomes = Incomes.Where(i => string.IsNullOrWhiteSpace(SearchText) || i.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            FilteredIncomes.Clear();
            foreach(var income in filteredIncomes)
            {
                FilteredIncomes.Add(income);
            }
        }

        private async Task EditIncome(Transaction transaction)
        {
            if(EditingTransaction == null)
            {
                transaction.IsEditing = true;
                EditingTransaction = transaction;    
            }
        }

        private async Task SaveIncome(Transaction transaction)
        {
            if(transaction.DeductionType?.Id == Guid.Empty)
            {
                transaction.DeductionTypeId = null;
            }
            else
            {
                transaction.DeductionTypeId = transaction.DeductionType?.Id;
            }
            transaction.IsEditing = false;
            EditingTransaction = null;
            await databaseService.UpdateTransaction(transaction);
            await LoadIncomes();
        }

        private async Task DeleteIncome(Transaction transaction)
        {
            EditingTransaction = null;
            Incomes.Remove(transaction);
            await databaseService.DeleteTransaction(transaction);
            await FilterIncomes();
        }
    }
}
