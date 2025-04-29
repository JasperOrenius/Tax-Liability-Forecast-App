using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Commands
{
    class TaxDeadlineCommand : BaseCommand
    {
        private readonly Func<TaxDeadline, Task> execute;

        public TaxDeadlineCommand(Func<TaxDeadline, Task> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
        public override async void Execute(object? parameter)
        {
            if (parameter is TaxDeadline taxDeadline)
            {
                await execute(taxDeadline);
            }
        }
    }
}
