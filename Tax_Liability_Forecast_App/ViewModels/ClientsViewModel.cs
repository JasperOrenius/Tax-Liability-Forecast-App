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

        private string editBtnText;
        public string EditBtnText
        {
            get => editBtnText;
            set
            {
                editBtnText = value;
                OnPropertyChanged(nameof(EditBtnText));
            }
        }

        private ObservableCollection<Client> clients = new ObservableCollection<Client>();
        public ObservableCollection<Client> Clients
        {
            get => clients;
            set
            {
                clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public ClientsViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            AddBtnClick = new ClientCommand(AddBtnClickFunc);
            RemoveBtnClick = new ClientCommand(RemoveBtnClickFunc);
            EditBtnClick = new RelayCommand(EditBtnClickFunc);
            EditBtnText = "Edit";
            FetchTable();
        }

        private async void FetchTable()
        {
            Clients.Clear();
            var result = await databaseService.FetchClientTable();
            Clients = new ObservableCollection<Client>(result);
        }

        async Task AddBtnClickFunc(Client client)
        {

            //Client client = new Client() {Name = client1.Name, Email = client1.Email, PhoneNum = client1.PhoneNum };
            client.Id = Guid.NewGuid();
            var result = MessageBox.Show($"Are you sure that you want to add a new client with these values; \nName: {client.Name} \nEmail: {client.Email} \nPhone number: {client.PhoneNum}", "Note!!!", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                await databaseService.AddClient(client);
                FetchTable();
            }
            else if (result == MessageBoxResult.No)
            {
                FetchTable();
                return;
            }
        }

        async Task RemoveBtnClickFunc(Client client)
        {
            await databaseService.RemoveClient(client);
            FetchTable();
        }

        private bool clicked = true;
        private async Task EditBtnClickFunc()
        {
            if (clicked)
            {
                editBtnText = "Save";
                clicked = false;
            }
            else
            {
                editBtnText = "Edit";
                clicked = true;
            }

            FetchTable();
        }
    }
}
