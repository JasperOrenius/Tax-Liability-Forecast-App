using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Utils
{
    public interface IChartFactory
    {
        public byte[] CreateIncomeExpensePieChart(Dictionary<string, decimal> data, double availableWidth, double? availableHeight = null);
        public byte[] CreateTaxOverTimeLineChart(List<(DateTime month, decimal tax, decimal net)> points, double availableWidth, double? availableHeight = null);
    }
}
