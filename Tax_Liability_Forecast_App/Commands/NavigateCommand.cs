using Tax_Liability_Forecast_App.Services;
using Tax_Liability_Forecast_App.ViewModels;

namespace Tax_Liability_Forecast_App.Commands
{
    public class NavigateCommand : BaseCommand
    {
        private readonly NavigationService navigationService;
        private readonly Func<BaseViewModel> createViewModel;

        public NavigateCommand(NavigationService navigationService, Func<BaseViewModel> createViewModel)
        {
            this.navigationService = navigationService;
            this.createViewModel = createViewModel;
        }

        public override void Execute(object? parameter)
        {
            navigationService.CurrentViewModel = createViewModel();
        }
    }
}
