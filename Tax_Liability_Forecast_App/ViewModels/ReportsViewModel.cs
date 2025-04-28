using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

        public ObservableCollection<Transaction> Incomes { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Transaction> Expenses { get; set; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        public List<String> TransactionTypes { get; } = new List<String> { "All", "Income", "Expense" };

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

        private decimal totalExpenses;
        public decimal TotalExpenses
        {
            get => totalExpenses;
            set
            {
                totalExpenses = value;
                OnPropertyChanged(nameof(TotalExpenses));
            }
        }

        private decimal netIncome;
        public decimal NetIncome
        {
            get => netIncome;
            set
            {
                netIncome = value;
                OnPropertyChanged(nameof(NetIncome));
            }
        }

        private decimal estimatedTax;
        public decimal EstimatedTax
        {
            get => estimatedTax;
            set
            {
                NetIncome = value;
                OnPropertyChanged(nameof(EstimatedTax));
            }
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToPDFCommand { get; }

        public ReportsViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportToPDFCommand = new RelayCommand(ExportToPDF);
            LoadClients();
        }

        private async Task LoadClients()
        {
            var clients = await databaseService.FetchClientTable();
            Clients.Clear();
            Clients = new ObservableCollection<Client>(clients);
            if(Clients.Count > 0)
            {
                SelectedClient = Clients[0];
            }
        }

        private async Task GenerateReport()
        {
            if (SelectedClient == null) return;
            var transactions = await databaseService.GetTransactionsByClientId(SelectedClient.Id);
            Incomes.Clear();
            Expenses.Clear();
            foreach(var transaction in transactions)
            {
                if(transaction.Type == TransactionType.Income)
                {
                    Incomes.Add(transaction);
                }
                else
                {
                    Expenses.Add(transaction);
                }
            }
            TotalIncome = Incomes.Sum(i => i.Amount);
            TotalExpenses = Expenses.Sum(e => e.Amount);
        }

        private async Task ExportToPDF()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                DefaultExt = ".pdf"
            };
            if(saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                MessageBox.Show($"File saved at {filePath}");
            }
            else
            {
                MessageBox.Show("File save operation was cancelled");
            }
        }
    }
}