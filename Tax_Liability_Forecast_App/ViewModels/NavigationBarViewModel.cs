using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class NavigationBarViewModel : BaseViewModel
    {
        public ICommand Dashboard { get; }
        public ICommand Clients { get; }
        public ICommand Income { get; }
        public ICommand Expenses { get; }
        public ICommand TaxForecast { get; }
        public ICommand TaxSettings { get; }
        public ICommand Reports { get; }

        public ICommand ToggleNavigationBar { get; }

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public NavigationBarViewModel(NavigationService navigationService, Func<DashboardViewModel> createDashboardViewModel, Func<ClientsViewModel> createClientsViewModel, Func<IncomeViewModel> createIncomeViewModel, Func<ExpensesViewModel> createExpensesViewModel, Func<TaxForecastViewModel> createTaxForecastViewModel, Func<TaxSettingsViewModel> createTaxSettingsViewModel, Func<ReportsViewModel> createReportsViewModel)
        {
            Dashboard = new NavigateCommand(navigationService, createDashboardViewModel);
            Clients = new NavigateCommand(navigationService, createClientsViewModel);
            Income = new NavigateCommand(navigationService, createIncomeViewModel);
            Expenses = new NavigateCommand(navigationService, createExpensesViewModel);
            TaxForecast = new NavigateCommand(navigationService, createTaxForecastViewModel);
            TaxSettings = new NavigateCommand(navigationService, createTaxSettingsViewModel);
            Reports = new NavigateCommand(navigationService, createReportsViewModel);

            ToggleNavigationBar = new RelayCommand(SetIsExpanded);
        }

        private async Task SetIsExpanded()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
