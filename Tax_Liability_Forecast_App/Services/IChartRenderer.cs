using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Tax_Liability_Forecast_App.Services
{
    public interface IChartRenderer
    {
        BitmapSource CaptureIncomeExpenseChart();
        BitmapSource CaptureTaxOverTimeChart();
    }
}
