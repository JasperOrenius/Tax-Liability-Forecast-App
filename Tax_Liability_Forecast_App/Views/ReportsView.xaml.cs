using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tax_Liability_Forecast_App.Services;
using Tax_Liability_Forecast_App.ViewModels;

namespace Tax_Liability_Forecast_App.Views
{
    /// <summary>
    /// Interaction logic for ReportsView.xaml
    /// </summary>
    public partial class ReportsView : UserControl, IChartRenderer
    {
        public ReportsView()
        {
            InitializeComponent();

            Loaded += ReportsView_Loaded;
        }

        private void ReportsView_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = new ReportsViewModel(((App)Application.Current).DatabaseService);
            viewModel.SetChartRenderer(this);
            DataContext = viewModel;
        }

        public BitmapSource CaptureIncomeExpenseChart()
        {
            return ReportsViewModel.RenderVisualToBitmap(IncomeExpenseChart);
        }

        public BitmapSource CaptureTaxOverTimeChart()
        {
            return ReportsViewModel.RenderVisualToBitmap(TaxOverTimeChart);
        }
    }
}
