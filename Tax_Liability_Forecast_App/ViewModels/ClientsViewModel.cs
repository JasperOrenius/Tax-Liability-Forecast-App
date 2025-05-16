using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public ICommand SaveBtnClick { get; }

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
        private ObservableCollection<Client> clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<Client> FilteredClients { get; set; } = new ObservableCollection<Client>();
        private string clientNameInput;
        private string clientEmailInput;
        private string clientPhoneNumInput;

        public string ClientNameInput
        {
            get => clientNameInput;
            set
            {
                clientNameInput = value;
                OnPropertyChanged(nameof(ClientNameInput));
            }
        }
        public string ClientEmailInput
        {
            get => clientEmailInput;
            set
            {
                clientEmailInput = value;
                OnPropertyChanged(nameof(ClientEmailInput));
            }
        }
        public string ClientPhoneNumInput
        {
            get => clientPhoneNumInput;
            set
            {
                clientPhoneNumInput = value;
                OnPropertyChanged(nameof(ClientPhoneNumInput));
            }
        }

        private Client? editingClient;
        public Client? EditingClient
        {
            get => editingClient;
            set
            {
                editingClient = value;
                OnPropertyChanged(nameof(EditingClient));
            }
        }

        private string searchBoxText;
        public string SearchBoxText
        {
            get => searchBoxText;
            set
            {
                searchBoxText = value;
                OnPropertyChanged(nameof(SearchBoxText));
                FilterClients();
            }
        }

        public ClientsViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            AddBtnClick = new RelayCommand(AddBtnClickFunc);
            RemoveBtnClick = new ClientCommand(RemoveBtnClickFunc);
            EditBtnClick = new ClientCommand(EditBtnClickFunc);
            SaveBtnClick = new ClientCommand(SaveBtnClickFunc);
            EditBtnText = "Edit";
            FetchTable();
        }

        private async void FetchTable()
        {
            clients.Clear();
            try
            {
                var result = await databaseService.FetchClientTable();
                clients = new ObservableCollection<Client>(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            FilterClients();
        }

        async Task AddBtnClickFunc()
        {
            Regex EmailValidation = new Regex("^\\S+@\\S+\\.\\S+$");
            Regex PhoneNumValidation = new Regex("^\\+?\\d{1,4}?[-.\\s]?\\(?\\d{1,3}?\\)?[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,9}$");
            if (string.IsNullOrEmpty(ClientNameInput) || string.IsNullOrEmpty(ClientEmailInput) || string.IsNullOrEmpty(ClientPhoneNumInput))
            {
                MessageBox.Show("Please fill all fields!!!");
                return;
            }
            else if (!EmailValidation.IsMatch(ClientEmailInput))
            {
                MessageBox.Show("Email address not valid!!!");
                return;
            }
            else if (!PhoneNumValidation.IsMatch(ClientPhoneNumInput))
            {
                MessageBox.Show("Invalid phone number!!!");
                return;
            }

            Client client = new Client() {Name = ClientNameInput, Email = ClientEmailInput, PhoneNum = ClientPhoneNumInput};
            client.Id = Guid.NewGuid();
            var result = MessageBox.Show($"Are you sure that you want to add a new client with these values; \nName: {client.Name} \nEmail: {client.Email} \nPhone number: {client.PhoneNum}", "Note!!!", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                await databaseService.AddClient(client);
                ClientNameInput = string.Empty;
                ClientEmailInput = string.Empty;
                ClientPhoneNumInput = string.Empty;
                FetchTable();
            }
            else if (result == MessageBoxResult.No)
            {
                ClientNameInput = string.Empty;
                ClientEmailInput = string.Empty;
                ClientPhoneNumInput = string.Empty;
                FetchTable();
                return;
            }
        }

        async Task RemoveBtnClickFunc(Client client)
        {
            try
            {
                await databaseService.RemoveClient(client);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            FetchTable();
        }

        private bool clicked = true;
        private async Task EditBtnClickFunc(Client client)
        {
            if(EditingClient == null)
            {
                client.IsEditing = true;
                EditingClient = client;
            }
        }

        private async Task SaveBtnClickFunc(Client client)
        {
            client.IsEditing = false;
            EditingClient = null;
            await databaseService.UpdateClient(client);
            FetchTable();
        }

        private void FilterClients()
        {
            List<Client> filteredClients = clients.Where(c => string.IsNullOrEmpty(searchBoxText) || c.Name.Contains(searchBoxText, StringComparison.OrdinalIgnoreCase)).ToList();
            FilteredClients.Clear();
            foreach (Client client in filteredClients)
            {
                FilteredClients.Add(client);
            }
        }
    }
}
