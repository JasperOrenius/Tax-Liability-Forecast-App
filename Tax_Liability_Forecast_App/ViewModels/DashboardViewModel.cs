using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public SeriesCollection IncomevsExpenseSeries { get; set; }

        List<Transaction> IncomeList { get; set; } = new List<Transaction>();
        List<Transaction> ExpenseList { get; set; } = new List<Transaction>();
        List<Transaction> TransactionList { get; set; } = new List<Transaction>();

        public ChartValues<decimal> TimeForecast { get; set; } = new ChartValues<decimal>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        private Client selectedClient;
        public Client SelectedClient
        {
            get => selectedClient;
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
                if (selectedClient != null)
                    _ = GenerateDashboard();
            }
        }

        private readonly IDatabaseService databaseService;

        private decimal income;
        private decimal expense;
        private decimal estimatedTax;
        private string deadlineText;

        public decimal Income
        {
            get => income;
            set
            {
                income = value;
                OnPropertyChanged(nameof(Income));
            }
        }

        public decimal Expense
        {
            get => expense;
            set
            {
                expense = value;
                OnPropertyChanged(nameof(Expense));
            }
        }

        public decimal EstimatedTax
        {
            get => estimatedTax;
            set
            {
                estimatedTax = value;
                OnPropertyChanged(nameof(EstimatedTax));
            }
        }

        public string DeadlineText
        {
            get => deadlineText;
            set
            {
                deadlineText = value;
                OnPropertyChanged(nameof(DeadlineText));
            }
        }

        private TaxDeadline nextDeadline;
        public TaxDeadline NextDeadline
        {
            get => nextDeadline;
            set
            {
                nextDeadline = value;
                OnPropertyChanged(nameof(NextDeadline));
            }
        }


        public DashboardViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            IncomevsExpenseSeries = new SeriesCollection();
            _ = LoadClient();
            
        }

        private async Task LoadClient()
        {
            var clients = await databaseService.FetchClientTable();
            Clients = new ObservableCollection<Client>(clients);

            OnPropertyChanged(nameof(Clients));

            if (Clients.Count > 0)
                SelectedClient = Clients[0];
        }

        private void UpdateChart()
        {
            if (IncomevsExpenseSeries == null)
                IncomevsExpenseSeries = new SeriesCollection();

            IncomevsExpenseSeries.Clear();

            var transactionGroup = TransactionList
                .GroupBy(t => t.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Sum(t => t.Amount)
                });

            foreach (var group in transactionGroup)
            {
                if (group.Total > 0)
                {
                    IncomevsExpenseSeries.Add(new PieSeries
                    {
                        Title = group.Type.ToString(),
                        Values = new ChartValues<decimal> { group.Total },
                        
                    });
                }
            }

            OnPropertyChanged(nameof(IncomevsExpenseSeries));
        }

        private decimal CalculateEstimatedTax(decimal income, List<TaxBracket> taxBrackets)
        {
            const decimal defaultTaxRate = 7.38m;
            if (taxBrackets == null || taxBrackets.Count == 0)
                return income * (defaultTaxRate / 100m);

            var sortedBrackets = taxBrackets.OrderBy(b => b.MinIncome).ToList();
            foreach (var bracket in sortedBrackets)
            {
                if (income >= bracket.MinIncome && income <= bracket.MaxIncome)
                    return income * (bracket.TaxRate / 100m);
            }

            var highestBracket = sortedBrackets.LastOrDefault();
            if (highestBracket != null && income > highestBracket.MaxIncome)
                return income * (highestBracket.TaxRate / 100m);

            return income * (defaultTaxRate / 100m);
        }

        private async Task GenerateDashboard()
        {
            if (SelectedClient == null)
                return;

            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            var taxBrackets = await databaseService.FetchAllTaxBrackets();
            var taxBracketList = new List<TaxBracket>(taxBrackets);

            var deadlines = await databaseService.FetchAllTaxDeadLines();
            var upcoming = deadlines
                .Where(d => !d.IsEmpty && d.DueDate >= DateTime.Today)
                .OrderBy(d => d.DueDate)
                .FirstOrDefault();

            DeadlineText = upcoming != null
                ? $"{upcoming.Period} — {upcoming.DueDate:MMMM dd, yyyy}"
                : "No upcoming deadlines";


            NextDeadline = upcoming;

            TransactionList.Clear();
            IncomeList.Clear();
            ExpenseList.Clear();

            foreach (var transaction in transactions)
            {
                TransactionList.Add(transaction);
                if (transaction.Type == TransactionType.Income)
                    IncomeList.Add(transaction);
                else if (transaction.Type == TransactionType.Expense)
                    ExpenseList.Add(transaction);
            }

            Income = IncomeList.Sum(i => i.Amount);
            Expense = ExpenseList.Sum(t => t.Amount);
            EstimatedTax = CalculateEstimatedTax(Income, taxBracketList);

            UpdateChart();
        }

    }
}
