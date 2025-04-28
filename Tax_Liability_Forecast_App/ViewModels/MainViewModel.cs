using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly NavigationService navigationService;

        public BaseViewModel CurrentViewModel => navigationService.CurrentViewModel;

        public NavigationBarViewModel NavigationBarViewModel { get; }

        public MainViewModel(NavigationService navigationService, IDatabaseService databaseService)
        {
            this.navigationService = navigationService;
            this.navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            NavigationBarViewModel = new NavigationBarViewModel(
                navigationService,
                () => new DashboardViewModel(databaseService),
                () => new ClientsViewModel(databaseService),
                () => new IncomeViewModel(databaseService),
                () => new ExpensesViewModel(databaseService),
                () => new TaxForecastViewModel(databaseService),
                () => new TaxSettingsViewModel(databaseService),
                () => new ReportsViewModel(databaseService)
                );
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}