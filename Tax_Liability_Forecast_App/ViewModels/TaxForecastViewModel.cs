using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class TaxForecastViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        private Client selectedClient;
        public Client SelectedClient
        {
            get => selectedClient;
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
            }
        }

        private decimal totalIncome;
        public decimal TotalIncome
        {
            get => totalIncome;
            set
            {
                totalIncome = value;
                OnPropertyChanged(nameof(TotalIncome));
            }
        }

        private decimal taxableIncome;
        public decimal TaxableIncome
        {
            get => taxableIncome;
            set
            {
                taxableIncome = value;
                OnPropertyChanged(nameof(TaxableIncome));
            }
        }

        private decimal totalDeductions;
        public decimal TotalDeductions
        {
            get => totalDeductions;
            set
            {
                totalDeductions = value;
                OnPropertyChanged(nameof(TotalDeductions));
            }
        }

        private decimal estimatedTax;
        public decimal EstimatedTax
        {
            get => estimatedTax;
            set
            {
                estimatedTax = value;
                OnPropertyChanged(nameof(EstimatedTax));
            }
        }

        public TaxForecastViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
        }
    }
}