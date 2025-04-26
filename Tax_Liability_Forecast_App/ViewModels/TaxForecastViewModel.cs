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
    public class TaxForecastViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public ObservableCollection<Transaction> Incomes { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Expenses { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        private Client selectedClient;
        public Client SelectedClient
        {
            get => selectedClient;
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
            }
        }

        private int year;
        public int Year
        {
            get => year;
            set
            {
                year = value;
                OnPropertyChanged(nameof(Year));
            }
        }

        private decimal totalIncome;
        public decimal TotalIncome
        {
            get => totalIncome;
            set
            {
                totalIncome = value;
                OnPropertyChanged(nameof(TotalIncome));
            }
        }

        private decimal taxableIncome;
        public decimal TaxableIncome
        {
            get => taxableIncome;
            set
            {
                taxableIncome = value;
                OnPropertyChanged(nameof(TaxableIncome));
            }
        }

        private decimal totalDeductions;
        public decimal TotalDeductions
        {
            get => totalDeductions;
            set
            {
                totalDeductions = value;
                OnPropertyChanged(nameof(TotalDeductions));
            }
        }

        private decimal estimatedTax;
        public decimal EstimatedTax
        {
            get => estimatedTax;
            set
            {
                estimatedTax = value;
                OnPropertyChanged(nameof(EstimatedTax));
            }
        }

        public ICommand GenerateForecastCommand { get; }

        public TaxForecastViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            GenerateForecastCommand = new RelayCommand(GenerateForecast);
            LoadClients();
            Year = DateTime.Now.Year;
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

        private async Task GenerateForecast()
        {
            if (Year < 1 || SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            var taxBrackets = await databaseService.FetchAllTaxBrackets();
            var taxBracketList = new List<TaxBracket>(taxBrackets);

            Incomes.Clear();
            Expenses.Clear();
            foreach(var transaction in transactions)
            {
                if(transaction.Type == TransactionType.Income && transaction.Date.Year == Year)
                {
                    Incomes.Add(transaction);
                }
                else if(transaction.Type == TransactionType.Expense && transaction.Date.Year == Year)
                {
                    Expenses.Add(transaction);
                }
            }
            TotalIncome = Incomes.Sum(i => i.Amount);
            TaxableIncome = CalculateEstimatedTax(TotalIncome, taxBracketList);
        }
        
        private decimal CalculateEstimatedTax(decimal totalIncome, List<TaxBracket> taxBrackets)
        {
            const decimal defaultTaxRate = 7.38m;
            if(taxBrackets == null || taxBrackets.Count == 0)
            {
                return totalIncome * (defaultTaxRate / 100m);
            }

            var sortedBrackets = taxBrackets.OrderBy(b => b.MinIncome).ToList();
            foreach(var bracket in sortedBrackets)
            {
                if(totalIncome >= bracket.MinIncome && totalIncome <= bracket.MaxIncome)
                {
                    return totalIncome * (bracket.TaxRate / 100m);
                }
            }

            var highestBracket = sortedBrackets.LastOrDefault();
            if(highestBracket != null && totalIncome > highestBracket.MaxIncome)
            {
                return totalIncome * (highestBracket.TaxRate / 100m);
            }

            return totalIncome * (defaultTaxRate / 100m);
        }
    }
}