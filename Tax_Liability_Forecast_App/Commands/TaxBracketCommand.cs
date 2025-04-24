using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Commands
{
    class TaxBracketCommand : BaseCommand
    {
        private readonly Func<TaxBracket, Task> execute;

        public TaxBracketCommand(Func<TaxBracket, Task> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override async void Execute(object? parameter)
        {
            if(parameter is TaxBracket taxBracket)
            {
                await execute(taxBracket);
            }
        }
    }
}
