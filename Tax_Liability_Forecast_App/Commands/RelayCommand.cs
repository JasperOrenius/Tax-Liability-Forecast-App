using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Commands
{
    class RelayCommand : BaseCommand
    {
        private readonly Func<Task> execute;

        public RelayCommand(Func<Task> execute)
        {
            this.execute = execute;
        }

        public override async void Execute(object? parameter)
        {
            if(execute != null)
            {
                await execute();
            }
        }
    }
}
