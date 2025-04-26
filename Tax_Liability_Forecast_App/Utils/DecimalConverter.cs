using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Tax_Liability_Forecast_App.Utils
{
    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString();
            if(string.IsNullOrWhiteSpace(text))
            {
                return 0m;
            }
            text = text.Replace(".", culture.NumberFormat.NumberDecimalSeparator);
            if(decimal.TryParse(text, NumberStyles.Any, culture, out var result))
            {
                return result;
            }
            return Binding.DoNothing;
        }
    }
}
