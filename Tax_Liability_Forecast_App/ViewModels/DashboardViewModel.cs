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
        public SeriesCollection TimeForecastSeries { get; set; } = new SeriesCollection();
        public ObservableCollection<string> TimeLabels { get; set; } = new ObservableCollection<string>();
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
            set { income = value; OnPropertyChanged(nameof(Income)); }
        }

        public decimal Expense
        {
            get => expense;
            set { expense = value; OnPropertyChanged(nameof(Expense)); }
        }

        public decimal EstimatedTax
        {
            get => estimatedTax;
            set { estimatedTax = value; OnPropertyChanged(nameof(EstimatedTax)); }
        }

        public string DeadlineText
        {
            get => deadlineText;
            set { deadlineText = value; OnPropertyChanged(nameof(DeadlineText)); }
        }

        private TaxDeadline nextDeadline;
        public TaxDeadline NextDeadline
        {
            get => nextDeadline;
            set { nextDeadline = value; OnPropertyChanged(nameof(NextDeadline)); }
        }

        public DashboardViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            IncomevsExpenseSeries = new SeriesCollection();
            LoadClient();
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
                        Values = new ChartValues<decimal> { group.Total }
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
            var deadlines = await databaseService.FetchAllTaxDeadLines();

            var upcoming = deadlines
                .Where(d => !d.IsEmpty && d.DueDate >= DateTime.Today)
                .OrderBy(d => d.DueDate)
                .FirstOrDefault();

            DeadlineText = upcoming != null
                ? $"{upcoming.Period} — {upcoming.DueDate:MMMM dd, yyyy}"
                : "No upcoming deadlines";

            NextDeadline = upcoming;

            TransactionList = transactions.ToList();
            IncomeList = TransactionList.Where(t => t.Type == TransactionType.Income).ToList();
            ExpenseList = TransactionList.Where(t => t.Type == TransactionType.Expense).ToList();

            Income = IncomeList.Sum(i => i.Amount);
            Expense = ExpenseList.Sum(t => t.Amount);
            EstimatedTax = CalculateEstimatedTax(Income, taxBrackets.ToList());

            UpdateChart();
            GenerateTimeForecast(TransactionList);
        }

        private void GenerateTimeForecast(List<Transaction> transactions)
        {
            TimeForecastSeries.Clear();
            TimeLabels.Clear();

            var groupedByMonth = transactions
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month = g.Key,
                    Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                    Net = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) -
                          g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                })
                .ToList();

            System.Diagnostics.Debug.WriteLine("Grouped months: " + groupedByMonth.Count); // add this

            var incomeValues = new ChartValues<decimal>();
            var expenseValues = new ChartValues<decimal>();
            var netValues = new ChartValues<decimal>();

            foreach (var item in groupedByMonth)
            {
                incomeValues.Add(item.Income);
                expenseValues.Add(item.Expense);
                netValues.Add(item.Net);
                TimeLabels.Add(item.Month.ToString("MMM yyyy"));
            }

            TimeForecastSeries.Add(new LineSeries
            {
                Title = "Income",
                Values = incomeValues,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5

            });

            TimeForecastSeries.Add(new LineSeries
            {
                Title = "Expenses",
                Values = expenseValues,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5

            });

            TimeForecastSeries.Add(new LineSeries
            {
                Title = "Net",
                Values = netValues,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5

            });

            OnPropertyChanged(nameof(TimeForecastSeries));
            OnPropertyChanged(nameof(TimeLabels));
        }

    }
}
