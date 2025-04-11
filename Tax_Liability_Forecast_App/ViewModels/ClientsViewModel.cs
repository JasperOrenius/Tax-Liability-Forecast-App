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
    public class ClientsViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;
        public ICommand AddBtnClick { get; }
        public ICommand RemoveBtnClick { get; }
        public ICommand EditBtnClick { get; }

        private ObservableCollection<Client> clients1 = new ObservableCollection<Client>();
        public ObservableCollection<Client> Clients1
        {
            get => clients1;
            set
            {
                clients1 = value;
                OnPropertyChanged(nameof(Clients1));
            }
        }

        public ClientsViewModel(IDatabaseService databaseService)
        {
            AddBtnClick = new RelayCommand(AddBtnClickFunc);
            RemoveBtnClick = new RelayCommand(RemoveBtnClickFunc);
            FetchTable();
        }

        private void FetchTable()
        {
            //Clients1 = new ObservableCollection<Client>(databaseService.FetchClientTable());
        }

        async Task AddBtnClickFunc()
        {
            
        }

        async Task RemoveBtnClickFunc()
        {
            

        }
    }
}
