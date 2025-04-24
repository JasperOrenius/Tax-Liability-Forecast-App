using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;


namespace Tax_Liability_Forecast_App.ViewModels
{
    public class TaxSettingsViewModel : BaseViewModel
    {
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ObservableCollection<TaxBracket> TaxBrackets { get; set; } = new ObservableCollection<TaxBracket>();
        public ObservableCollection<DeductionType> DeductionTypes { get; set; } = new ObservableCollection<DeductionType>();
        public ObservableCollection<TaxDeadline> TaxDeadlines { get; set; } = new ObservableCollection<TaxDeadline>();

        // Commands
        public ICommand AddTaxBracketCommand { get; }
        public ICommand EditTaxBracketCommand { get; }
        public ICommand SaveTaxBracketCommand { get; }
        public ICommand DeleteTaxBracketCommand { get; }

        public ICommand AddDeductionCommand { get; }
        public ICommand EditDeductionCommand { get; }
        public ICommand SaveDeductionCommand { get; }
        public ICommand DeleteDeductionCommand { get; }

        public ICommand AddDeadlineCommand { get; }
        public ICommand EditDeadlineCommand { get; }
        public ICommand SaveDeadlineCommand { get; }
        public ICommand DeleteDeadlineCommand { get; }


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
            DeleteTaxBracketCommand = new RelayCommand(TaxSettingDelete);
            //AddTaxBracketDummyRow();
        }

        void AddTaxBracketDummyRow()
        {
            
        }

        async Task TaxSettingSave()
        {
            


        }
        async Task TaxSettingDelete()
        {
            MessageBox.Show("Test");
        }
    }
}
