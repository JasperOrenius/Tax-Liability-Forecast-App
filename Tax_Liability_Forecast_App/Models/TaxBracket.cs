using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tax_Liability_Forecast_App.Models
{
    public class TaxBracket : INotifyPropertyChanged
    {
        [Key]
        public Guid Id { get; set; }
        public decimal MinIncome { get; set; }
        public decimal MaxIncome { get; set; }
        public decimal TaxRate { get; set; }

        [NotMapped]
        public bool IsEmpty { get; set; }

        [NotMapped]
        private bool isEditing;
        [NotMapped]
        public bool IsEditing
        {
            get => isEditing || IsEmpty;
            set
            {
                isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(CanDisplayIsEditing));
                OnPropertyChanged(nameof(CanDisplayNotEditing));
            }
        }

        [NotMapped]
        public bool CanDisplayIsEditing => IsEditing && !IsEmpty;
        [NotMapped]
        public bool CanDisplayNotEditing => !IsEditing && !IsEmpty;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
