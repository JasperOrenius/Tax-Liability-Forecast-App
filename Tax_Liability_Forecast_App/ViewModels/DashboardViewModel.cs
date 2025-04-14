using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;


namespace Tax_Liability_Forecast_App.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public SeriesCollection IncomevsExpenseSeries { get; set; } = new SeriesCollection();

        List<Transaction> IncomeList { get; set; } = new List<Transaction>();
        List<Transaction> ExpenseList { get; set; } = new List<Transaction>();
        List<Transaction> TransactionList { get; set; } = new List<Transaction>();
        public ChartValues<decimal> TimeForecast { get; set; } = new ChartValues<decimal>();
        private readonly IDatabaseService databaseService;

        private decimal income;
        private decimal expense;
        private string incomeText;
        private string expenseText;
        private string estimatedTaxText;
        private string deadlineText;
        public string IncomeText
        {
            get => incomeText;
            set
            {
                incomeText = value;
                OnPropertyChanged(nameof(IncomeText));
            }
        }
        public string ExpenseText
        {
            get => expenseText;
            set
            {
                expenseText = value;
                OnPropertyChanged(nameof(ExpenseText));
            }
        }
        public string EstimatedTaxText
        {
            get => estimatedTaxText;
            set
            {
                estimatedTaxText = value;
                OnPropertyChanged(nameof(EstimatedTaxText));
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
        
        public DashboardViewModel(IDatabaseService databaseService) {
            this.databaseService = databaseService;
            TransactionList.Clear();
            LoadExpenses();
            LoadIncomes();
            
            ExpenseText = "0";
            EstimatedTaxText = "0";
            DeadlineText = "0";
            CalculateExpenseIncome();
            
        }
        private async Task LoadIncomes()
        {

            var incomes = await databaseService.GetAllTransactions();
            IncomeList.Clear();
            foreach(var income in incomes)
            {
                if(income.Type == TransactionType.Income)
                {

                    IncomeList.Add(income);
                    TransactionList.Add(income);
                }
            }
        }
        private async Task LoadExpenses()
        {
            var expenses = await databaseService.GetAllTransactions();
            ExpenseList.Clear();
            foreach(var expense in expenses)
            {
                if(expense.Type == TransactionType.Expense)
                {
                    ExpenseList.Add(expense);
                    TransactionList.Add(expense);
                }
            }
        }
        private void CalculateExpenseIncome()
        {

            IncomeText = IncomeList.Where(t => t.Type.ToString() == "Income").Sum(t => t.Amount).ToString();
            ExpenseText = ExpenseList.Where(t => t.Type.ToString() == "Expense").Sum(t => t.Amount).ToString();

            var transactionGroup = TransactionList.GroupBy(t => t.Type).Select(g => new {Type = g.Key, Total = g.Sum(t => t.Amount)}).ToList();
            foreach(var group in transactionGroup)
            {
                IncomevsExpenseSeries.Add(new PieSeries
                {
                    Title = group.Type.ToString(),
                    Values = new ChartValues<decimal> { group.Total}

                });
            }

        }

    }
}
