using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;
using Tax_Liability_Forecast_App.Utils;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;
        private IChartFactory chartFactory = new ChartFactory();

        public SeriesCollection IncomeExpenseSeries { get; set; } = new SeriesCollection();
        public SeriesCollection TaxOverTimeSeries { get; set; } = new SeriesCollection();
        public List<string> MonthLabels { get; set; } = new List<string>();

        public ObservableCollection<Transaction> Incomes { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Expenses { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Transactions { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        private List<TaxBracket> TaxBrackets = new List<TaxBracket>();

        public List<String> TransactionTypes { get; } = new List<String> { "All", "Income", "Expense" };

        private bool isReportGenerated = false;

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
            Year = DateTime.Today.Year;
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
            if (SelectedClient == null || Year < 0) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            var taxBrackets = await databaseService.FetchAllTaxBrackets();
            var taxOverTime = new List<(DateTime Month, decimal Income, decimal Tax, decimal Net)>();
            var taxValues = new ChartValues<decimal>();
            var netIncomeValues = new ChartValues<decimal>();
            decimal totalNetIncome = 0m;
            decimal totalEstimatedTax = 0m;
            MonthLabels.Clear();
            Incomes.Clear();
            Expenses.Clear();
            TaxBrackets = new List<TaxBracket>(taxBrackets);
            foreach(var transaction in transactions)
            {
                if(transaction.Date.Year == Year)
                {
                    Transactions.Add(transaction);
                    if(transaction.Type == TransactionType.Income)
                    {
                        Incomes.Add(transaction);
                    }
                    else
                    {
                        Expenses.Add(transaction);
                    }
                }
            }
            var incomeExpenseGroup = Transactions
                .GroupBy(t => t.Type)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                .ToList();
            var monthlyGroups = Transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .OrderBy(g => g.Key);
            var monthlyData = Transactions
                .Where(t => t.Type == TransactionType.Income)
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

            isReportGenerated = true;
        }

        private (Dictionary<string, decimal> incomeExpenseData, List<(DateTime month, decimal tax, decimal net)> taxOverTimeData) GetChartData(List<Transaction> transactions, List<TaxBracket> taxBrackets)
        {
            var incomeExpenseGroup = transactions
                .GroupBy(t => t.Type)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                .ToList();

            var incomeExpenseData = incomeExpenseGroup
                .ToDictionary(x => x.Type.ToString(), x => x.Total);

            int currentYear = DateTime.Now.Year;
            var monthlyGroups = transactions
                .Where(t => t.Type == TransactionType.Income && t.Date.Year == currentYear)
                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
                .OrderBy(g => g.Key);

            var taxOverTimeData = new List<(DateTime month, decimal tax, decimal net)>();
            foreach (var group in monthlyGroups)
            {
                var totalIncome = group.Sum(t => t.Amount);
                var tax = CalculateEstimatedTax(totalIncome, taxBrackets.ToList());
                var netIncome = totalIncome - tax;
                taxOverTimeData.Add((group.Key, tax, netIncome));
            }

            return (incomeExpenseData, taxOverTimeData);
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
            if(!isReportGenerated)
            {
                MessageBox.Show("Generate report first!");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                DefaultExt = ".pdf"
            };
            if(saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                GeneratePDFReport(Transactions.ToList(), filePath);

                MessageBox.Show($"File saved at {filePath}");
            }
            else
            {
                MessageBox.Show("File save operation was cancelled");
            }
        }

        private void GeneratePDFReport(List<Transaction> transactions, string filePath)
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
                XFont legendFont = new XFont("Verdana", 10);

                int margin = 40;
                int padding = 10;
                double yOffset = margin;

                double pageWidth = page.Width - 100;

                gfx.DrawString("Tax Report", titleFont, XBrushes.Black, new XPoint(50, yOffset));
                yOffset += 30;

                gfx.DrawString($"Date: {DateTime.Now.ToShortDateString()}", regularFont, XBrushes.Black, new XPoint(margin, yOffset));
                yOffset += 20;

                gfx.DrawString($"Total Income: {TotalIncome.ToString("C")}", regularFont, XBrushes.Black, new XPoint(margin, yOffset));
                yOffset += 20;
                gfx.DrawString($"Total Expenses: {TotalExpenses.ToString("C")}", regularFont, XBrushes.Black, new XPoint(margin, yOffset));
                yOffset += 20;
                gfx.DrawString($"Net Income: {NetIncome.ToString("C")}", regularFont, XBrushes.Black, new XPoint(margin, yOffset));
                yOffset += 20;
                gfx.DrawString($"Estimated Tax: {EstimatedTax.ToString("C")}", regularFont, XBrushes.Black, new XPoint(margin, yOffset));
                yOffset += 30;

                var (incomeExpenseData, taxOverTimeData) = GetChartData(Transactions.ToList(), TaxBrackets);

                double availableHeight = page.Height - yOffset;

                double pieChartHeight = availableHeight * 0.4;
                double lineChartHeight = availableHeight * 0.6;

                var incomeExpenseChartBytes = chartFactory.CreateIncomeExpensePieChart(incomeExpenseData, pageWidth, pieChartHeight);
                var taxOverTimeChartBytes = chartFactory.CreateTaxOverTimeLineChart(taxOverTimeData, pageWidth, lineChartHeight);

                var incomeExpenseChartStream = new MemoryStream();
                using(var originalStream = new MemoryStream(incomeExpenseChartBytes))
                {
                    originalStream.CopyTo(incomeExpenseChartStream);
                }
                incomeExpenseChartStream.Position = 0;
                var incomeExpenseChartImg = XImage.FromStream(incomeExpenseChartStream);
                double incomeExpenseChartWidth = 400;
                double incomeExpenseChartHeight = incomeExpenseChartImg.PixelHeight * (incomeExpenseChartWidth / incomeExpenseChartImg.PixelWidth);
                gfx.DrawImage(incomeExpenseChartImg, margin, yOffset, incomeExpenseChartWidth, incomeExpenseChartHeight);
                yOffset += incomeExpenseChartHeight + 20;

                var taxOverTimeChartStream = new MemoryStream();
                using(var originalStream = new MemoryStream(taxOverTimeChartBytes))
                {
                    originalStream.CopyTo(taxOverTimeChartStream);
                }
                taxOverTimeChartStream.Position = 0;
                var taxOverTimeChartImg = XImage.FromStream(taxOverTimeChartStream);
                double taxOverTimeChartWidth = 400;
                double taxOverTimeChartHeight = taxOverTimeChartImg.PixelHeight * (taxOverTimeChartWidth / taxOverTimeChartImg.PixelWidth);
                gfx.DrawImage(taxOverTimeChartImg, margin, yOffset, taxOverTimeChartWidth, taxOverTimeChartHeight);
                yOffset += taxOverTimeChartHeight + 30;

                page = document.AddPage();
                gfx.Dispose();
                gfx = XGraphics.FromPdfPage(page);
                yOffset = margin;

                int maxDateWidth = (int)transactions.Max(t => gfx.MeasureString(t.Date.ToShortDateString(), regularFont).Width);
                int maxClientWidth = (int)transactions.Max(t => gfx.MeasureString(t.Client.Name, regularFont).Width);
                int maxDescriptionWidth = (int)transactions.Max(t => gfx.MeasureString(t.Description, regularFont).Width);
                int maxTypeWidth = (int)transactions.Max(t => gfx.MeasureString(t.Type.ToString(), regularFont).Width);
                int maxAmountWidth = (int)transactions.Max(t => gfx.MeasureString(t.Amount.ToString("C"), regularFont).Width);

                maxDateWidth += padding;
                maxClientWidth += padding;
                maxDescriptionWidth += padding;
                maxTypeWidth += padding;
                maxAmountWidth += padding;

                double totalWidth = maxDateWidth + maxClientWidth + maxDescriptionWidth + maxTypeWidth + maxAmountWidth;
                double scaleFactor = pageWidth / totalWidth;

                double adjustedDateWidth = maxDateWidth * scaleFactor;
                double adjustedClientWidth = maxClientWidth * scaleFactor;
                double adjustedDescriptionWidth = maxDescriptionWidth * scaleFactor;
                double adjustedTypeWidth = maxTypeWidth * scaleFactor;
                double adjustedAmountWidth = maxAmountWidth * scaleFactor;

                int rowHeight = 20;
                int bottomMargin = 40;
                gfx.DrawString("Date", regularFont, XBrushes.Black, new XPoint(margin, yOffset));
                gfx.DrawString("Client", regularFont, XBrushes.Black, new XPoint(margin + adjustedDateWidth, yOffset));
                gfx.DrawString("Description", regularFont, XBrushes.Black, new XPoint(margin + adjustedDateWidth + adjustedClientWidth, yOffset));
                gfx.DrawString("Type", regularFont, XBrushes.Black, new XPoint(margin + adjustedDateWidth + adjustedClientWidth + adjustedDescriptionWidth, yOffset));
                gfx.DrawString("Amount", regularFont, XBrushes.Black, new XPoint(margin + adjustedDateWidth + adjustedClientWidth + adjustedDescriptionWidth + adjustedTypeWidth, yOffset));

                yOffset += rowHeight;
                gfx.DrawLine(XPens.Black, margin, yOffset, page.Width - margin, yOffset);
                yOffset += 20;

                foreach (var transaction in transactions)
                {
                    if(yOffset + rowHeight + bottomMargin > page.Height)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yOffset = margin;
                    }
                    gfx.DrawString(transaction.Date.ToShortDateString(), regularFont, XBrushes.Black, new XPoint(50, yOffset));
                    gfx.DrawString(transaction.Client.Name, regularFont, XBrushes.Black, new XPoint(50 + adjustedDateWidth, yOffset));
                    gfx.DrawString(transaction.Description, regularFont, XBrushes.Black, new XPoint(50 + adjustedDateWidth + adjustedClientWidth, yOffset));
                    gfx.DrawString(transaction.Type.ToString(), regularFont, XBrushes.Black, new XPoint(50 + adjustedDateWidth + adjustedClientWidth + adjustedDescriptionWidth, yOffset));
                    gfx.DrawString(transaction.Amount.ToString(), regularFont, XBrushes.Black, new XPoint(50 + adjustedDateWidth + adjustedClientWidth + adjustedDescriptionWidth + adjustedTypeWidth, yOffset));

                    yOffset += rowHeight;
                }

                document.Save(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}