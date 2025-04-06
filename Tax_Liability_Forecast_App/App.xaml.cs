using System.Windows;
using Tax_Liability_Forecast_App.ViewModels;

namespace Tax_Liability_Forecast_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationService navigationService;

        public App()
        {
            navigationService = new NavigationService();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            navigationService.CurrentViewModel = new DashboardViewModel();
            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(navigationService)
            };
            MainWindow.Show();
            base.OnStartup(e);
        }
    }

}
