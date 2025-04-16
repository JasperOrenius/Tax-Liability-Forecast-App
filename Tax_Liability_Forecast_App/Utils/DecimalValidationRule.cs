using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Tax_Liability_Forecast_App.Utils
{
    public class DecimalValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(string.IsNullOrWhiteSpace(value as string))
            {
                return new ValidationResult(false, "Field can not be null");
            }

            if(decimal.TryParse(value as string, NumberStyles.Number, cultureInfo, out decimal result) && result >= 0)
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Enter a valid decimal number");
        }
    }
}
