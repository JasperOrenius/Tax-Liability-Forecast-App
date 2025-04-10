using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public SeriesCollection IncomevsExpenseSeries { get; set; } = new SeriesCollection();
        public ChartValues<decimal> TimeForecast { get; set; } = new ChartValues<decimal>();

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
        
        public DashboardViewModel() {
            IncomeText = "0";
            ExpenseText = "0";
            EstimatedTaxText = "0";
            DeadlineText = "0";
        }

    }
}
