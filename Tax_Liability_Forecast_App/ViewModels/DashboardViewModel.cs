using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public ChartValues<decimal> IncomevsExpenseSeries { get; set; } = new ChartValues<decimal>();
        public ChartValues<decimal> TimeForecast { get; set; } = new ChartValues<decimal>();

        private decimal income;
        private decimal expense;

    }
}
