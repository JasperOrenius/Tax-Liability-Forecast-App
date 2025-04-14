using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;

namespace Tax_Liability_Forecast_App.ViewModels
{
    public class TaxSettingsViewModel : BaseViewModel
    {
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        public List<string> taxDeductionList = new List<string>
        {
            "Työmatkakulut",
            "Kotitalousvähennys",
            "Tulonhankkimisvähennys",
            "Asunnon ja työpaikan välinen matka",
            "Opintolainavähennys",
            "Ansiotulovähennys",
            "Elatusvelvollisuusvähennys",
            "Jäsenmaksut",
            "Lahjoitusvähennys"
        };
        public List<string> TaxDeductionList
        {
            get => taxDeductionList;
            set
            {
                taxDeductionList = value;
                OnPropertyChanged(nameof(TaxDeductionList));
            }
        }

        public TaxSettingsViewModel()
        {
            SaveCommand = new RelayCommand(TaxSettingSave);
            DeleteCommand = new RelayCommand(TaxSettingDelete);
        }
        async Task TaxSettingSave()
        {
            


        }
        async Task TaxSettingDelete()
        {

        }
    }
}
