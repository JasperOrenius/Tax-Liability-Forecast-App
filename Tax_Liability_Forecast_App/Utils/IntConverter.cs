using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tax_Liability_Forecast_App.Utils
{
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if(string.IsNullOrEmpty(str))
            {
                return 0;
            }
            if(int.TryParse(str, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}
