using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.Commands
{
    class ClientCommand : BaseCommand
    {
        private readonly Func<Client, Task> execute;

        public ClientCommand(Func<Client, Task> execute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is Client client)
            {
                await execute(client);
            }
        }
    }
}
