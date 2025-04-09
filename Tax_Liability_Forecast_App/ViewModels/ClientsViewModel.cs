using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class ClientsViewModel : BaseViewModel
    {
        public ICommand AddBtnClick { get; }

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

        private void FillList()
        {
            for (int i = 0; i < 25; i++)
            {
                Clients1.Add(new Client { ClientID = i, Name = "JohnDoe" + i });
            }
        }

        async Task AddToList()
        {
            int index = Clients1.Count;
            Clients1.Add(new Client { ClientID = index, Name = "JohnDoe" + index});
            
        }

        public ClientsViewModel()
        {
            AddBtnClick = new RelayCommand(AddToList);
            FillList();
        }
    }
}
