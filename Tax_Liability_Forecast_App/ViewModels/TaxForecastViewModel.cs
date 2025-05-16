using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private ObservableCollection<Transaction> dataGridSource = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> DataGridSource
        {
            get => dataGridSource;
            set
            {
                dataGridSource = value;
                OnPropertyChanged(nameof(dataGridSource));
            }
        }

        private ObservableCollection<DataGridData> datagridSource2 = new ObservableCollection<DataGridData>();
        public ObservableCollection <DataGridData> DataGridSource2
        {
            get => datagridSource2;
            set
            {
                datagridSource2 = value;
                OnPropertyChanged(nameof(datagridSource2));
            }
        }
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

        private string comboboxSelectedItem;
        public string ComboboxSelectedItem
        {
            get => comboboxSelectedItem;
            set
            {
                comboboxSelectedItem = value;
                OnPropertyChanged(nameof(ComboboxSelectedItem));
            }
        }

        public List<string> TransactionTypes { get; } = new List<string> { "All", "Income", "Expense" };

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

        public List<decimal> TaxByMonth = [0,0,0,0,0,0,0,0,0,0,0,0];

        public ICommand GenerateForecastCommand { get; }

        public TaxForecastViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            GenerateForecastCommand = new RelayCommand(GenerateForecast);
            LoadClients();
            Year = DateTime.Now.Year;
            TotalIncome = 0m;
            TaxableIncome = 0m;
            TotalDeductions = 0m;
            EstimatedTax = 0m;
            ComboboxSelectedItem = TransactionTypes[0];
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

        private async Task GenerateForecast()
        {
            if (Year < 1 || SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            var taxBrackets = await databaseService.FetchAllTaxBrackets();
            var taxBracketList = new List<TaxBracket>(taxBrackets);
            var deductions = await databaseService.FetchAllDeductionTypes();

            Incomes.Clear();
            Expenses.Clear();
            foreach (var transaction in transactions)
            {
                if (transaction.Type == TransactionType.Income && transaction.Date.Year == Year)
                {
                    Incomes.Add(transaction);
                }
                else if (transaction.Type == TransactionType.Expense && transaction.Date.Year == Year)
                {
                    Expenses.Add(transaction);
                }
            }
            TaxableIncome = 0m;
            TotalDeductions = 0m;
            List<decimal> taxableIncomeAndTotalDeductions = CalculateTaxableIncome(Incomes.ToList(), taxBracketList, deductions.ToList());
            TaxableIncome = taxableIncomeAndTotalDeductions[0];
            TotalDeductions = taxableIncomeAndTotalDeductions[1];
            TotalIncome = Incomes.Sum(i => i.Amount);
            EstimatedTax = CalculateEstimatedTax(TaxableIncome, taxBracketList);
            DataGridSource.Clear();
            switch (ComboboxSelectedItem)
            {
                case "All":
                    DataGridSource = new ObservableCollection<Transaction>(transactions.Where(t => t.Date.Year == Year));
                    break;
                case "Income":
                    DataGridSource = Incomes;
                    break;
                case "Expense":
                    DataGridSource = Expenses;
                    break;
            }
            TaxPerMonth(taxBracketList);
            InsertDataToPieChart(TaxableIncome, TotalDeductions, EstimatedTax);

            DataGridSource2.Clear();
            foreach (var transaction in DataGridSource)
            {
                DataGridSource2.Add(CreateDatagridRowData(transaction.IncomeType, transaction.Amount, transaction?.DeductionTypeId, taxBracketList, deductions.ToList()));
            }
        }

        private decimal CalculateEstimatedTax(decimal totalIncome, List<TaxBracket> taxBrackets)
        {
            const decimal defaultTaxRate = 7.38m;
            if (taxBrackets == null || taxBrackets.Count == 0)
            {
                return totalIncome * (defaultTaxRate / 100m);
            }

            var sortedBrackets = taxBrackets.OrderBy(b => b.MinIncome).ToList();
            foreach (var bracket in sortedBrackets)
            {
                if (totalIncome >= bracket.MinIncome && totalIncome <= bracket.MaxIncome)
                {
                    return totalIncome * (bracket.TaxRate / 100m);
                }
            }

            var highestBracket = sortedBrackets.LastOrDefault();
            if (highestBracket != null && totalIncome > highestBracket.MaxIncome)
            {
                return totalIncome * (highestBracket.TaxRate / 100m);
            }

            return totalIncome * (defaultTaxRate / 100m);
        }

        private List<decimal> CalculateTaxableIncome(List<Transaction> incomes, List<TaxBracket> taxBrackets, List<DeductionType> deductions)
        {
            decimal taxableIncome = 0m;
            decimal deductionAmount = 0m;
            List<decimal> taxableIncomeAndDeductionSum = new List<decimal>();
            if (taxBrackets == null || taxBrackets.Count == 0)
            {
                foreach (var transaction in incomes)
                {
                    if (transaction.DeductionTypeId != null)
                    {
                        DeductionType deduction = deductions.Where(d => d.Id == transaction.DeductionTypeId).First();
                        TaxByMonth[transaction.Date.Month-1] += transaction.Amount-deduction.Amount;
                        taxableIncome += transaction.Amount - deduction.Amount;
                        deductionAmount += deduction.Amount;
                    }
                    else
                    {
                        TaxByMonth[transaction.Date.Month-1] += transaction.Amount;
                        taxableIncome += transaction.Amount;
                    }
                }
            }
            else
            {
                var sortedBrackets = taxBrackets.OrderBy(b => b.MinIncome).ToList();
                var smallestBracket = sortedBrackets[0];
                foreach (var transaction in incomes)
                {
                    if(transaction.DeductionTypeId != null && transaction.Amount >= smallestBracket.MinIncome)
                    {
                        DeductionType deduction = deductions.Where(d => d.Id == transaction.DeductionTypeId).First();
                        TaxByMonth[transaction.Date.Month-1] += transaction.Amount-deduction.Amount;
                        taxableIncome += transaction.Amount - deduction.Amount;
                        deductionAmount += deduction.Amount;
                    }
                    else if (transaction.Amount >= smallestBracket.MinIncome)
                    {
                        TaxByMonth[transaction.Date.Month-1] += transaction.Amount;
                        taxableIncome += transaction.Amount;
                    }
                }
            }
            taxableIncomeAndDeductionSum.Add(taxableIncome);
            taxableIncomeAndDeductionSum.Add(deductionAmount);
            return taxableIncomeAndDeductionSum;
        }


        // Charts

        public void TaxPerMonth(List<TaxBracket>brackets)
        {
            scollection[0].Values.Clear();
            foreach(decimal d in TaxByMonth)
            {
                decimal estimatedTaxPerMonth = CalculateEstimatedTax(d, brackets);
                scollection[0].Values.Add(estimatedTaxPerMonth);
            }
        }
        public SeriesCollection scollection { get; } = new SeriesCollection
        {
            new ColumnSeries
            {
                Values = new ChartValues<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            }
        };
        public string[] labels { get; } = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

        public void InsertDataToPieChart(decimal taxableIncome, decimal deductions, decimal tax)
        {
            scollection2[0].Values.Clear();
            scollection2[1].Values.Clear();
            scollection2[2].Values.Clear();
            scollection2[0].Values.Add(taxableIncome);
            scollection2[1].Values.Add(deductions);
            scollection2[2].Values.Add(tax);
        }
        public SeriesCollection scollection2 { get; } = new SeriesCollection
        {
            new PieSeries
            {
                Values = new ChartValues<decimal> {0},
                Title = "Taxable income",
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#3498db")
            },
            new PieSeries
            {
                Values = new ChartValues<decimal> {0},
                Title = "Deductions",
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#f1c40f")

            },
            new PieSeries
            {
                Values = new ChartValues<decimal> {0},
                Title = "Tax",
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#e74c3c")
            }
        };

        //Datagrid
        private bool CheckIfTaxable(decimal amount, List<TaxBracket> brackets)
        {
            if (brackets == null || brackets.Count == 0)
            {
                return true;
            }
            var smallestBracket = brackets.OrderBy(b => b.MinIncome).ToList()[0];
            if (amount >= smallestBracket.MinIncome)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private DataGridData CreateDatagridRowData(string incomeType, decimal amount, Guid? deductionID, List<TaxBracket> brackets, List<DeductionType> deductions)
        {
            DataGridData data = new DataGridData();
            data.Amount = amount;
            if (CheckIfTaxable(amount, brackets))
            {
                data.IsTaxable = true;
                if (deductionID == null)
                {
                    data.IsDeducted = false;
                    data.TaxOwed = Math.Round(CalculateEstimatedTax(amount, brackets), 2);
                }
                else
                {
                    data.IsDeducted = true;
                    DeductionType deduction = deductions.Where(d => d.Id == deductionID).First();
                    data.TaxOwed = Math.Round(CalculateEstimatedTax(amount - deduction.Amount, brackets), 2);
                }
            }
            else
            {
                data.IsTaxable = false;
            }
            if (incomeType == string.Empty)
            {
                data.IncomeType = "Expense";
                data.TaxOwed = 0m;
                data.IsTaxable = false;
            }
            else
            {
                data.IncomeType = incomeType;
            }
            return data;
        }

        //Help class for datagrid

        public class DataGridData
        {
            public string IncomeType { get; set; }
            public decimal Amount { get; set; }
            public bool IsTaxable { get; set; }
            public bool IsDeducted { get; set; }
            public decimal TaxOwed { get; set; }
        }
    }
}