using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class IncomeViewModel : BaseViewModel
    {
        public DateTime IncomeDate { get; set; } = DateTime.Today;
        public List<string> Category = new List<string>();

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private decimal amount;
        public decimal Amount
        {
            get => amount;
            set
            {
                amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public ICommand AddEntryCommand { get; }

        public IncomeViewModel()
        {
            AddEntryCommand = new RelayCommand(AddEntry);
        }

        private async Task AddEntry()
        {

        }
    }
}
