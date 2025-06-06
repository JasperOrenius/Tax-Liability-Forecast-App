﻿using System;
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
    public class ExpensesViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public ObservableCollection<Transaction> Expenses { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> FilteredExpenses { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<DeductionType> DeductionTypes { get; set; } = new ObservableCollection<DeductionType>();

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

        private Client selectedClient;
        public Client SelectedClient
        {
            get => selectedClient;
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                LoadExpenses();
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
                LoadExpenses();
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
                FilterExpenses();
            }
        }

        private readonly DeductionType NotDeductible = new()
        {
            Id = Guid.Empty,
            Name = "Not Deductible",
            Amount = 0,
            AppliesTo = DeductionAppliesTo.Both
        };

        public ICommand AddEntryCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public ExpensesViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            AddEntryCommand = new RelayCommand(AddEntry);
            EditCommand = new TransactionCommand(EditExpense);
            SaveCommand = new TransactionCommand(SaveExpense);
            DeleteCommand = new TransactionCommand(DeleteExpense);
            LoadData();
        }

        private async Task AddEntry()
        {
            if (SelectedClient == null || string.IsNullOrEmpty(Description) || amount <= 0)
            {
                return;
            }
            var newTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Date = ExpenseDate,
                Description = Description,
                Amount = Amount,
                IncomeType = string.Empty,
                Type = TransactionType.Expense,
                ClientId = SelectedClient.Id,
                DeductionTypeId = SelectedDeductionType.Id == Guid.Empty ? null : SelectedDeductionType.Id,
            };
            await databaseService.CreateTransaction(newTransaction);
            Expenses.Add(newTransaction);
            Description = string.Empty;
            Amount = 0;
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Amount));
            LoadExpenses();
        }

        private async Task LoadData()
        {
            await LoadExpenses();
            await LoadClients();
            await LoadDeductionTypes();
        }

        private async Task LoadClients()
        {
            var clients = await databaseService.FetchClientTable();
            Clients.Clear();
            Clients = new ObservableCollection<Client>(clients);
            if (Clients.Count > 0)
            {
                SelectedClient = Clients[0];
            }
        }

        private async Task LoadDeductionTypes()
        {
            var deductionTypes = await databaseService.FetchAllDeductionTypes();
            DeductionTypes.Clear();
            DeductionTypes.Add(NotDeductible);
            foreach (var deductionType in deductionTypes)
            {
                if (deductionType.AppliesTo == DeductionAppliesTo.Expense || deductionType.AppliesTo == DeductionAppliesTo.Both)
                {
                    DeductionTypes.Add(deductionType);
                }
            }
            SelectedDeductionType = DeductionTypes[0];
        }

        private async Task LoadExpenses()
        {
            if (SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            Expenses.Clear();
            foreach (var transaction in transactions)
            {
                if (transaction.Type == TransactionType.Expense)
                {
                    if (transaction.DeductionTypeId == null)
                    {
                        transaction.DeductionTypeId = Guid.Empty;
                    }
                    if (transaction.DeductionType == null)
                    {
                        transaction.DeductionType = NotDeductible;
                    }
                    Expenses.Add(transaction);
                }
            }
            FilterExpenses();
        }

        private async Task FilterExpenses()
        {
            var filteredExpenses = Expenses.Where(e => string.IsNullOrWhiteSpace(SearchText) || e.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            FilteredExpenses.Clear();
            foreach (var expense in filteredExpenses)
            {
                FilteredExpenses.Add(expense);
            }
        }

        private async Task EditExpense(Transaction transaction)
        {
            if (EditingTransaction == null)
            {
                transaction.IsEditing = true;
                EditingTransaction = transaction;
            }
        }

        private async Task SaveExpense(Transaction transaction)
        {
            if (transaction.DeductionTypeId == Guid.Empty)
            {
                transaction.DeductionTypeId = null;
            }
            transaction.IsEditing = false;
            EditingTransaction = null;
            await databaseService.UpdateTransaction(transaction);
            await LoadExpenses();
        }

        private async Task DeleteExpense(Transaction transaction)
        {
            EditingTransaction = null;
            transaction.DeductionTypeId = null;
            transaction.DeductionType = null;
            Expenses.Remove(transaction);
            await databaseService.DeleteTransaction(transaction);
            await FilterExpenses();
        }
    }
}
