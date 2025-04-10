using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Commands
{
    public class TransactionCommand : BaseCommand
    {
        private readonly Func<Transaction, Task> execute;

        public TransactionCommand(Func<Transaction, Task> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override async void Execute(object? parameter)
        {
            if(parameter is Transaction transaction)
            {
                await execute(transaction);
            }
        }
    }
}
