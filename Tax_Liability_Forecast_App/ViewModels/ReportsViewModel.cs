using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;
using Tax_Liability_Forecast_App.Utils;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public SeriesCollection IncomeExpenseSeries { get; set; } = new SeriesCollection();
        public SeriesCollection TaxOverTimeSeries { get; set; } = new SeriesCollection();
        public List<string> MonthLabels { get; set; } = new List<string>();

        public ObservableCollection<Transaction> Incomes { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Expenses { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        public List<String> TransactionTypes { get; } = new List<String> { "All", "Income", "Expense" };

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

        private string selectedTransactionType;
        public string SelectedTransactionType
        {
            get => selectedTransactionType;
            set
            {
                selectedTransactionType = value;
                OnPropertyChanged(nameof(SelectedTransactionType));
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

        private decimal totalExpenses;
        public decimal TotalExpenses
        {
            get => totalExpenses;
            set
            {
                totalExpenses = value;
                OnPropertyChanged(nameof(TotalExpenses));
            }
        }

        private decimal netIncome;
        public decimal NetIncome
        {
            get => netIncome;
            set
            {
                netIncome = value;
                OnPropertyChanged(nameof(NetIncome));
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

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToPDFCommand { get; }

        public ReportsViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportToPDFCommand = new RelayCommand(ExportToPDF);
            LoadClients();
            SelectedTransactionType = TransactionTypes[0];
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

        private async Task GenerateReport()
        {
            if (SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            var taxBrackets = await databaseService.FetchAllTaxBrackets();
            var taxOverTime = new List<(DateTime Month, decimal Income, decimal Tax, decimal Net)>();
            var taxValues = new ChartValues<decimal>();
            var netIncomeValues = new ChartValues<decimal>();
            int currentYear = DateTime.Now.Year;
            decimal totalNetIncome = 0m;
            decimal totalEstimatedTax = 0m;
            MonthLabels.Clear();
            Incomes.Clear();
            Expenses.Clear();
            Transactions = new ObservableCollection<Transaction>(transactions);
            foreach(var transaction in transactions)
            {
                if(transaction.Type == TransactionType.Income)
                {
                    Incomes.Add(transaction);
                }
                else
                {
                    Expenses.Add(transaction);
                }
            }
            var incomeExpenseGroup = transactions
                .GroupBy(t => t.Type)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                .ToList();
            var monthlyGroups = transactions
                .Where(t => t.Type == TransactionType.Income && t.Date.Year == currentYear)
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .OrderBy(g => g.Key);
            var monthlyData = transactions
                .Where(t => t.Type == TransactionType.Income && t.Date.Year == DateTime.Now.Year)
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    decimal income = g.Sum(t => t.Amount);
                    decimal tax = CalculateEstimatedTax(income, taxBrackets.ToList());
                    decimal netIncome = income - tax;
                    return new { Date = g.Key, Tax = tax, NetIncome = netIncome };
                })
                .ToList();

            foreach(var group in incomeExpenseGroup)
            {
                IncomeExpenseSeries.Add(new PieSeries
                {
                    Title = group.Type.ToString(),
                    Values = new ChartValues<decimal> { group.Total }
                });
            }
            foreach(var group in monthlyGroups)
            {
                var totalIncome = group.Sum(t => t.Amount);
                var tax = CalculateEstimatedTax(totalIncome, taxBrackets.ToList());
                var netIncome = totalIncome - tax;
                totalNetIncome += netIncome;
                totalEstimatedTax += tax;

                taxOverTime.Add((group.Key, totalIncome, tax, netIncome));
            }
            foreach(var item in monthlyData)
            {
                taxValues.Add(item.Tax);
                netIncomeValues.Add(item.NetIncome);
                MonthLabels.Add(item.Date.ToString("MMM yyyy"));
            }

            TaxOverTimeSeries.Clear();
            TaxOverTimeSeries.Add(new LineSeries
            {
                Title = "Tax Owed",
                Values = taxValues,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 6
            });
            TaxOverTimeSeries.Add(new LineSeries
            {
                Title = "Net Income",
                Values = netIncomeValues,
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 6
            });

            TotalIncome = Incomes.Sum(i => i.Amount);
            TotalExpenses = Expenses.Sum(e => e.Amount);
            NetIncome = totalNetIncome;
            EstimatedTax = totalEstimatedTax;
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

        private async Task ExportToPDF()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                DefaultExt = ".pdf"
            };
            if(saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                GeneratePDFReport(filePath);

                MessageBox.Show($"File saved at {filePath}");
            }
            else
            {
                MessageBox.Show("File save operation was cancelled");
            }
        }

        private void GeneratePDFReport(string filePath)
        {
            try
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Tax Report";

                PdfPage page = document.AddPage();

                XGraphics gfx = XGraphics.FromPdfPage(page);

                GlobalFontSettings.FontResolver = new WindowsFontResolver();

                XFont titleFont = new XFont("Verdana", 16, XFontStyleEx.Bold);
                XFont regularFont = new XFont("Verdana", 12);

                gfx.DrawString("Tax Report", titleFont, XBrushes.Black, new XPoint(50, 50));

                gfx.DrawString($"Date: {DateTime.Now.ToShortDateString()}", regularFont, XBrushes.Black, new XPoint(50, 80));

                gfx.DrawString($"Total Income: {TotalIncome.ToString("C")}", regularFont, XBrushes.Black, new XPoint(50, 110));
                gfx.DrawString($"Total Expenses: {TotalExpenses.ToString("C")}", regularFont, XBrushes.Black, new XPoint(50, 130));
                gfx.DrawString($"Net Income: {NetIncome.ToString("C")}", regularFont, XBrushes.Black, new XPoint(50, 150));
                gfx.DrawString($"Estimated Tax: {EstimatedTax.ToString("C")}", regularFont, XBrushes.Black, new XPoint(50, 170));

                document.Save(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}