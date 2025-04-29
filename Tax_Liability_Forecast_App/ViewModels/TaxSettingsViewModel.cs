using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Tax_Liability_Forecast_App.Commands;
using Tax_Liability_Forecast_App.Models;
using Tax_Liability_Forecast_App.Services;


namespace Tax_Liability_Forecast_App.ViewModels
{
    public class TaxSettingsViewModel : BaseViewModel
    {
        private readonly IDatabaseService databaseService;

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

        private TaxBracket? editingTaxBracket;
        private TaxBracket? EditingTaxBracket
        {
            get => editingTaxBracket;
            set
            {
                editingTaxBracket = value;
                OnPropertyChanged(nameof(EditingTaxBracket));
            }
        }
        private DeductionType? editingDeductionType;
        private DeductionType? EditingDeductionType
        {
            get => editingDeductionType;
            set
            {
                editingDeductionType = value;
                OnPropertyChanged(nameof(EditingDeductionType));
            }
        }

        private TaxDeadline? editingTaxDeadLine;
        private TaxDeadline? EditingTaxDeadLine
        {
            get => editingTaxDeadLine;
            set
            {
                editingTaxDeadLine = value;
                OnPropertyChanged(nameof(EditingTaxDeadLine));
            }
        }


        public TaxSettingsViewModel(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
            AddTaxBracketCommand = new TaxBracketCommand(AddTaxBracket);
            EditTaxBracketCommand = new TaxBracketCommand(EditTaxBracket);
            SaveTaxBracketCommand = new TaxBracketCommand(SaveTaxBracket);
            DeleteTaxBracketCommand = new TaxBracketCommand(DeleteTaxBracket);

            AddDeductionCommand = new DeductionTypeCommand(AddDeductionTypes);
            EditDeductionCommand = new DeductionTypeCommand(EditDeductionTypes);
            SaveDeductionCommand = new DeductionTypeCommand(SaveDeductionTypes);
            DeleteDeductionCommand = new DeductionTypeCommand(DeleteDeductionTypes);
            LoadRows();
        }

        private async Task LoadRows()
        {
            var taxBrackets = await databaseService.FetchAllTaxBrackets();
            var deductionTypes = await databaseService.FetchAllDeductionTypes();
            var taxDeadLines = await databaseService.FetchAllTaxDeadLines();
            TaxBrackets.Clear();
            DeductionTypes.Clear();
            TaxDeadlines.Clear();
            foreach(var bracket in taxBrackets)
            {
                TaxBrackets.Add(bracket);
            }
            foreach(var bracket in deductionTypes)
            {
                DeductionTypes.Add(bracket);
            }
            foreach(var bracket in taxDeadLines)
            {
                TaxDeadlines.Add(bracket);
            }
            AddEmptyRows();
        }

        void AddEmptyRows()
        {
            TaxBrackets.Add(new TaxBracket { IsEmpty = true });
            DeductionTypes.Add(new DeductionType { IsEmpty = true });
            TaxDeadlines.Add(new TaxDeadline { IsEmpty = true, DueDate = DateTime.Today });
        }

        async Task AddTaxBracket(TaxBracket taxBracket)
        {
            if (taxBracket.MinIncome > taxBracket.MaxIncome || taxBracket.TaxRate is < 0 or > 100) return;
            var empty = TaxBrackets.FirstOrDefault(t => t.IsEmpty);
            if(empty != null)
            {
                TaxBrackets.Remove(empty);
            }
            var newBracket = new TaxBracket
            {
                Id = Guid.NewGuid(),
                MinIncome = taxBracket.MinIncome,
                MaxIncome = taxBracket.MaxIncome,
                TaxRate = taxBracket.TaxRate,
                IsEmpty = false,
            };
            await databaseService.CreateTaxBracket(newBracket);
            TaxBrackets.Add(newBracket);
            TaxBrackets.Add(new TaxBracket { IsEmpty = true });
        }

        async Task EditTaxBracket(TaxBracket taxBracket)
        {
            if(EditingTaxBracket == null)
            {
                taxBracket.IsEditing = true;
                EditingTaxBracket = taxBracket;
            }
        }

        async Task SaveTaxBracket(TaxBracket taxBracket)
        {
            taxBracket.IsEditing = false;
            EditingTaxBracket = null;
            await databaseService.UpdateTaxBracket(taxBracket);
            await LoadRows();
        }

        async Task DeleteTaxBracket(TaxBracket taxBracket)
        {
            EditingTaxBracket = null;
            TaxBrackets.Remove(taxBracket);
            await databaseService.RemoveTaxBracket(taxBracket);
            await LoadRows();
        }
        async Task AddDeductionTypes(DeductionType deductionType)
        {
            if (deductionType.Amount < 0 || deductionType.Name == null) return;
            var empty = DeductionTypes.FirstOrDefault(t => t.IsEmpty);
            if (empty != null)
            {
                DeductionTypes.Remove(empty);
            }
            var newDeduction = new DeductionType
            {
                Id = Guid.NewGuid(),
                Name = deductionType.Name,
                Amount = deductionType.Amount,
                IsDeductible = deductionType.IsDeductible,
                IsEmpty = false
            };
            await databaseService.CreateDeductionType(newDeduction);
            DeductionTypes.Add(newDeduction);
            DeductionTypes.Add(new DeductionType { IsEmpty = true });
        }
        async Task EditDeductionTypes(DeductionType deductionType)
        {
            if(editingDeductionType == null)
            {
                deductionType.IsEditing = true;
                EditingDeductionType = deductionType;
            }
        }
        async Task SaveDeductionTypes(DeductionType deductionType)
        {
            deductionType.IsEditing = false;
            EditingDeductionType = null;
            await databaseService.UpdateDeductionType(deductionType);
            await LoadRows();

        }
        async Task DeleteDeductionTypes(DeductionType deductionType)
        {
            EditingDeductionType = null;
            DeductionTypes.Remove(deductionType);
            await databaseService.RemoveDeductionType(deductionType);
            await LoadRows();
        }
    }
}
