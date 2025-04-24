using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;


namespace Tax_Liability_Forecast_App.ViewModels
{
    public class TaxSettingsViewModel : BaseViewModel
    {
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ObservableCollection<TaxBracket> TaxBrackets { get; set; }
        public ObservableCollection<DeductionType> DeductionTypes { get; set; }
        public ObservableCollection<TaxDeadline> TaxDeadlines { get; set; }

        // Commands
        public ICommand AddTaxBracketCommand { get; }
        public ICommand EditTaxBracketCommand { get; }
        public ICommand DeleteTaxBracketCommand { get; }

        public ICommand AddDeductionCommand { get; }
        public ICommand EditDeductionCommand { get; }
        public ICommand DeleteDeductionCommand { get; }

        public ICommand AddDeadlineCommand { get; }
        public ICommand EditDeadlineCommand { get; }
        public ICommand DeleteDeadlineCommand { get; }

        public class TaxBracket
        {
            public decimal From { get; set; }
            public decimal To { get; set; }
            public decimal Rate { get; set; }
        }

        public class DeductionType
        {
            public string Name { get; set; }
            public decimal Amount { get; set; }
            public bool IsDeductible { get; set; }
        }

        public class TaxDeadline
        {
            public string Period { get; set; }
            public DateTime DueDate { get; set; }
        }


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
