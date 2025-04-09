using Microsoft.EntityFrameworkCore;
using System.Windows;
using Tax_Liability_Forecast_App.DbContexts;
using Tax_Liability_Forecast_App.Services;
using Tax_Liability_Forecast_App.ViewModels;

namespace Tax_Liability_Forecast_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string connectionString = "Data Source=taxforecast.db";
        private readonly IDatabaseService databaseService;
        private readonly NavigationService navigationService;

        public App()
        {
            databaseService = new DatabaseService(new AppDbContextFactory(connectionString));
            navigationService = new NavigationService();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(connectionString).Options;
            using (AppDbContext dbContext = new AppDbContext(options))
            {
                dbContext.Database.Migrate();
            }
            navigationService.CurrentViewModel = new DashboardViewModel();
            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(navigationService, databaseService)
            };
            MainWindow.Show();
            base.OnStartup(e);
        }
    }

}
