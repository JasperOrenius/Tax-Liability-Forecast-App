using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            this.databaseService = databaseService;
            AddBtnClick = new RelayCommand(AddBtnClickFunc);
            RemoveBtnClick = new ClientCommand(RemoveBtnClickFunc);
            EditBtnClick = new RelayCommand(EditBtnClickFunc);
            FetchTable();
        }

        private async void FetchTable()
        {
            Clients1.Clear();
            var result = await databaseService.FetchClientTable();
            Clients1 = new ObservableCollection<Client>(result);
        }

        async Task AddBtnClickFunc()
        {
            Client client = new Client() {Id = Guid.NewGuid(), Name = "John Doe", Email = "johndoe@gmial.com", PhoneNum = "1234567890" };
            await databaseService.AddClient(client);
            FetchTable();
        }

        async Task RemoveBtnClickFunc(Client client)
        {
            await databaseService.RemoveClient(Clients1[0]);
            FetchTable();
        }

        private async Task EditBtnClickFunc()
        {
            MessageBox.Show("salkjdlsa");
        }
    }
}
