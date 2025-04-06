using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tax_Liability_Forecast_App.ViewModels;

namespace Tax_Liability_Forecast_App
{
    public class NavigationService
    {
        public event Action CurrentViewModelChanged;

        private BaseViewModel currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                CurrentViewModelChanged?.Invoke();
            }
        }
    }
}
